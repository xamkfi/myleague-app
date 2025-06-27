namespace WebAPI.Models.Floorball
{
    /// <summary>
    /// Request model for rescheduling a floorball match
    /// </summary>
    public class RescheduleFloorballMatchRequest
    {
        /// <summary>
        /// Gets or sets the new scheduled date and time for the match
        /// </summary>
        public string NewScheduledDateTime { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the new venue for the match (optional)
        /// </summary>
        public string? NewVenue { get; set; }
    }
} 