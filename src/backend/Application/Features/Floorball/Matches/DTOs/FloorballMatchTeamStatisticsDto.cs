namespace Application.Features.Floorball.Matches.DTOs;

/// <summary>
/// DTO for floorball match team statistics
/// </summary>
public class FloorballMatchTeamStatisticsDto
{
    /// <summary>
    /// Gets or sets the statistics ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the match ID
    /// </summary>
    public Guid MatchId { get; set; }

    /// <summary>
    /// Gets or sets the team ID
    /// </summary>
    public Guid TeamId { get; set; }

    /// <summary>
    /// Gets or sets team name
    /// </summary>
    public string TeamName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets shots on goal
    /// </summary>
    public int ShotsOnGoal { get; set; }

    /// <summary>
    /// Gets or sets total shots
    /// </summary>
    public int ShotsTotal { get; set; }

    /// <summary>
    /// Gets or sets shot percentage
    /// </summary>
    public decimal ShotPercentage { get; set; }

    /// <summary>
    /// Gets or sets faceoffs won
    /// </summary>
    public int FaceoffWins { get; set; }

    /// <summary>
    /// Gets or sets total faceoffs
    /// </summary>
    public int FaceoffAttempts { get; set; }

    /// <summary>
    /// Gets or sets faceoff percentage
    /// </summary>
    public decimal FaceoffPercentage { get; set; }

    /// <summary>
    /// Gets or sets power play opportunities
    /// </summary>
    public int PowerPlayOpportunities { get; set; }

    /// <summary>
    /// Gets or sets power play goals
    /// </summary>
    public int PowerPlayGoals { get; set; }

    /// <summary>
    /// Gets or sets power play minutes
    /// </summary>
    public int PowerPlayMinutes { get; set; }

    /// <summary>
    /// Gets or sets penalty kill opportunities
    /// </summary>
    public int PenaltyKillOpportunities { get; set; }

    /// <summary>
    /// Gets or sets successful penalty kills
    /// </summary>
    public int PenaltyKillSuccess { get; set; }

    /// <summary>
    /// Gets or sets short-handed goals
    /// </summary>
    public int ShortHandedGoals { get; set; }

    /// <summary>
    /// Gets or sets penalty minutes
    /// </summary>
    public int PenaltyMinutes { get; set; }

    /// <summary>
    /// Gets or sets hits delivered
    /// </summary>
    public int Hits { get; set; }

    /// <summary>
    /// Gets or sets shots blocked
    /// </summary>
    public int BlockedShots { get; set; }

    /// <summary>
    /// Gets or sets takeaways
    /// </summary>
    public int Takeaways { get; set; }

    /// <summary>
    /// Gets or sets giveaways
    /// </summary>
    public int Giveaways { get; set; }
}
