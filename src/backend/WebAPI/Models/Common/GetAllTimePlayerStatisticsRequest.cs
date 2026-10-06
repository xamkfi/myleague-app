using System.ComponentModel.DataAnnotations;
using Domain.Enums.Common;

namespace WebAPI.Models.Common;

/// <summary>
/// Query parameters for a public all-time player statistics page.
/// </summary>
public record GetAllTimePlayerStatisticsRequest
{
    /// <summary>
    /// Page number, starting at 1.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; init; } = 1;

    /// <summary>
    /// Number of players per page.
    /// </summary>
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; init; } = 25;

    /// <summary>
    /// Team category. Defaults to adult competitions.
    /// </summary>
    public TeamCategory TeamCategory { get; init; } = TeamCategory.Adult;

    /// <summary>
    /// Seasons, tournaments, or both. Defaults to seasons.
    /// </summary>
    public AllTimeCompetitionFilter CompetitionType { get; init; } = AllTimeCompetitionFilter.Season;

    /// <summary>
    /// Column to sort by. Defaults to points.
    /// </summary>
    public AllTimeStatSort Sort { get; init; } = AllTimeStatSort.Points;

    /// <summary>
    /// Sort direction. Defaults to descending.
    /// </summary>
    public AllTimeSortDirection Direction { get; init; } = AllTimeSortDirection.Desc;

    /// <summary>
    /// Optional player name filter. Matching players keep their rank in the full list.
    /// </summary>
    [StringLength(100, ErrorMessage = "Search must be at most 100 characters")]
    public string? Search { get; init; }

    /// <summary>
    /// Optional team. When set, only that team's statistics are summed and ranked.
    /// </summary>
    public Guid? TeamId { get; init; }
}
