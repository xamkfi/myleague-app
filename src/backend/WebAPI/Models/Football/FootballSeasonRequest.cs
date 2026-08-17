using System.ComponentModel.DataAnnotations;
using Domain.Enums.Common;
using WebAPI.Models.Common.Pagination;

namespace WebAPI.Models.Football;

/// <summary>
/// Request model for getting paginated football seasons
/// </summary>
public record GetFootballSeasonsPagedRequest : PagedRequestBase
{
    /// <summary>
    /// Optional season year filter (for example 2025-2026)
    /// </summary>
    [StringLength(20)]
    public string? SeasonYear { get; init; }

    /// <summary>
    /// Optional audience / age-group category filter
    /// </summary>
    public TeamCategory? TeamCategory { get; init; }
}

/// <summary>
/// Request model for creating a football season
/// </summary>
public class CreateFootballSeasonRequest
{
    /// <summary>
    /// Season name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Season start date
    /// </summary>
    [Required]
    public string StartDate { get; set; } = string.Empty;

    /// <summary>
    /// Season end date
    /// </summary>
    [Required]
    public string EndDate { get; set; } = string.Empty;

    /// <summary>
    /// Division identifiers included in the season
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one division must be specified.")]
    public List<Guid> DivisionIds { get; set; } = new();

    /// <summary>
    /// Number of halves in regular time
    /// </summary>
    public int NumberOfHalves { get; set; } = 2;

    /// <summary>
    /// Duration of each half in minutes
    /// </summary>
    public int HalfDurationMinutes { get; set; } = 45;

    /// <summary>
    /// Number of players on the field per team
    /// </summary>
    public int PlayersOnField { get; set; } = 11;

    /// <summary>
    /// Whether a goalkeeper is required
    /// </summary>
    public bool RequireGoalkeeper { get; set; } = true;

    /// <summary>
    /// Maximum number of substitutions allowed
    /// </summary>
    public int MaxSubstitutions { get; set; }

    /// <summary>
    /// Whether officials must be assigned before kickoff
    /// </summary>
    public bool RequireOfficialsToStart { get; set; }

    /// <summary>
    /// Whether extra time is allowed
    /// </summary>
    public bool AllowExtraTime { get; set; }

    /// <summary>
    /// Number of extra-time halves
    /// </summary>
    public int ExtraTimeHalfCount { get; set; } = 2;

    /// <summary>
    /// Duration of each extra-time half in minutes
    /// </summary>
    public int ExtraTimeHalfDurationMinutes { get; set; } = 15;

    /// <summary>
    /// Whether a penalty shootout is allowed
    /// </summary>
    public bool AllowPenaltyShootout { get; set; }

    /// <summary>
    /// Points awarded for a win
    /// </summary>
    public int WinPoints { get; set; } = 3;

    /// <summary>
    /// Points awarded for a draw
    /// </summary>
    public int DrawPoints { get; set; } = 1;

    /// <summary>
    /// Points awarded for a loss
    /// </summary>
    public int LossPoints { get; set; }

    /// <summary>
    /// Audience / age-group category for the season
    /// </summary>
    public TeamCategory TeamCategory { get; set; } = TeamCategory.Adult;
}

/// <summary>
/// Request model for updating a football season
/// </summary>
public class UpdateFootballSeasonRequest
{
    /// <summary>
    /// Season name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Season start date
    /// </summary>
    [Required]
    public string StartDate { get; set; } = string.Empty;

    /// <summary>
    /// Season end date
    /// </summary>
    [Required]
    public string EndDate { get; set; } = string.Empty;

    /// <summary>
    /// Number of halves in regular time
    /// </summary>
    public int NumberOfHalves { get; set; } = 2;

    /// <summary>
    /// Duration of each half in minutes
    /// </summary>
    public int HalfDurationMinutes { get; set; } = 45;

    /// <summary>
    /// Number of players on the field per team
    /// </summary>
    public int PlayersOnField { get; set; } = 11;

    /// <summary>
    /// Whether a goalkeeper is required
    /// </summary>
    public bool RequireGoalkeeper { get; set; } = true;

    /// <summary>
    /// Maximum number of substitutions allowed
    /// </summary>
    public int MaxSubstitutions { get; set; }

    /// <summary>
    /// Whether officials must be assigned before kickoff
    /// </summary>
    public bool RequireOfficialsToStart { get; set; }

    /// <summary>
    /// Whether extra time is allowed
    /// </summary>
    public bool AllowExtraTime { get; set; }

    /// <summary>
    /// Number of extra-time halves
    /// </summary>
    public int ExtraTimeHalfCount { get; set; } = 2;

    /// <summary>
    /// Duration of each extra-time half in minutes
    /// </summary>
    public int ExtraTimeHalfDurationMinutes { get; set; } = 15;

    /// <summary>
    /// Whether a penalty shootout is allowed
    /// </summary>
    public bool AllowPenaltyShootout { get; set; }

    /// <summary>
    /// Points awarded for a win
    /// </summary>
    public int WinPoints { get; set; } = 3;

    /// <summary>
    /// Points awarded for a draw
    /// </summary>
    public int DrawPoints { get; set; } = 1;

    /// <summary>
    /// Points awarded for a loss
    /// </summary>
    public int LossPoints { get; set; }

    /// <summary>
    /// Audience / age-group category for the season
    /// </summary>
    public TeamCategory? TeamCategory { get; set; }
}
