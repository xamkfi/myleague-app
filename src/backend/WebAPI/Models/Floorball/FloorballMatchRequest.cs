using System.ComponentModel.DataAnnotations;
using WebAPI.Models.Common.Pagination;
using Domain.Enums.Floorball;

namespace WebAPI.Models.Floorball;

/// <summary>
/// Request model for getting paginated floorball matches
/// </summary>
public record GetFloorballMatchesRequest : PagedRequestBase
{
    /// <summary>
    /// Gets the competition ID filter (season or tournament)
    /// </summary>
    public Guid? CompetitionId { get; init; }

    /// <summary>
    /// Gets the team ID filter (matches where this team played)
    /// </summary>
    public Guid? TeamId { get; init; }

    /// <summary>
    /// Gets the tournament group ID filter (matches in this tournament group only)
    /// </summary>
    public Guid? TournamentGroupId { get; init; }

    /// <summary>
    /// Gets the start date filter (matches on or after this date)
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// Gets the end date filter (matches on or before this date)
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// Gets the match status filter
    /// </summary>
    public FloorballMatchStatus? Status { get; init; }

    /// <summary>
    /// Gets the sort order ("asc" or "desc")
    /// </summary>
    public string SortOrder { get; init; } = "desc";

    /// <summary>
    /// Gets the search query to filter matches by team names (case-insensitive, partial match)
    /// </summary>
    public string? SearchQuery { get; init; }

    /// <summary>
    /// Gets the competition type filter (Season or Tournament). When null, matches from both types are returned.
    /// </summary>
    public FloorballCompetitionType? CompetitionType { get; init; }
}

/// <summary>
/// Request model for getting team matches with pagination and filtering (team ID comes from route)
/// </summary>
public record GetTeamMatchesRequest : PagedRequestBase
{
    /// <summary>
    /// Gets the start date filter (matches on or after this date)
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// Gets the end date filter (matches on or before this date)
    /// </summary>
    public DateTime? EndDate { get; init; }
}

/// <summary>
/// Request model for creating a new floorball match
/// </summary>
public record CreateFloorballMatchRequest
{
    /// <summary>
    /// Gets the competition ID (season or tournament)
    /// </summary>
    [Required(ErrorMessage = "Competition ID is required")]
    public Guid? CompetitionId { get; init; }

    /// <summary>
    /// Gets the home team ID. Optional: leave null to schedule a fixture before the participant is
    /// known (e.g. future league round, playoff slot waiting on a feeder). Use the
    /// <c>PUT /api/floorball-matches/{id}/teams</c> endpoint to fill it in later.
    /// </summary>
    public Guid? HomeTeamId { get; init; }

    /// <summary>
    /// Gets the away team ID. Optional; see <see cref="HomeTeamId"/>.
    /// </summary>
    public Guid? AwayTeamId { get; init; }

    /// <summary>
    /// Gets the referee ID (optional)
    /// </summary>
    public Guid? RefereeId { get; init; }

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

    /// <summary>
    /// Optional tournament group ID for tournament group-stage matches.
    /// Ignored for league matches.
    /// </summary>
    public Guid? TournamentGroupId { get; init; }

    /// <summary>
    /// Optional tournament stage label (e.g. "GroupStage", "Quarterfinal") for tournament matches.
    /// Ignored for league matches.
    /// </summary>
    public string? TournamentStage { get; init; }
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
/// Request model for the "assign teams to a scheduled match" endpoint. Either side may be
/// <c>null</c> to clear that slot back to "to be determined". When both are present they must
/// reference different teams.
/// </summary>
public record AssignMatchTeamsRequest
{
    /// <summary>
    /// New home team for the match, or <c>null</c> to clear the slot.
    /// </summary>
    public Guid? HomeTeamId { get; init; }

    /// <summary>
    /// New away team for the match, or <c>null</c> to clear the slot.
    /// </summary>
    public Guid? AwayTeamId { get; init; }
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
    /// Gets the second assisting player ID (optional)
    /// </summary>
    public Guid? SecondaryAssistingPlayerIs { get; init; }

    /// <summary>
    /// Gets the period number
    /// </summary>
    [Required(ErrorMessage = "Period number is required")]
    [Range(1, 4, ErrorMessage = "Period number must be between 1 and 4")]
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
