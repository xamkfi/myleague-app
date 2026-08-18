namespace Domain.Entities.Hockey.Competitions;

/// <summary>
/// A group (lohko) within a <see cref="HockeyTournament"/> used during the group stage.
/// Each group holds a set of <see cref="HockeyTournamentGroupTeam"/> memberships that
/// reference <see cref="HockeyCompetitionTeam"/> — never <c>HockeyTeam</c> directly.
/// Groups are owned by the tournament aggregate; team placement is orchestrated via
/// <see cref="HockeyTournament.AddTeamToGroup"/>, which validates competition membership
/// and enforces one active group per competition team.
/// </summary>
public class HockeyTournamentGroup : BaseEntity
{
    /// <summary>Gets the tournament this group belongs to.</summary>
    public Guid TournamentId { get; private set; }

    /// <summary>Gets the parent tournament aggregate.</summary>
    public HockeyTournament Tournament { get; private set; } = null!;

    /// <summary>Gets the display name of the group (e.g. "A-lohko").</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the display order of this group within the tournament.</summary>
    public int SortOrder { get; private set; }

    /// <summary>Gets the competition teams assigned to this group.</summary>
    public IReadOnlyCollection<HockeyTournamentGroupTeam> Teams => _teams.AsReadOnly();
    private readonly List<HockeyTournamentGroupTeam> _teams = new();

    private HockeyTournamentGroup() { }

    internal HockeyTournamentGroup(Guid tournamentId, string name, int sortOrder)
    {
        if (tournamentId == Guid.Empty)
            throw new ArgumentException("Tournament id cannot be empty.", nameof(tournamentId));
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name cannot be null or empty.", nameof(name));
        if (sortOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(sortOrder), "Sort order cannot be negative.");

        TournamentId = tournamentId;
        Name = name;
        SortOrder = sortOrder;
    }

    internal void UpdateName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name cannot be null or empty.", nameof(name));

        Name = name;
    }

    internal void UpdateSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(sortOrder), "Sort order cannot be negative.");

        SortOrder = sortOrder;
    }

    /// <summary>
    /// Adds a competition team to the group. Idempotent for an already-active membership.
    /// Competition-team validation is the caller's responsibility (tournament aggregate).
    /// </summary>
    internal HockeyTournamentGroupTeam AddTeam(Guid competitionTeamId, int? seed = null)
    {
        if (competitionTeamId == Guid.Empty)
            throw new ArgumentException("Competition team id cannot be empty.", nameof(competitionTeamId));

        HockeyTournamentGroupTeam? existing = _teams.FirstOrDefault(t => t.CompetitionTeamId == competitionTeamId && t.IsActive);
        if (existing is not null)
            return existing;

        HockeyTournamentGroupTeam groupTeam = new(Id, competitionTeamId, seed);
        _teams.Add(groupTeam);
        return groupTeam;
    }

    /// <summary>Soft-removes a competition team from the group.</summary>
    internal void RemoveTeam(Guid competitionTeamId)
    {
        HockeyTournamentGroupTeam? existing = _teams.FirstOrDefault(t => t.CompetitionTeamId == competitionTeamId && t.IsActive);
        if (existing is null)
            throw new InvalidOperationException("Competition team is not part of this group.");

        existing.Deactivate();
    }

    /// <summary>Checks whether the given competition team has an active membership in this group.</summary>
    internal bool HasActiveTeam(Guid competitionTeamId) =>
        _teams.Any(t => t.CompetitionTeamId == competitionTeamId && t.IsActive);
}
