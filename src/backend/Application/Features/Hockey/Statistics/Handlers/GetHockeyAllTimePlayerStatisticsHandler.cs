using Application.Common;
using Application.Features.Common.Statistics;
using Application.Features.Hockey.Statistics.DTOs;
using Application.Features.Hockey.Statistics.Queries;
using Domain.Common;
using Domain.Repositories.Common;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Statistics.Handlers;

/// <summary>
/// Sums public hockey competition statistics into a paged all-time player list.
/// </summary>
public class GetHockeyAllTimePlayerStatisticsHandler
    : IRequestHandler<GetHockeyAllTimePlayerStatisticsQuery, Result<PagedResult<HockeyAllTimePlayerStatisticsDto>>>
{
    private readonly IHockeyStatisticsRepository _statisticsRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetHockeyAllTimePlayerStatisticsHandler> _logger;

    public GetHockeyAllTimePlayerStatisticsHandler(
        IHockeyStatisticsRepository statisticsRepository,
        IPersonRepository personRepository,
        ILogger<GetHockeyAllTimePlayerStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<HockeyAllTimePlayerStatisticsDto>>> Handle(
        GetHockeyAllTimePlayerStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting hockey all-time player statistics page {Page} sorted by {Sort} {Direction}",
            request.Page,
            request.Sort,
            request.Direction);

        PagedResult<HockeyAllTimePlayerStatisticsDto> page = await AllTimePlayerStatisticsPager.PageAsync(
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
            (player, playerName, rank) => new HockeyAllTimePlayerStatisticsDto(
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

        return Result<PagedResult<HockeyAllTimePlayerStatisticsDto>>.Success(page);
    }
}
