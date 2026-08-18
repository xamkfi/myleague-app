namespace Domain.Entities.Hockey.Competitions;

/// <summary>
/// Assigns a <see cref="HockeyCompetitionTeam"/> to a specific <see cref="HockeyCompetitionDivision"/>.
/// this does not reference <c>HockeyTeam</c> directly — it always goes through
/// the competition team surrogate. A team must first be added to the competition via
/// <see cref="HockeyCompetition.AddTeam"/> before it can be placed in a division.
/// <see cref="HockeyCompetition.AddTeamToDivision"/> enforces that the competition team belongs
/// to the same competition and is not already in another active division.
/// Removal is a soft deactivate (<see cref="Deactivate"/>), not a hard delete.
/// </summary>
public class HockeyCompetitionDivisionTeam : BaseEntity
{
    /// <summary>Gets the competition division this membership belongs to.</summary>
    public Guid CompetitionDivisionId { get; private set; }

    /// <summary>Gets the parent competition division.</summary>
    public HockeyCompetitionDivision CompetitionDivision { get; private set; } = null!;

    /// <summary>
    /// Gets the competition team participating in this division.
    /// References <see cref="HockeyCompetitionTeam"/>, not a raw team id.
    /// </summary>
    public Guid CompetitionTeamId { get; private set; }

    /// <summary>Gets the competition team entity.</summary>
    public HockeyCompetitionTeam CompetitionTeam { get; private set; } = null!;

    /// <summary>Gets the optional seed within this division (e.g. for playoff bracketing).</summary>
    public int? Seed { get; private set; }

    /// <summary>Gets the current standing rank within the division, if calculated.</summary>
    public int? StandingRank { get; private set; }

    /// <summary>
    /// Gets whether this division membership is active.
    /// Deactivated memberships are retained for history.
    /// </summary>
    public bool IsActive { get; private set; }

    private HockeyCompetitionDivisionTeam() { }

    internal HockeyCompetitionDivisionTeam(Guid competitionDivisionId, Guid competitionTeamId, int? seed = null)
    {
        if (competitionDivisionId == Guid.Empty)
            throw new ArgumentException("Competition division id cannot be empty.", nameof(competitionDivisionId));
        if (competitionTeamId == Guid.Empty)
            throw new ArgumentException("Competition team id cannot be empty.", nameof(competitionTeamId));

        CompetitionDivisionId = competitionDivisionId;
        CompetitionTeamId = competitionTeamId;
        Seed = seed;
        IsActive = true;
    }

    /// <summary>Soft-removes the team from the division without deleting the record.</summary>
    internal void Deactivate() => IsActive = false;

    internal void UpdateSeed(int? seed) => Seed = seed;

    internal void UpdateStandingRank(int? standingRank) => StandingRank = standingRank;
}
