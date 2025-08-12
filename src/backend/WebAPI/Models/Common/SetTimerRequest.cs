using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Common
{
    /// <summary>
    /// Request model for setting timer to a specific time
    /// </summary>
    public class SetTimerRequest
    {
        /// <summary>
        /// The time to set in seconds
        /// </summary>
        [Required]
        [Range(0, 7200, ErrorMessage = "Time in seconds must be between 0 and 7200 (2 hours)")]
        public int TimeInSeconds { get; set; }
    }
}
