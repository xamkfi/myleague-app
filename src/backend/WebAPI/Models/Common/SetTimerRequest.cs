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
        [Range(0, int.MaxValue, ErrorMessage = "Time in seconds must be non-negative")]
        public int TimeInSeconds { get; set; }
    }
}
