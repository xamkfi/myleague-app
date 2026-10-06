using Application.Common;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Common.Statistics;
using Application.Features.Hockey.Statistics.Queries;
using Domain.Common;
using Domain.Repositories.Hockey;
using MediatR;

namespace Application.Features.Hockey.Statistics.Handlers;

/// <summary>
/// Lists hockey teams for the all-time statistics team filter.
/// </summary>
public class GetHockeyAllTimeTeamsHandler
    : IRequestHandler<GetHockeyAllTimeTeamsQuery, Result<List<AllTimeTeamOptionDto>>>
{
    private readonly IHockeyStatisticsRepository _statisticsRepository;

    public GetHockeyAllTimeTeamsHandler(IHockeyStatisticsRepository statisticsRepository)
    {
        _statisticsRepository = statisticsRepository;
    }

    public async Task<Result<List<AllTimeTeamOptionDto>>> Handle(
        GetHockeyAllTimeTeamsQuery request,
        CancellationToken cancellationToken)
    {
        List<AllTimePlayerStatRow> rows = await _statisticsRepository.GetAllTimePlayerStatRowsAsync(
            request.TeamCategory,
            request.CompetitionType,
            cancellationToken);

        return Result<List<AllTimeTeamOptionDto>>.Success(AllTimePlayerStatistics.Teams(rows));
    }
}
