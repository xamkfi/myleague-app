using Domain.Enums.Common;
using Domain.Enums.Football;
using Domain.ValueObjects.Football;

namespace Domain.Entities.Football.Competitions;

/// <summary>
/// A football tournament with group stages and an optional knockout bracket.
/// </summary>
public class FootballTournament : FootballCompetition
{
    public string? ContentHtml { get; private set; }
    public string? Venue { get; private set; }
    public FootballTournamentStatus TournamentStatus { get; private set; }
    public FootballTournamentRules TournamentRules { get; private set; }
    public Guid? ChampionTeamId { get; private set; }
    public IReadOnlyCollection<FootballTournamentGroup> Groups => _groups.AsReadOnly();
    private readonly List<FootballTournamentGroup> _groups = new();
    public IReadOnlyCollection<FootballPlayoffScheduleSlot> PlayoffSchedule => PlayoffScheduleBacking.AsReadOnly();

    private List<FootballPlayoffScheduleSlot> _playoffSchedule = new();

    private List<FootballPlayoffScheduleSlot> PlayoffScheduleBacking
    {
        get
        {
            _playoffSchedule ??= new List<FootballPlayoffScheduleSlot>();
            return _playoffSchedule;
        }
    }

    private FootballTournament() : base()
    {
        TournamentStatus = FootballTournamentStatus.Draft;
        TournamentRules = FootballTournamentRules.Default();
    }

    public FootballTournament(
        string name,
        DateTime startDate,
        DateTime endDate,
        string? venue = null,
        string? contentHtml = null,
        FootballTournamentRules? tournamentRules = null,
        IEnumerable<FootballPlayoffScheduleSlot>? playoffSchedule = null,
        TeamCategory teamCategory = TeamCategory.Adult)
        : base(name, startDate, endDate, tournamentRules?.GroupStageMatchRules, FootballStandingRules.Default(), teamCategory)
    {
        Venue = venue;
        ContentHtml = contentHtml;
        TournamentStatus = FootballTournamentStatus.Draft;
        TournamentRules = tournamentRules ?? FootballTournamentRules.Default();
        if (playoffSchedule != null)
            SetPlayoffSchedule(playoffSchedule);
    }

    public void SetPlayoffSchedule(IEnumerable<FootballPlayoffScheduleSlot> slots)
    {
        ArgumentNullException.ThrowIfNull(slots);
        if (TournamentStatus == FootballTournamentStatus.PlayoffStage
            || TournamentStatus == FootballTournamentStatus.Completed)
        {
            throw new InvalidOperationException("Cannot change the playoff schedule once the playoff stage has started.");
        }

        List<FootballPlayoffScheduleSlot> materialized = slots.ToList();
        HashSet<(FootballPlayoffRound, int)> seen = new();
        foreach (FootballPlayoffScheduleSlot slot in materialized)
        {
            ArgumentNullException.ThrowIfNull(slot);
            if (!seen.Add((slot.Round, slot.Order)))
                throw new InvalidOperationException($"Playoff schedule contains a duplicate slot for {slot.Round} #{slot.Order}.");
        }

        List<FootballPlayoffScheduleSlot> backing = PlayoffScheduleBacking;
        backing.Clear();
        backing.AddRange(materialized);
    }

    public void UpdateContent(string? contentHtml) => ContentHtml = contentHtml;
    public void UpdateVenue(string? venue) => Venue = venue;

    public void UpdateTournamentRules(FootballTournamentRules rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        if (TournamentStatus == FootballTournamentStatus.Completed)
            throw new InvalidOperationException("Cannot update rules for a completed tournament.");
        if (_matches.Count > 0)
            throw new InvalidOperationException("Cannot change tournament rules once matches have been created.");

        TournamentRules = rules;
        UpdateMatchRules(rules.GroupStageMatchRules);
    }

    public void StartGroupStage()
    {
        if (TournamentStatus != FootballTournamentStatus.Draft)
            throw new InvalidOperationException($"Cannot start group stage when status is {TournamentStatus}.");
        if (_groups.Count == 0)
            throw new InvalidOperationException("Cannot start group stage without any groups defined.");
        foreach (FootballTournamentGroup g in _groups)
        {
            if (g.Teams.Count < 2)
                throw new InvalidOperationException($"Cannot start group stage: group '{g.Name}' must have at least 2 teams.");
        }

        TournamentStatus = FootballTournamentStatus.GroupStage;
        Activate();
    }

    public void StartPlayoffStage()
    {
        if (TournamentStatus != FootballTournamentStatus.GroupStage)
            throw new InvalidOperationException($"Cannot start playoff stage when status is {TournamentStatus}.");
        if (!TournamentRules.HasPlayoffStage)
            throw new InvalidOperationException("This tournament does not have a playoff stage.");
        TournamentStatus = FootballTournamentStatus.PlayoffStage;
    }

    public void CompleteTournament()
    {
        if (TournamentStatus != FootballTournamentStatus.PlayoffStage &&
            TournamentStatus != FootballTournamentStatus.GroupStage)
            throw new InvalidOperationException($"Cannot complete tournament when status is {TournamentStatus}.");
        TournamentStatus = FootballTournamentStatus.Completed;
        Complete();
    }

    public void SetChampion(Guid championTeamId)
    {
        if (championTeamId == Guid.Empty)
            throw new ArgumentException("Champion team id cannot be empty.", nameof(championTeamId));
        ChampionTeamId = championTeamId;
    }

    public void CancelTournament()
    {
        if (TournamentStatus == FootballTournamentStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed tournament.");
        if (TournamentStatus == FootballTournamentStatus.Cancelled)
            throw new InvalidOperationException("Tournament is already cancelled.");
        TournamentStatus = FootballTournamentStatus.Cancelled;
        Deactivate();
    }

    public FootballTournamentGroup AddGroup(string name)
    {
        if (TournamentStatus == FootballTournamentStatus.Completed)
            throw new InvalidOperationException("Cannot add groups to a completed tournament.");
        FootballTournamentGroup group = new(Id, name, _groups.Count);
        _groups.Add(group);
        return group;
    }

    public void RemoveGroup(Guid groupId)
    {
        if (TournamentStatus == FootballTournamentStatus.Completed)
            throw new InvalidOperationException("Cannot remove groups from a completed tournament.");
        FootballTournamentGroup? group = _groups.FirstOrDefault(g => g.Id == groupId);
        if (group != null)
            _groups.Remove(group);
    }

    public FootballTournamentGroup? GetGroup(Guid groupId) =>
        _groups.FirstOrDefault(g => g.Id == groupId);
}
