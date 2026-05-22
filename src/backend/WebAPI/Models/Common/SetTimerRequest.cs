using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Common
{
    /// <summary>
    /// Request model for setting timer to a specific time
    /// </summary>
    public class SetTimerRequest
    {
        /// <summary>
        /// The time to set in seconds. Only a non-negative floor is enforced – the
        /// scorekeeper is trusted to enter any value appropriate for the match's
        /// configured periods/overtime.
        /// </summary>
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Time in seconds must be non-negative")]
        public int TimeInSeconds { get; set; }
    }
}
