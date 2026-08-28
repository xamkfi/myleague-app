using System.ComponentModel.DataAnnotations;
using Domain.Enums.Common;
using Domain.Enums.Football;
using WebAPI.Models.Common.Pagination;

namespace WebAPI.Models.Football;

/// <summary>
/// Request model for getting paginated football matches
/// </summary>
public record GetFootballMatchesRequest : PagedRequestBase
{
    /// <summary>
    /// Optional competition filter
    /// </summary>
    public Guid? CompetitionId { get; init; }

    /// <summary>
    /// Optional team filter
    /// </summary>
    public Guid? TeamId { get; init; }

    /// <summary>
    /// Optional tournament group filter
    /// </summary>
    public Guid? TournamentGroupId { get; init; }

    /// <summary>
    /// Optional start of the scheduled-date range
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// Optional end of the scheduled-date range
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// Optional match status filter
    /// </summary>
    public FootballMatchStatus? Status { get; init; }

    /// <summary>
    /// Sort order for scheduled date (asc or desc)
    /// </summary>
    public string SortOrder { get; init; } = "desc";

    /// <summary>
    /// Optional team-name search query
    /// </summary>
    public string? SearchQuery { get; init; }

    /// <summary>
    /// Optional competition type filter
    /// </summary>
    public FootballCompetitionType? CompetitionType { get; init; }

    /// <summary>
    /// Optional audience / age-group category filter
    /// </summary>
    public TeamCategory? TeamCategory { get; init; }
}

/// <summary>
/// Request model for getting paginated matches for a single team
/// </summary>
public record GetTeamMatchesRequest : PagedRequestBase
{
    /// <summary>
    /// Optional start of the scheduled-date range
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// Optional end of the scheduled-date range
    /// </summary>
    public DateTime? EndDate { get; init; }
}

/// <summary>
/// Request model for creating a football match
/// </summary>
public record CreateFootballMatchRequest
{
    /// <summary>
    /// Competition the match belongs to
    /// </summary>
    [Required(ErrorMessage = "Competition ID is required")]
    public Guid? CompetitionId { get; init; }

    /// <summary>
    /// Home team identifier
    /// </summary>
    public Guid? HomeTeamId { get; init; }

    /// <summary>
    /// Away team identifier
    /// </summary>
    public Guid? AwayTeamId { get; init; }

    /// <summary>
    /// Optional referee identifier
    /// </summary>
    public Guid? RefereeId { get; init; }

    /// <summary>
    /// Scheduled kickoff date and time
    /// </summary>
    [Required(ErrorMessage = "Scheduled date and time is required")]
    public string ScheduledDateTime { get; init; } = string.Empty;

    /// <summary>
    /// Optional venue
    /// </summary>
    [StringLength(200, ErrorMessage = "Venue cannot exceed 200 characters")]
    public string? Venue { get; init; }

    /// <summary>
    /// Optional tournament group identifier
    /// </summary>
    public Guid? TournamentGroupId { get; init; }

    /// <summary>
    /// Optional tournament stage name
    /// </summary>
    public string? TournamentStage { get; init; }
}

/// <summary>
/// Request model for updating a football match
/// </summary>
public record UpdateFootballMatchRequest
{
    /// <summary>
    /// Match identifier
    /// </summary>
    [Required(ErrorMessage = "Match ID is required")]
    public Guid Id { get; init; }

    /// <summary>
    /// Scheduled kickoff date and time
    /// </summary>
    [Required(ErrorMessage = "Scheduled date and time is required")]
    public string ScheduledDateTime { get; init; } = string.Empty;

    /// <summary>
    /// Optional venue
    /// </summary>
    [StringLength(200, ErrorMessage = "Venue cannot exceed 200 characters")]
    public string? Venue { get; init; }
}

/// <summary>
/// Request model for assigning home and away teams to a match
/// </summary>
public record AssignMatchTeamsRequest
{
    /// <summary>
    /// Home team identifier
    /// </summary>
    public Guid? HomeTeamId { get; init; }

    /// <summary>
    /// Away team identifier
    /// </summary>
    public Guid? AwayTeamId { get; init; }
}

/// <summary>
/// Request model for recording a goal
/// </summary>
public record RecordGoalRequest
{
    /// <summary>
    /// Match identifier
    /// </summary>
    [Required(ErrorMessage = "Match ID is required")]
    public Guid MatchId { get; init; }

    /// <summary>
    /// Team that scored
    /// </summary>
    [Required(ErrorMessage = "Scoring team ID is required")]
    public Guid ScoringTeamId { get; init; }

    /// <summary>
    /// Player that scored
    /// </summary>
    [Required(ErrorMessage = "Scoring player ID is required")]
    public Guid ScoringPlayerId { get; init; }

    /// <summary>
    /// Optional assisting player
    /// </summary>
    public Guid? AssistingPlayerId { get; init; }

    /// <summary>
    /// Period in which the goal was scored
    /// </summary>
    [Required(ErrorMessage = "Period number is required")]
    [Range(1, 8, ErrorMessage = "Period number must be between 1 and 8")]
    public int PeriodNumber { get; init; }

    /// <summary>
    /// Elapsed time in the period, in seconds
    /// </summary>
    [Required(ErrorMessage = "Time in seconds is required")]
    public int TimeInSeconds { get; init; }

    /// <summary>
    /// Optional event description
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Optional goal type
    /// </summary>
    public FootballGoalType? GoalType { get; init; }

    /// <summary>
    /// When <c>true</c>, skips the per-(match, scorer) double-click window. Intended for
    /// historical import / admin backfill, not the live scorekeeper UI.
    /// </summary>
    public bool SkipRateLimit { get; init; }
}
