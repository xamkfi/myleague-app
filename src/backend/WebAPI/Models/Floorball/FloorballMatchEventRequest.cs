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
    /// Request model for creating an event-sourced floorball match
    /// </summary>
    public class CreateEventSourcedFloorballMatchRequest
    {
        /// <summary>
        /// ID of the season
        /// </summary>
        [Required(ErrorMessage = "Season ID is required")]
        public Guid SeasonId { get; set; }

        /// <summary>
        /// ID of the home team
        /// </summary>
        [Required(ErrorMessage = "Home team ID is required")]
        public Guid HomeTeamId { get; set; }

        /// <summary>
        /// ID of the away team
        /// </summary>
        [Required(ErrorMessage = "Away team ID is required")]
        public Guid AwayTeamId { get; set; }

        /// <summary>
        /// Scheduled date and time of the match
        /// </summary>
        [Required(ErrorMessage = "Scheduled date and time is required")]
        public string ScheduledDateTime { get; set; } = string.Empty;

        /// <summary>
        /// Venue of the match
        /// </summary>
        [StringLength(200, ErrorMessage = "Venue cannot exceed 200 characters")]
        public string? Venue { get; set; }
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

    /// <summary>
    /// Request model for adding an official to a match
    /// </summary>
    public class AddOfficialToMatchRequest
    {
        /// <summary>
        /// ID of the match
        /// </summary>
        [Required(ErrorMessage = "Match ID is required")]
        public Guid MatchId { get; set; }

        /// <summary>
        /// ID of the referee
        /// </summary>
        [Required(ErrorMessage = "Referee ID is required")]
        public Guid RefereeId { get; set; }
    }

    /// <summary>
    /// Request model for changing the season of an event-sourced floorball match
    /// </summary>
    public class ChangeEventSourcedFloorballMatchSeasonRequest
    {
        /// <summary>
        /// New season ID for the match
        /// </summary>
        [Required(ErrorMessage = "New season ID is required")]
        public Guid NewSeasonId { get; set; }
    }

    /// <summary>
    /// Request model for changing the teams of an event-sourced floorball match
    /// </summary>
    public class ChangeEventSourcedFloorballMatchTeamsRequest
    {
        /// <summary>
        /// New home team ID for the match
        /// </summary>
        [Required(ErrorMessage = "New home team ID is required")]
        public Guid NewHomeTeamId { get; set; }

        /// <summary>
        /// New away team ID for the match
        /// </summary>
        [Required(ErrorMessage = "New away team ID is required")]
        public Guid NewAwayTeamId { get; set; }
    }

    /// <summary>
    /// Request model for changing the venue of an event-sourced floorball match
    /// </summary>
    public class ChangeEventSourcedFloorballMatchVenueRequest
    {
        /// <summary>
        /// New venue for the match
        /// </summary>
        [Required(ErrorMessage = "New venue is required")]
        [StringLength(200, ErrorMessage = "Venue cannot exceed 200 characters")]
        public string NewVenue { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for changing the date/time of an event-sourced floorball match
    /// </summary>
    public class ChangeEventSourcedFloorballMatchDateTimeRequest
    {
        /// <summary>
        /// New date and time for the match
        /// </summary>
        [Required(ErrorMessage = "New date and time is required")]
        public DateTime NewDateTime { get; set; }
    }
} 
