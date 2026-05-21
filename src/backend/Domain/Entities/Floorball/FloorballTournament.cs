using Domain.Enums.Floorball;
using Domain.ValueObjects.Floorball;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a floorball tournament with group stages and optional playoff bracket
/// </summary>
public class FloorballTournament : FloorballCompetition
{
    /// <summary>
    /// Gets the HTML content describing the tournament (rendered via Quill editor)
    /// </summary>
    public string? ContentHtml { get; private set; }

    /// <summary>
    /// Gets the primary venue for the tournament
    /// </summary>
    public string? Venue { get; private set; }

    /// <summary>
    /// Gets the current lifecycle status of the tournament
    /// </summary>
    public FloorballTournamentStatus TournamentStatus { get; private set; }

    /// <summary>
    /// Gets the tournament-specific rules (group/playoff match rules, advancement, bracket)
    /// </summary>
    public FloorballTournamentRules TournamentRules { get; private set; }

    /// <summary>
    /// Gets the team that won the playoff final (null until the final has been completed).
    /// </summary>
    public Guid? ChampionTeamId { get; private set; }

    /// <summary>
    /// Gets the groups within this tournament
    /// </summary>
    public IReadOnlyCollection<FloorballTournamentGroup> Groups => _groups.AsReadOnly();
    private readonly List<FloorballTournamentGroup> _groups = new();

    /// <summary>
    /// Optional pre-defined playoff bracket schedule. Each slot pins a (round, order) position
    /// in the bracket to a specific kickoff time / venue. When the playoff stage is started,
    /// <see cref="Application.Features.Floorball.Tournaments.Handlers.StartTournamentPlayoffStageHandler"/>
    /// honors these slots instead of auto-calculating "next day at 16:00 UTC + 2-hour offsets".
    /// Empty list = no pre-defined schedule = fall back to auto-scheduling.
    /// </summary>
    public IReadOnlyCollection<PlayoffScheduleSlot> PlayoffSchedule => PlayoffScheduleBacking.AsReadOnly();

    // Not readonly: EF Core assigns this backing field directly when hydrating from the
    // JSON column and may leave it null for older rows where PlayoffSchedule IS NULL.
    private List<PlayoffScheduleSlot> _playoffSchedule = new();

    private List<PlayoffScheduleSlot> PlayoffScheduleBacking
    {
        get
        {
            _playoffSchedule ??= new List<PlayoffScheduleSlot>();
            return _playoffSchedule;
        }
    }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballTournament() : base()
    {
        TournamentStatus = FloorballTournamentStatus.Draft;
        TournamentRules = FloorballTournamentRules.Default();
        _groups = new List<FloorballTournamentGroup>();
        _playoffSchedule = new List<PlayoffScheduleSlot>();
    }

    public FloorballTournament(
        string name,
        DateTime startDate,
        DateTime endDate,
        string? venue = null,
        string? contentHtml = null,
        FloorballTournamentRules? tournamentRules = null,
        IEnumerable<PlayoffScheduleSlot>? playoffSchedule = null)
        : base(name, startDate, endDate, tournamentRules?.GroupStageMatchRules)
    {
        Venue = venue;
        ContentHtml = contentHtml;
        TournamentStatus = FloorballTournamentStatus.Draft;
        TournamentRules = tournamentRules ?? FloorballTournamentRules.Default();
        _groups = new List<FloorballTournamentGroup>();
        _playoffSchedule = new List<PlayoffScheduleSlot>();
        if (playoffSchedule != null)
        {
            SetPlayoffSchedule(playoffSchedule);
        }
    }

    /// <summary>
    /// Replaces the entire pre-defined playoff schedule with the provided slots.
    /// Pass an empty enumerable to clear the schedule.
    /// </summary>
    /// <param name="slots">The new schedule. Duplicate (Round, Order) entries are rejected.</param>
    public void SetPlayoffSchedule(IEnumerable<PlayoffScheduleSlot> slots)
    {
        ArgumentNullException.ThrowIfNull(slots);
        if (TournamentStatus == FloorballTournamentStatus.PlayoffStage
            || TournamentStatus == FloorballTournamentStatus.Completed)
        {
            throw new InvalidOperationException("Cannot change the playoff schedule once the playoff stage has started.");
        }

        List<PlayoffScheduleSlot> materialized = slots.ToList();
        HashSet<(FloorballPlayoffRound, int)> seen = new();
        foreach (PlayoffScheduleSlot slot in materialized)
        {
            ArgumentNullException.ThrowIfNull(slot);
            if (!seen.Add((slot.Round, slot.Order)))
            {
                throw new InvalidOperationException($"Playoff schedule contains a duplicate slot for {slot.Round} #{slot.Order}.");
            }
        }

        List<PlayoffScheduleSlot> backing = PlayoffScheduleBacking;
        backing.Clear();
        backing.AddRange(materialized);
    }

    public void UpdateContent(string? contentHtml)
    {
        ContentHtml = contentHtml;
    }

    public void UpdateVenue(string? venue)
    {
        Venue = venue;
    }

    public void UpdateTournamentRules(FloorballTournamentRules rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        if (TournamentStatus == FloorballTournamentStatus.Completed)
            throw new InvalidOperationException("Cannot update rules for a completed tournament.");
        // Once matches are scheduled the match rules (period count, OT, etc.) are snapshotted into
        // each FloorballMatch. Allowing tournament rule edits after that point would produce a
        // confusing split between the tournament's "current" rules and historical match rules.
        if (_matches.Count > 0)
            throw new InvalidOperationException("Cannot change tournament rules once matches have been created.");

        TournamentRules = rules;
        UpdateMatchRules(rules.GroupStageMatchRules);
    }

    public void StartGroupStage()
    {
        if (TournamentStatus != FloorballTournamentStatus.Draft)
            throw new InvalidOperationException($"Cannot start group stage when status is {TournamentStatus}.");
        if (_groups.Count == 0)
            throw new InvalidOperationException("Cannot start group stage without any groups defined.");

        foreach (FloorballTournamentGroup g in _groups)
        {
            if (g.Teams.Count < 2)
                throw new InvalidOperationException($"Cannot start group stage: group '{g.Name}' must have at least 2 teams.");
        }

        TournamentStatus = FloorballTournamentStatus.GroupStage;
        Activate();
    }

    public void StartPlayoffStage()
    {
        if (TournamentStatus != FloorballTournamentStatus.GroupStage)
            throw new InvalidOperationException($"Cannot start playoff stage when status is {TournamentStatus}.");
        if (!TournamentRules.HasPlayoffStage)
            throw new InvalidOperationException("This tournament does not have a playoff stage.");

        TournamentStatus = FloorballTournamentStatus.PlayoffStage;
    }

    public void CompleteTournament()
    {
        if (TournamentStatus != FloorballTournamentStatus.PlayoffStage &&
            TournamentStatus != FloorballTournamentStatus.GroupStage)
            throw new InvalidOperationException($"Cannot complete tournament when status is {TournamentStatus}.");

        TournamentStatus = FloorballTournamentStatus.Completed;
        Complete();
    }

    /// <summary>
    /// Records the playoff champion. Called automatically when the final is completed.
    /// Idempotent: re-setting to the same team is allowed.
    /// </summary>
    public void SetChampion(Guid championTeamId)
    {
        if (championTeamId == Guid.Empty)
            throw new ArgumentException("Champion team id cannot be empty.", nameof(championTeamId));

        ChampionTeamId = championTeamId;
    }

    public void CancelTournament()
    {
        if (TournamentStatus == FloorballTournamentStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed tournament.");
        if (TournamentStatus == FloorballTournamentStatus.Cancelled)
            throw new InvalidOperationException("Tournament is already cancelled.");

        TournamentStatus = FloorballTournamentStatus.Cancelled;
        Deactivate();
    }

    public FloorballTournamentGroup AddGroup(string name)
    {
        if (TournamentStatus == FloorballTournamentStatus.Completed)
            throw new InvalidOperationException("Cannot add groups to a completed tournament.");

        int nextOrder = _groups.Count;
        FloorballTournamentGroup group = new(Id, name, nextOrder);
        _groups.Add(group);
        return group;
    }

    public void RemoveGroup(Guid groupId)
    {
        if (TournamentStatus == FloorballTournamentStatus.Completed)
            throw new InvalidOperationException("Cannot remove groups from a completed tournament.");

        FloorballTournamentGroup? group = _groups.FirstOrDefault(g => g.Id == groupId);
        if (group != null)
            _groups.Remove(group);
    }

    public FloorballTournamentGroup? GetGroup(Guid groupId)
    {
        return _groups.FirstOrDefault(g => g.Id == groupId);
    }
}
