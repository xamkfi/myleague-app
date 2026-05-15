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
    /// Private constructor for EF Core
    /// </summary>
    private FloorballTournament() : base()
    {
        TournamentStatus = FloorballTournamentStatus.Draft;
        TournamentRules = FloorballTournamentRules.Default();
        _groups = new List<FloorballTournamentGroup>();
    }

    public FloorballTournament(
        string name,
        DateTime startDate,
        DateTime endDate,
        string? venue = null,
        string? contentHtml = null,
        FloorballTournamentRules? tournamentRules = null)
        : base(name, startDate, endDate, tournamentRules?.GroupStageMatchRules)
    {
        Venue = venue;
        ContentHtml = contentHtml;
        TournamentStatus = FloorballTournamentStatus.Draft;
        TournamentRules = tournamentRules ?? FloorballTournamentRules.Default();
        _groups = new List<FloorballTournamentGroup>();
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

        TournamentRules = rules;
        UpdateMatchRules(rules.GroupStageMatchRules);
    }

    public void StartGroupStage()
    {
        if (TournamentStatus != FloorballTournamentStatus.Draft)
            throw new InvalidOperationException($"Cannot start group stage when status is {TournamentStatus}.");
        if (_groups.Count == 0)
            throw new InvalidOperationException("Cannot start group stage without any groups defined.");

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
