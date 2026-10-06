using Application.Common;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Common.Statistics;
using Application.Features.Football.Statistics.Queries;
using Domain.Common;
using Domain.Repositories.Football;
using MediatR;

namespace Application.Features.Football.Statistics.Handlers;

/// <summary>
/// Lists football teams for the all-time statistics team filter.
/// </summary>
public class GetFootballAllTimeTeamsHandler
    : IRequestHandler<GetFootballAllTimeTeamsQuery, Result<List<AllTimeTeamOptionDto>>>
{
    private readonly IFootballStatisticsRepository _statisticsRepository;

    public GetFootballAllTimeTeamsHandler(IFootballStatisticsRepository statisticsRepository)
    {
        _statisticsRepository = statisticsRepository;
    }

    public async Task<Result<List<AllTimeTeamOptionDto>>> Handle(
        GetFootballAllTimeTeamsQuery request,
        CancellationToken cancellationToken)
    {
        List<AllTimePlayerStatRow> rows = await _statisticsRepository.GetAllTimePlayerStatRowsAsync(
            request.TeamCategory,
            request.CompetitionType,
            cancellationToken);

        return Result<List<AllTimeTeamOptionDto>>.Success(AllTimePlayerStatistics.Teams(rows));
    }
}
