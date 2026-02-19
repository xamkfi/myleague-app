using Application.Features.Floorball.Statistics.DTOs;

namespace Application.Features.Floorball.Players.DTOs
{
    /// <summary>
    /// Represents a complete player profile with all season statistics
    /// </summary>
    public class FloorballPlayerProfileDto
    {
        /// <summary>
        /// Gets or sets the player information
        /// </summary>
        public FloorballPlayerPublicDto Player { get; set; } = null!;

        /// <summary>
        /// Gets or sets all season statistics for this player
        /// </summary>
        public List<FloorballPlayerSeasonStatisticsDto>? SeasonStatistics { get; set; } = new();

        public List<FloorballGoalieSeasonStatisticsDto>? SeasonStatisticsForGoalie { get; set; } = new();
    }
}
