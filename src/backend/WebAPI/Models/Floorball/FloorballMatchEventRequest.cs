using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Floorball
{
    /// <summary>
    /// Base request model for floorball match events
    /// </summary>
    public abstract class FloorballMatchEventBaseRequest
    {
        /// <summary>
        /// ID of the match where the event occurred
        /// </summary>
        [Required(ErrorMessage = "Match ID is required")]
        public Guid MatchId { get; set; }

        /// <summary>
        /// ID of the team that the event is associated with
        /// </summary>
        [Required(ErrorMessage = "Team ID is required")]
        public Guid TeamId { get; set; }

        /// <summary>
        /// ID of the player involved in the event
        /// </summary>
        [Required(ErrorMessage = "Player ID is required")]
        public Guid PlayerId { get; set; }

        /// <summary>
        /// Period number when the event occurred
        /// </summary>
        [Required(ErrorMessage = "Period number is required")]
        [Range(1, 5, ErrorMessage = "Period number must be between 1 and 5")]
        public int PeriodNumber { get; set; }

        /// <summary>
        /// Time in seconds when the event occurred
        /// </summary>
        [Required(ErrorMessage = "Time in seconds is required")]
        public int TimeInSeconds { get; set; }

        /// <summary>
        /// Whether the event occurred during overtime
        /// </summary>
        public bool WasInOvertime { get; set; }

        /// <summary>
        /// Whether the event occurred during shootout
        /// </summary>
        public bool WasInShootout { get; set; }
    }

    /// <summary>
    /// Request model for recording a goal event
    /// </summary>
    public class RecordGoalEventRequest : FloorballMatchEventBaseRequest
    {
        /// <summary>
        /// ID of the player who assisted the goal (if any)
        /// </summary>
        public Guid? AssisterId { get; set; }
    }

    /// <summary>
    /// Request model for recording a penalty event
    /// </summary>
    public class RecordPenaltyEventRequest : FloorballMatchEventBaseRequest
    {
        /// <summary>
        /// Duration of the penalty in minutes
        /// </summary>
        [Required(ErrorMessage = "Penalty duration is required")]
        [Range(2, 20, ErrorMessage = "Penalty duration must be between 2 and 20 minutes")]
        public int DurationMinutes { get; set; }

        /// <summary>
        /// Type of the penalty
        /// </summary>
        [Required(ErrorMessage = "Penalty type is required")]
        public string PenaltyType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for updating a goal event
    /// </summary>
    public class UpdateGoalEventRequest : RecordGoalEventRequest
    {
        /// <summary>
        /// ID of the goal event to update
        /// </summary>
        [Required(ErrorMessage = "Event ID is required")]
        public Guid EventId { get; set; }
    }

    /// <summary>
    /// Request model for updating a penalty event
    /// </summary>
    public class UpdatePenaltyEventRequest : RecordPenaltyEventRequest
    {
        /// <summary>
        /// ID of the penalty event to update
        /// </summary>
        [Required(ErrorMessage = "Event ID is required")]
        public Guid EventId { get; set; }
    }
} 