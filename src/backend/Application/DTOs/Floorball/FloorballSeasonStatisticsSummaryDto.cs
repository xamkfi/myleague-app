namespace Application.DTOs.Floorball;

/// <summary>
/// DTO for comprehensive season statistics summary
/// </summary>
public class FloorballSeasonStatisticsSummaryDto
{
    /// <summary>
    /// Gets or sets the season ID
    /// </summary>
    public Guid SeasonId { get; set; }

    /// <summary>
    /// Gets or sets season name
    /// </summary>
    public string SeasonName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets team standings ordered by points
    /// </summary>
    public List<FloorballTeamSeasonStatisticsDto> TeamStandings { get; set; } = new();

    /// <summary>
    /// Gets or sets top scoring players
    /// </summary>
    public List<FloorballPlayerSeasonStatisticsDto> TopScorers { get; set; } = new();

    /// <summary>
    /// Gets or sets top assist leaders
    /// </summary>
    public List<FloorballPlayerSeasonStatisticsDto> TopAssists { get; set; } = new();

    /// <summary>
    /// Gets or sets total games played in the season
    /// </summary>
    public int TotalGames { get; set; }

    /// <summary>
    /// Gets or sets total goals scored in the season
    /// </summary>
    public int TotalGoals { get; set; }

    /// <summary>
    /// Gets or sets average goals per game
    /// </summary>
    public decimal AverageGoalsPerGame { get; set; }
}
