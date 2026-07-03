namespace Domain.Entities.Hockey.Competitions;

/// <summary>
/// Links a team to a hockey competition.
/// </summary>
public class HockeyCompetitionTeam : BaseEntity
{
    public Guid CompetitionId { get; private set; }
    public HockeyCompetition Competition { get; private set; } = null!;
    public Guid TeamId { get; private set; }
    public int? Seed { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }
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

    internal void Leave()
    {
        if (LeftAt is not null)
            return;

        LeftAt = DateTime.UtcNow;
    }

    internal void UpdateSeed(int? seed) => Seed = seed;
}
