using Application.Common;
using Application.Features.Common.Statistics;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Statistics.Queries;
using Domain.Common;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Statistics.Handlers;

/// <summary>
/// Sums public floorball competition statistics into a paged all-time player list.
/// </summary>
public class GetFloorballAllTimePlayerStatisticsHandler
    : IRequestHandler<GetFloorballAllTimePlayerStatisticsQuery, Result<PagedResult<FloorballAllTimePlayerStatisticsDto>>>
{
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetFloorballAllTimePlayerStatisticsHandler> _logger;

    public GetFloorballAllTimePlayerStatisticsHandler(
        IFloorballStatisticsRepository statisticsRepository,
        IPersonRepository personRepository,
        ILogger<GetFloorballAllTimePlayerStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<FloorballAllTimePlayerStatisticsDto>>> Handle(
        GetFloorballAllTimePlayerStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting floorball all-time player statistics page {Page} sorted by {Sort} {Direction}",
            request.Page,
            request.Sort,
            request.Direction);

        PagedResult<FloorballAllTimePlayerStatisticsDto> page = await AllTimePlayerStatisticsPager.PageAsync(
            (pageRequest, ct) => _statisticsRepository.GetAllTimePlayerPageAsync(
                request.TeamCategory,
                request.CompetitionType,
                pageRequest,
                ct),
            new AllTimePageRequest(
                request.Page,
                request.PageSize,
                request.Sort,
                request.Direction,
                request.Search,
                request.TeamId),
            _personRepository,
            (player, playerName, rank) => new FloorballAllTimePlayerStatisticsDto(
                rank,
                player.PlayerId,
                playerName,
                player.TeamName,
                player.GamesPlayed,
                player.Goals,
                player.Assists,
                player.Points,
                player.PenaltyMinutes),
            cancellationToken);

        return Result<PagedResult<FloorballAllTimePlayerStatisticsDto>>.Success(page);
    }
}
