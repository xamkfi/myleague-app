using Application.Features.Football.Statistics.DTOs;

namespace Application.Features.Football.Players.DTOs;

/// <summary>
/// Represents a complete football player profile with season statistics.
/// </summary>
public class FootballPlayerProfileDto
{
    /// <summary>
    /// Gets or sets the player information
    /// </summary>
    public FootballPlayerPublicDto Player { get; set; } = null!;

    /// <summary>
    /// Gets or sets all season statistics for this player
    /// </summary>
    public List<FootballPlayerSeasonStatisticsDto>? SeasonStatistics { get; set; } = new();
}
