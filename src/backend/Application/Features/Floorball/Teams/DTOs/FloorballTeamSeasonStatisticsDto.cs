using Domain.Enums.Floorball;

namespace Application.DTOs.Floorball;

/// <summary>
/// DTO for floorball team season statistics
/// </summary>
public class FloorballTeamSeasonStatisticsDto
{
    /// <summary>
    /// Gets or sets the statistics ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the team ID
    /// </summary>
    public Guid TeamId { get; set; }

    /// <summary>
    /// Gets or sets the season ID
    /// </summary>
    public Guid SeasonId { get; set; }

    /// <summary>
    /// Gets or sets team name
    /// </summary>
    public string TeamName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets team logo
    /// </summary>
    public Uri? TeamLogo { get; set; }

    /// <summary>
    /// Gets or sets season name
    /// </summary>
    public string SeasonName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of games played
    /// </summary>
    public int GamesPlayed { get; set; }

    /// <summary>
    /// Gets or sets the number of wins
    /// </summary>
    public int Wins { get; set; }

    /// <summary>
    /// Gets or sets the number of losses
    /// </summary>
    public int Losses { get; set; }

    /// <summary>
    /// Gets or sets the number of ties
    /// </summary>
    public int Ties { get; set; }

    /// <summary>
    /// Gets or sets the total points
    /// </summary>
    public int Points { get; set; }

    /// <summary>
    /// Gets or sets goals scored
    /// </summary>
    public int GoalsFor { get; set; }

    /// <summary>
    /// Gets or sets goals conceded
    /// </summary>
    public int GoalsAgainst { get; set; }

    /// <summary>
    /// Gets or sets goal difference
    /// </summary>
    public int GoalDifference { get; set; }

    /// <summary>
    /// Gets or sets shots taken
    /// </summary>
    public int ShotsFor { get; set; }

    /// <summary>
    /// Gets or sets shots faced
    /// </summary>
    public int ShotsAgainst { get; set; }

    /// <summary>
    /// Gets or sets shot percentage
    /// </summary>
    public decimal ShotPercentage { get; set; }

    /// <summary>
    /// Gets or sets power play goals
    /// </summary>
    public int PowerPlayGoals { get; set; }

    /// <summary>
    /// Gets or sets power play opportunities
    /// </summary>
    public int PowerPlayOpportunities { get; set; }

    /// <summary>
    /// Gets or sets power play percentage
    /// </summary>
    public decimal PowerPlayPercentage { get; set; }

    /// <summary>
    /// Gets or sets short-handed goals
    /// </summary>
    public int ShortHandedGoals { get; set; }

    /// <summary>
    /// Gets or sets penalty kill opportunities
    /// </summary>
    public int PenaltyKillOpportunities { get; set; }

    /// <summary>
    /// Gets or sets penalty kill percentage
    /// </summary>
    public decimal PenaltyKillPercentage { get; set; }

    /// <summary>
    /// Gets or sets total penalty minutes
    /// </summary>
    public int PenaltyMinutes { get; set; }

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
    /// Gets or sets home wins
    /// </summary>
    public int HomeWins { get; set; }

    /// <summary>
    /// Gets or sets home losses
    /// </summary>
    public int HomeLosses { get; set; }

    /// <summary>
    /// Gets or sets away wins
    /// </summary>
    public int AwayWins { get; set; }

    /// <summary>
    /// Gets or sets away losses
    /// </summary>
    public int AwayLosses { get; set; }

    /// <summary>
    /// Gets or sets last five form. values are W, L, T
    /// </summary>
    public FloorballGameResult[] LastFiveForm { get; set; } = Array.Empty<FloorballGameResult>();
}
