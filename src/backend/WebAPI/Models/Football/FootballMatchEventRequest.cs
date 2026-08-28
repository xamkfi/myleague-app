using System.ComponentModel.DataAnnotations;
using Domain.Enums.Football;

namespace WebAPI.Models.Football;

/// <summary>
/// Shared fields for football match event requests
/// </summary>
public abstract class FootballMatchEventBaseRequest
{
    /// <summary>
    /// Match identifier
    /// </summary>
    [Required(ErrorMessage = "Match ID is required")]
    public Guid MatchId { get; set; }

    /// <summary>
    /// Team associated with the event
    /// </summary>
    [Required(ErrorMessage = "Team ID is required")]
    public Guid TeamId { get; set; }

    /// <summary>
    /// Period in which the event occurred
    /// </summary>
    [Required(ErrorMessage = "Period number is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Period number must be 1 or greater")]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Elapsed time in the period, in seconds
    /// </summary>
    [Required(ErrorMessage = "Time in seconds is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Time must be non-negative")]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Optional event description
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request model for recording a card event
/// </summary>
public class RecordCardEventRequest : FootballMatchEventBaseRequest
{
    /// <summary>
    /// Player who received the card
    /// </summary>
    [Required(ErrorMessage = "Player ID is required")]
    public Guid PlayerId { get; set; }

    /// <summary>
    /// Card type (yellow or red)
    /// </summary>
    [Required(ErrorMessage = "Card type is required")]
    public FootballCardType CardType { get; set; }

    /// <summary>
    /// When <c>true</c>, skips the per-(match, player) double-click window. Intended for
    /// historical import / admin backfill, not the live scorekeeper UI.
    /// </summary>
    public bool SkipRateLimit { get; set; }
}

/// <summary>
/// Request model for recording a substitution
/// </summary>
public class RecordSubstitutionEventRequest : FootballMatchEventBaseRequest
{
    /// <summary>
    /// Player leaving the field
    /// </summary>
    [Required(ErrorMessage = "Player going off is required")]
    public Guid PlayerOffId { get; set; }

    /// <summary>
    /// Player entering the field
    /// </summary>
    [Required(ErrorMessage = "Player coming on is required")]
    public Guid PlayerOnId { get; set; }
}

/// <summary>
/// Request model that identifies a match
/// </summary>
public class MatchIdRequest
{
    /// <summary>
    /// Match identifier
    /// </summary>
    [Required(ErrorMessage = "Match ID is required")]
    public Guid MatchId { get; set; }
}

/// <summary>
/// Batch import of historical football match events. Not rate-limited; intended for
/// the JoomLeague importer and admin backfill, not live scorekeeping.
/// </summary>
public class ImportFootballMatchEventsRequest
{
    /// <summary>
    /// Goals and cards to record, in clock order.
    /// </summary>
    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    public List<ImportFootballMatchEventRequest> Events { get; set; } = [];
}

/// <summary>
/// One event in an <see cref="ImportFootballMatchEventsRequest"/> batch.
/// <c>EventType</c> is <c>Goal</c> or <c>Card</c>.
/// </summary>
public class ImportFootballMatchEventRequest
{
    /// <summary>
    /// Event kind: <c>Goal</c> or <c>Card</c>.
    /// </summary>
    [Required]
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Team associated with the event.
    /// </summary>
    [Required]
    public Guid TeamId { get; set; }

    /// <summary>
    /// Player who scored or received the card.
    /// </summary>
    public Guid? PlayerId { get; set; }

    /// <summary>
    /// Assister for a goal, when recorded.
    /// </summary>
    public Guid? AssistingPlayerId { get; set; }

    /// <summary>
    /// Period in which the event occurred.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Elapsed match clock time, in seconds.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int TimeInSeconds { get; set; }

    /// <summary>
    /// Optional football goal type for a goal event.
    /// </summary>
    public FootballGoalType? GoalType { get; set; }

    /// <summary>
    /// Optional free-text description of the event.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Card color. Required when <see cref="EventType"/> is <c>Card</c>.
    /// </summary>
    public FootballCardType? CardType { get; set; }
}
