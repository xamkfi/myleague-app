using Application.Common;
using Application.Features.Football.Statistics.DTOs;
using Application.Features.Football.Statistics.Mappings;
using Application.Features.Football.Statistics.Queries;
using Domain.Entities.Football.Statistics;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Statistics.Handlers;

/// <summary>
/// Handler for retrieving match statistics
/// </summary>
public class GetMatchStatisticsHandler : IRequestHandler<GetFootballMatchStatisticsQuery, Result<List<FootballMatchTeamStatisticsDto>>>
{
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetMatchStatisticsHandler> _logger;

    public GetMatchStatisticsHandler(
        IFootballStatisticsRepository statisticsRepository,
        ILogger<GetMatchStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    public async Task<Result<List<FootballMatchTeamStatisticsDto>>> Handle(GetFootballMatchStatisticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting match statistics for Match: {MatchId}", request.MatchId);

        List<FootballMatchTeamStatistics> matchStats =
            (await _statisticsRepository.GetMatchStatisticsAsync(request.MatchId, cancellationToken)).ToList();

        List<FootballMatchTeamStatisticsDto> dtos = matchStats
            .Select(ms => FootballStatisticsMapper.ToDto(ms))
            .ToList();

        return Result<List<FootballMatchTeamStatisticsDto>>.Success(dtos);
    }
}
