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
        /// Period number when the event occurred. Only a non-negative floor is enforced
        /// here; the actual upper bound (regulation + overtime + shootout) is configured
        /// per match in the domain layer.
        /// </summary>
        [Required(ErrorMessage = "Period number is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Period number must be 1 or greater")]
        public int PeriodNumber { get; set; }

        /// <summary>
        /// Time in seconds when the event occurred. With the continuous match clock the
        /// timestamp can exceed a single period's duration (e.g. period 2 begins at
        /// 900s for a 15-minute period), so no upper limit is enforced – the scorekeeper
        /// is trusted to enter the value shown on the live clock.
        /// </summary>
        [Required(ErrorMessage = "Time in seconds is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Time must be non-negative")]
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

        /// <summary>
        /// ID of the second player who assisted the goal (if any)
        /// </summary>
        public Guid? SecondaryAssisterId { get; set; }
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

        /// <summary>
        /// Optional free-text description of the penalty (e.g. "hooking", "slashing"). The
        /// scorekeeper enters this in the Record Penalty modal. Persisted on the event so it
        /// can be shown beneath the penalty line in both the admin live history and the public
        /// match events list. May be empty when the operator did not provide a reason.
        /// </summary>
        public string? Description { get; set; }
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

    /// <summary>
    /// Request model for recording a save event
    /// </summary>
    public class RecordSaveEventRequest : FloorballMatchEventBaseRequest
    {
        /// <summary>
        /// Number of save events to record at the supplied (period, time) coordinate. Defaults to
        /// `1`, which preserves the legacy single-save semantics including the controller-side
        /// rate limit. Values greater than 1 indicate a bulk backfill (e.g. the scorekeeper
        /// missed individual saves during play and is recording an aggregate count after the
        /// fact); the controller skips the rate limit in that case and the handler records the
        /// requested count inside a single transaction.
        /// </summary>
        [Range(1, 99, ErrorMessage = "Count must be between 1 and 99")]
        public int Count { get; set; } = 1;
    }


    /// <summary>
    /// Request model for match ID only operations
    /// </summary>
    public class MatchIdRequest
    {
        /// <summary>
        /// ID of the match
        /// </summary>
        [Required(ErrorMessage = "Match ID is required")]
        public Guid MatchId { get; set; }
    }

} 
