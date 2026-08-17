using System.ComponentModel.DataAnnotations;
using Domain.Enums.Common;
using Domain.Enums.Football;
using WebAPI.Models.Common.Pagination;

namespace WebAPI.Models.Football;

public record GetFootballMatchesRequest : PagedRequestBase
{
    public Guid? CompetitionId { get; init; }
    public Guid? TeamId { get; init; }
    public Guid? TournamentGroupId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public FootballMatchStatus? Status { get; init; }
    public string SortOrder { get; init; } = "desc";
    public string? SearchQuery { get; init; }
    public FootballCompetitionType? CompetitionType { get; init; }
    public TeamCategory? TeamCategory { get; init; }
}

public record GetTeamMatchesRequest : PagedRequestBase
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public record CreateFootballMatchRequest
{
    [Required(ErrorMessage = "Competition ID is required")]
    public Guid? CompetitionId { get; init; }

    public Guid? HomeTeamId { get; init; }
    public Guid? AwayTeamId { get; init; }
    public Guid? RefereeId { get; init; }

    [Required(ErrorMessage = "Scheduled date and time is required")]
    public string ScheduledDateTime { get; init; } = string.Empty;

    [StringLength(200, ErrorMessage = "Venue cannot exceed 200 characters")]
    public string? Venue { get; init; }

    public Guid? TournamentGroupId { get; init; }
    public string? TournamentStage { get; init; }
}

public record UpdateFootballMatchRequest
{
    [Required(ErrorMessage = "Match ID is required")]
    public Guid Id { get; init; }

    [Required(ErrorMessage = "Scheduled date and time is required")]
    public string ScheduledDateTime { get; init; } = string.Empty;

    [StringLength(200, ErrorMessage = "Venue cannot exceed 200 characters")]
    public string? Venue { get; init; }
}

public record AssignMatchTeamsRequest
{
    public Guid? HomeTeamId { get; init; }
    public Guid? AwayTeamId { get; init; }
}

public record RecordGoalRequest
{
    [Required(ErrorMessage = "Match ID is required")]
    public Guid MatchId { get; init; }

    [Required(ErrorMessage = "Scoring team ID is required")]
    public Guid ScoringTeamId { get; init; }

    [Required(ErrorMessage = "Scoring player ID is required")]
    public Guid ScoringPlayerId { get; init; }

    public Guid? AssistingPlayerId { get; init; }

    [Required(ErrorMessage = "Period number is required")]
    [Range(1, 8, ErrorMessage = "Period number must be between 1 and 8")]
    public int PeriodNumber { get; init; }

    [Required(ErrorMessage = "Time in seconds is required")]
    public int TimeInSeconds { get; init; }

    public string? Description { get; init; }
    public FootballGoalType? GoalType { get; init; }
}
