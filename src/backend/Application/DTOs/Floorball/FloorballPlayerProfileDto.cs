
namespace Application.DTOs.Floorball
{
    /// <summary>
    /// Represents a complete player profile with all season statistics
    /// </summary>
    public class FloorballPlayerProfileDto
    {
        /// <summary>
        /// Gets or sets the player information
        /// </summary>
        public FloorballPlayerDto Player { get; set; } = null!;

        /// <summary>
        /// Gets or sets all season statistics for this player
        /// </summary>
        public List<FloorballPlayerSeasonStatisticsDto> SeasonStatistics { get; set; } = new();
    }
}
