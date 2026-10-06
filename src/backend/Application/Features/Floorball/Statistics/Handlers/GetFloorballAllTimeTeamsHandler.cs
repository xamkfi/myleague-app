using Application.Common;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Common.Statistics;
using Application.Features.Floorball.Statistics.Queries;
using Domain.Common;
using Domain.Repositories.Floorball;
using MediatR;

namespace Application.Features.Floorball.Statistics.Handlers;

/// <summary>
/// Lists floorball teams for the all-time statistics team filter.
/// </summary>
public class GetFloorballAllTimeTeamsHandler
    : IRequestHandler<GetFloorballAllTimeTeamsQuery, Result<List<AllTimeTeamOptionDto>>>
{
    private readonly IFloorballStatisticsRepository _statisticsRepository;

    public GetFloorballAllTimeTeamsHandler(IFloorballStatisticsRepository statisticsRepository)
    {
        _statisticsRepository = statisticsRepository;
    }

    public async Task<Result<List<AllTimeTeamOptionDto>>> Handle(
        GetFloorballAllTimeTeamsQuery request,
        CancellationToken cancellationToken)
    {
        List<AllTimePlayerStatRow> rows = await _statisticsRepository.GetAllTimePlayerStatRowsAsync(
            request.TeamCategory,
            request.CompetitionType,
            cancellationToken);

        return Result<List<AllTimeTeamOptionDto>>.Success(AllTimePlayerStatistics.Teams(rows));
    }
}
