namespace Application.Features.Floorball.Statistics.DTOs;

/// <summary>
/// DTO for floorball player season statistics
/// </summary>
public class FloorballPlayerSeasonStatisticsDto
{
    /// <summary>
    /// Gets or sets the statistics ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the player ID
    /// </summary>
    public Guid PlayerId { get; set; }

    /// <summary>
    /// Gets or sets the team ID
    /// </summary>
    public Guid TeamId { get; set; }

    /// <summary>
    /// Gets or sets the season ID
    /// </summary>
    public Guid SeasonId { get; set; }

    /// <summary>
    /// Gets or sets player name
    /// </summary>
    public string PlayerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets team name
    /// </summary>
    public string TeamName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets team logo URL
    /// </summary>
    public string? TeamLogo { get; set; }

    /// <summary>
    /// Gets or sets season name
    /// </summary>
    public string SeasonName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of games played
    /// </summary>
    public int GamesPlayed { get; set; }

    /// <summary>
    /// Gets or sets the number of goals
    /// </summary>
    public int Goals { get; set; }

    /// <summary>
    /// Gets or sets the number of assists
    /// </summary>
    public int Assists { get; set; }

    /// <summary>
    /// Gets or sets total points
    /// </summary>
    public int Points { get; set; }

    /// <summary>
    /// Gets or sets penalty minutes
    /// </summary>
    public int PenaltyMinutes { get; set; }

    /// <summary>
    /// Gets or sets plus/minus rating
    /// </summary>
    public int PlusMinusRating { get; set; }

    /// <summary>
    /// Gets or sets shots on goal
    /// </summary>
    public int ShotsOnGoal { get; set; }

    /// <summary>
    /// Gets or sets shooting percentage
    /// </summary>
    public decimal ShotPercentage { get; set; }

    /// <summary>
    /// Gets or sets power play goals
    /// </summary>
    public int PowerPlayGoals { get; set; }

    /// <summary>
    /// Gets or sets power play assists
    /// </summary>
    public int PowerPlayAssists { get; set; }

    /// <summary>
    /// Gets or sets short-handed goals
    /// </summary>
    public int ShortHandedGoals { get; set; }

    /// <summary>
    /// Gets or sets short-handed assists
    /// </summary>
    public int ShortHandedAssists { get; set; }

    /// <summary>
    /// Gets or sets game-winning goals
    /// </summary>
    public int GameWinningGoals { get; set; }

    /// <summary>
    /// Gets or sets overtime goals
    /// </summary>
    public int OvertimeGoals { get; set; }

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
}
