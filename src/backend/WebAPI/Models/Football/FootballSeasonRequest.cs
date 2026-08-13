using System.ComponentModel.DataAnnotations;
using Domain.Enums.Common;
using WebAPI.Models.Common.Pagination;

namespace WebAPI.Models.Football;

public record GetFootballSeasonsPagedRequest : PagedRequestBase
{
    [StringLength(20)]
    public string? SeasonYear { get; init; }

    public TeamCategory? TeamCategory { get; init; }
}

public class CreateFootballSeasonRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string StartDate { get; set; } = string.Empty;

    [Required]
    public string EndDate { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "At least one division must be specified.")]
    public List<Guid> DivisionIds { get; set; } = new();

    public int NumberOfHalves { get; set; } = 2;
    public int HalfDurationMinutes { get; set; } = 45;
    public int PlayersOnField { get; set; } = 11;
    public bool RequireGoalkeeper { get; set; } = true;
    public int MaxSubstitutions { get; set; }
    public bool RequireOfficialsToStart { get; set; }
    public bool AllowExtraTime { get; set; }
    public int ExtraTimeHalfCount { get; set; } = 2;
    public int ExtraTimeHalfDurationMinutes { get; set; } = 15;
    public bool AllowPenaltyShootout { get; set; }
    public int WinPoints { get; set; } = 3;
    public int DrawPoints { get; set; } = 1;
    public int LossPoints { get; set; }
    public TeamCategory TeamCategory { get; set; } = TeamCategory.Adult;
}

public class UpdateFootballSeasonRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string StartDate { get; set; } = string.Empty;

    [Required]
    public string EndDate { get; set; } = string.Empty;

    public int NumberOfHalves { get; set; } = 2;
    public int HalfDurationMinutes { get; set; } = 45;
    public int PlayersOnField { get; set; } = 11;
    public bool RequireGoalkeeper { get; set; } = true;
    public int MaxSubstitutions { get; set; }
    public bool RequireOfficialsToStart { get; set; }
    public bool AllowExtraTime { get; set; }
    public int ExtraTimeHalfCount { get; set; } = 2;
    public int ExtraTimeHalfDurationMinutes { get; set; } = 15;
    public bool AllowPenaltyShootout { get; set; }
    public int WinPoints { get; set; } = 3;
    public int DrawPoints { get; set; } = 1;
    public int LossPoints { get; set; }
    public TeamCategory? TeamCategory { get; set; }
}
