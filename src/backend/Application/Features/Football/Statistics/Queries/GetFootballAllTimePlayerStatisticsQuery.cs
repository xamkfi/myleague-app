using Application.Common;
using Application.Features.Football.Statistics.DTOs;
using Domain.Common;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Football.Statistics.Queries;

/// <summary>
/// Paged all-time football player statistics.
/// </summary>
public record GetFootballAllTimePlayerStatisticsQuery(
    int Page = 1,
    int PageSize = 25,
    TeamCategory TeamCategory = TeamCategory.Adult,
    AllTimeCompetitionFilter CompetitionType = AllTimeCompetitionFilter.Season,
    AllTimeStatSort Sort = AllTimeStatSort.Points,
    AllTimeSortDirection Direction = AllTimeSortDirection.Desc,
    string? Search = null,
    Guid? TeamId = null
) : IRequest<Result<PagedResult<FootballAllTimePlayerStatisticsDto>>>;
