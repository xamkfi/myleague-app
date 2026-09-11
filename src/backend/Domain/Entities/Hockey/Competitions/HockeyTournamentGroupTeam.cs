namespace Domain.Entities.Hockey.Competitions;

/// <summary>
/// Assigns a <see cref="HockeyCompetitionTeam"/> to a <see cref="HockeyTournamentGroup"/>.
/// Used during the tournament group stage; references the competition team surrogate,
/// not <c>HockeyTeam</c> directly. The parent <see cref="HockeyTournament.AddTeamToGroup"/>
/// validates that the competition team is active and belongs to the same tournament,
/// and that it is not already in another active group.
/// Removal is a soft deactivate (<see cref="Deactivate"/>), not a hard delete.
/// </summary>
public class HockeyTournamentGroupTeam : BaseEntity
{
    /// <summary>Gets the tournament group this membership belongs to.</summary>
    public Guid TournamentGroupId { get; private set; }

    /// <summary>Gets the parent tournament group.</summary>
    public HockeyTournamentGroup TournamentGroup { get; private set; } = null!;

    /// <summary>
    /// Gets the competition team placed in this group.
    /// References <see cref="HockeyCompetitionTeam"/>, not a raw team id.
    /// </summary>
    public Guid CompetitionTeamId { get; private set; }

    /// <summary>Gets the competition team entity.</summary>
    public HockeyCompetitionTeam CompetitionTeam { get; private set; } = null!;

    /// <summary>Gets the optional seed within this group (e.g. for cross-group playoff seeding).</summary>
    public int? Seed { get; private set; }

    /// <summary>
    /// Gets whether this group membership is active.
    /// Deactivated memberships are retained for history.
    /// </summary>
    public bool IsActive { get; private set; }

    private HockeyTournamentGroupTeam() { }

    internal HockeyTournamentGroupTeam(Guid tournamentGroupId, Guid competitionTeamId, int? seed = null)
    {
        if (tournamentGroupId == Guid.Empty)
            throw new ArgumentException("Tournament group id cannot be empty.", nameof(tournamentGroupId));
        if (competitionTeamId == Guid.Empty)
            throw new ArgumentException("Competition team id cannot be empty.", nameof(competitionTeamId));

        TournamentGroupId = tournamentGroupId;
        CompetitionTeamId = competitionTeamId;
        Seed = seed;
        IsActive = true;
    }

    /// <summary>Soft-removes the team from the group without deleting the record.</summary>
    internal void Deactivate() => IsActive = false;

    internal void UpdateSeed(int? seed) => Seed = seed;
}
