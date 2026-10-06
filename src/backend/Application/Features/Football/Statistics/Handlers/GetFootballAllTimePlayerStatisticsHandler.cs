using Application.Common;
using Application.Features.Common.Statistics;
using Application.Features.Football.Statistics.DTOs;
using Application.Features.Football.Statistics.Queries;
using Domain.Common;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Statistics.Handlers;

/// <summary>
/// Sums public football competition statistics into a paged all-time player list.
/// </summary>
public class GetFootballAllTimePlayerStatisticsHandler
    : IRequestHandler<GetFootballAllTimePlayerStatisticsQuery, Result<PagedResult<FootballAllTimePlayerStatisticsDto>>>
{
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetFootballAllTimePlayerStatisticsHandler> _logger;

    public GetFootballAllTimePlayerStatisticsHandler(
        IFootballStatisticsRepository statisticsRepository,
        IPersonRepository personRepository,
        ILogger<GetFootballAllTimePlayerStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<FootballAllTimePlayerStatisticsDto>>> Handle(
        GetFootballAllTimePlayerStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting football all-time player statistics page {Page} sorted by {Sort} {Direction}",
            request.Page,
            request.Sort,
            request.Direction);

        PagedResult<FootballAllTimePlayerStatisticsDto> page = await AllTimePlayerStatisticsPager.PageAsync(
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
            (player, playerName, rank) => new FootballAllTimePlayerStatisticsDto(
                rank,
                player.PlayerId,
                playerName,
                player.TeamName,
                player.GamesPlayed,
                player.Goals,
                player.Assists,
                player.Points,
                player.YellowCards,
                player.RedCards),
            cancellationToken);

        return Result<PagedResult<FootballAllTimePlayerStatisticsDto>>.Success(page);
    }
}
