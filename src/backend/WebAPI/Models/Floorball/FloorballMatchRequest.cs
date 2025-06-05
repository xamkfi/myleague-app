using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Floorball;

/// <summary>
/// Request model for creating a new floorball match
/// </summary>
public record CreateFloorballMatchRequest
{
    /// <summary>
    /// Gets the season ID
    /// </summary>
    [Required(ErrorMessage = "Season ID is required")]
    public Guid SeasonId { get; init; }

    /// <summary>
    /// Gets the home team ID
    /// </summary>
    [Required(ErrorMessage = "Home team ID is required")]
    public Guid HomeTeamId { get; init; }

    /// <summary>
    /// Gets the away team ID
    /// </summary>
    [Required(ErrorMessage = "Away team ID is required")]
    public Guid AwayTeamId { get; init; }

    /// <summary>
    /// Gets the scheduled date and time of the match
    /// </summary>
    [Required(ErrorMessage = "Scheduled date and time is required")]
    public string ScheduledDateTime { get; init; } = string.Empty;

    /// <summary>
    /// Gets the venue of the match
    /// </summary>
    [StringLength(200, ErrorMessage = "Venue cannot exceed 200 characters")]
    public string? Venue { get; init; }
}

/// <summary>
/// Request model for updating an existing floorball match
/// </summary>
public record UpdateFloorballMatchRequest
{
    /// <summary>
    /// Gets the match ID
    /// </summary>
    [Required(ErrorMessage = "Match ID is required")]
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the scheduled date and time of the match
    /// </summary>
    [Required(ErrorMessage = "Scheduled date and time is required")]
    public string ScheduledDateTime { get; init; } = string.Empty;

    /// <summary>
    /// Gets the venue of the match
    /// </summary>
    [StringLength(200, ErrorMessage = "Venue cannot exceed 200 characters")]
    public string? Venue { get; init; }
}

/// <summary>
/// Request model for recording a goal in a floorball match
/// </summary>
public record RecordGoalRequest
{
    /// <summary>
    /// Gets the match ID
    /// </summary>
    [Required(ErrorMessage = "Match ID is required")]
    public Guid MatchId { get; init; }

    /// <summary>
    /// Gets the scoring team ID
    /// </summary>
    [Required(ErrorMessage = "Scoring team ID is required")]
    public Guid ScoringTeamId { get; init; }

    /// <summary>
    /// Gets the scoring player ID
    /// </summary>
    [Required(ErrorMessage = "Scoring player ID is required")]
    public Guid ScoringPlayerId { get; init; }

    /// <summary>
    /// Gets the assisting player ID (optional)
    /// </summary>
    public Guid? AssistingPlayerId { get; init; }

    /// <summary>
    /// Gets the period number
    /// </summary>
    [Required(ErrorMessage = "Period number is required")]
    public int PeriodNumber { get; init; }

    /// <summary>
    /// Gets the time in seconds
    /// </summary>
    [Required(ErrorMessage = "Time in seconds is required")]
    public int TimeInSeconds { get; init; }

    /// <summary>
    /// Gets the description of the goal (optional)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the goal type (optional)
    /// </summary>
    public int? GoalType { get; init; }
}
