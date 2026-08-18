namespace Domain.Entities.Hockey.Competitions;

/// <summary>
/// Represents a team's participation in a hockey competition.
/// This is the central join entity in the competition team chain:
/// <c>HockeyTeam → HockeyCompetitionTeam → Division / Group / Playoff</c>.
/// Only this entity references <see cref="TeamId"/> directly; divisions, groups and
/// playoff series always reference a <see cref="HockeyCompetitionTeam"/> instead.
/// Membership is soft-ended via <see cref="Leave"/> (<see cref="LeftAt"/>), not deleted.
/// </summary>
public class HockeyCompetitionTeam : BaseEntity
{
    /// <summary>Gets the competition this team participates in.</summary>
    public Guid CompetitionId { get; private set; }

    /// <summary>Gets the parent competition aggregate.</summary>
    public HockeyCompetition Competition { get; private set; } = null!;

    /// <summary>
    /// Gets the underlying team identifier from the hockey team context.
    /// This is the only place below the competition aggregate that points to a raw team.
    /// </summary>
    public Guid TeamId { get; private set; }

    /// <summary>Gets the optional seed assigned when the team joined the competition.</summary>
    public int? Seed { get; private set; }

    /// <summary>Gets when the team joined the competition.</summary>
    public DateTime JoinedAt { get; private set; }

    /// <summary>
    /// Gets when the team left the competition, or <c>null</c> if still active.
    /// Set by <see cref="Leave"/>; the row is retained for history.
    /// </summary>
    public DateTime? LeftAt { get; private set; }

    /// <summary>Gets whether the team is currently participating (<see cref="LeftAt"/> is null).</summary>
    public bool IsActive => LeftAt is null;

    private HockeyCompetitionTeam() { }

    internal HockeyCompetitionTeam(Guid competitionId, Guid teamId, int? seed = null)
    {
        if (competitionId == Guid.Empty)
            throw new ArgumentException("Competition id cannot be empty.", nameof(competitionId));
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team id cannot be empty.", nameof(teamId));

        CompetitionId = competitionId;
        TeamId = teamId;
        Seed = seed;
        JoinedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the team as having left the competition without removing the record.
    /// Idempotent when already left.
    /// </summary>
    internal void Leave()
    {
        if (LeftAt is not null)
            return;

        LeftAt = DateTime.UtcNow;
    }

    internal void UpdateSeed(int? seed) => Seed = seed;
}
