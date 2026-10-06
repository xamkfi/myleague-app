using Application.Common;
using Application.Features.Hockey.Statistics.DTOs;
using Domain.Common;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Hockey.Statistics.Queries;

/// <summary>
/// Paged all-time hockey player statistics.
/// </summary>
public record GetHockeyAllTimePlayerStatisticsQuery(
    int Page = 1,
    int PageSize = 25,
    TeamCategory TeamCategory = TeamCategory.Adult,
    AllTimeCompetitionFilter CompetitionType = AllTimeCompetitionFilter.Season,
    AllTimeStatSort Sort = AllTimeStatSort.Points,
    AllTimeSortDirection Direction = AllTimeSortDirection.Desc,
    string? Search = null,
    Guid? TeamId = null
) : IRequest<Result<PagedResult<HockeyAllTimePlayerStatisticsDto>>>;
