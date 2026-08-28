using System;
using System.ComponentModel.DataAnnotations;
using Domain.Enums.Floorball;
using WebAPI.Models.Common.Pagination;

namespace WebAPI.Models.Floorball
{
    /// <summary>
    /// Request for paginated public floorball season listing.
    /// </summary>
    public record GetFloorballSeasonsPagedRequest : PagedRequestBase
    {
        /// <summary>
        /// Optional season-year filter, e.g. "2024" or "2024-2025".
        /// </summary>
        [StringLength(20)]
        public string? SeasonYear { get; init; }

        /// <summary>
        /// Optional audience / age-group category filter.
        /// </summary>
        public Domain.Enums.Common.TeamCategory? TeamCategory { get; init; }
    }

    /// <summary>
    /// Request model for creating a floorball season
    /// </summary>
    public class CreateFloorballSeasonRequest
    {
        /// <summary>
        /// Name of the season
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Start date of the season
        /// </summary>
        [Required]
        public string StartDate { get; set; } = string.Empty;

        /// <summary>
        /// End date of the season
        /// </summary>
        [Required]
        public string EndDate { get; set; } = string.Empty;

        /// <summary>
        /// List of division IDs to associate with this season. At least one division must be provided.
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one division must be specified.")]
        public List<Guid> DivisionIds { get; set; } = new();

        /// <summary>
        /// Number of regular periods (e.g., 2 or 3). Default: 2.
        /// </summary>
        public int NumberOfPeriods { get; set; } = 2;

        /// <summary>
        /// Duration in minutes per regular period. Default: 15.
        /// </summary>
        public int PeriodDurationMinutes { get; set; } = 15;

        /// <summary>
        /// Whether overtime is allowed when the match is tied. Default: true.
        /// </summary>
        public bool AllowOvertime { get; set; } = true;

        /// <summary>
        /// Duration in minutes for the overtime period. Default: 5.
        /// </summary>
        public int OvertimeDurationMinutes { get; set; } = 5;

        /// <summary>
        /// Whether shootout is allowed after overtime. Default: true.
        /// </summary>
        public bool AllowShootout { get; set; } = true;

        /// <summary>
        /// Audience / age-group category. Default: Adult.
        /// </summary>
        public Domain.Enums.Common.TeamCategory TeamCategory { get; set; } = Domain.Enums.Common.TeamCategory.Adult;
    }

    /// <summary>
    /// Request model for updating a floorball season
    /// </summary>
    public class UpdateFloorballSeasonRequest
    {
        /// <summary>
        /// Name of the season
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Start date of the season
        /// </summary>
        [Required]
        public string StartDate { get; set; } = string.Empty;

        /// <summary>
        /// End date of the season
        /// </summary>
        [Required]
        public string EndDate { get; set; } = string.Empty;

        /// <summary>
        /// Number of regular periods (e.g., 2 or 3). Default: 2.
        /// </summary>
        public int NumberOfPeriods { get; set; } = 2;

        /// <summary>
        /// Duration in minutes per regular period. Default: 15.
        /// </summary>
        public int PeriodDurationMinutes { get; set; } = 15;

        /// <summary>
        /// Whether overtime is allowed when the match is tied. Default: true.
        /// </summary>
        public bool AllowOvertime { get; set; } = true;

        /// <summary>
        /// Duration in minutes for the overtime period. Default: 5.
        /// </summary>
        public int OvertimeDurationMinutes { get; set; } = 5;

        /// <summary>
        /// Whether shootout is allowed after overtime. Default: true.
        /// </summary>
        public bool AllowShootout { get; set; } = true;

        /// <summary>
        /// Audience / age-group category. When omitted, the existing value is kept.
        /// </summary>
        public Domain.Enums.Common.TeamCategory? TeamCategory { get; set; }
    }

    /// <summary>
    /// One season intro block in a replace-all payload.
    /// </summary>
    public class FloorballSeasonContentBlockItemRequest
    {
        /// <summary>
        /// Existing block id. Omit to create a new block.
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Card title shown on public pages.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// HTML body produced by the rich-text editor.
        /// </summary>
        [StringLength(50000)]
        public string ContentHtml { get; set; } = string.Empty;
    }

    /// <summary>
    /// Replace-all request for a season's intro blocks. Array order is the display order.
    /// </summary>
    public class ReplaceFloorballSeasonContentBlocksRequest
    {
        /// <summary>
        /// Intro blocks in display order.
        /// </summary>
        [Required]
        public List<FloorballSeasonContentBlockItemRequest> Items { get; set; } = new();
    }
}
