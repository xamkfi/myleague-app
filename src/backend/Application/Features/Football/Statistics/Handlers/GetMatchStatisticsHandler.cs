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
public class GetMatchStatisticsHandler : IRequestHandler<GetMatchStatisticsQuery, Result<List<FootballMatchTeamStatisticsDto>>>
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

    public async Task<Result<List<FootballMatchTeamStatisticsDto>>> Handle(GetMatchStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting match statistics for Match: {MatchId}", request.MatchId);

            List<FootballMatchTeamStatistics> matchStats =
                (await _statisticsRepository.GetMatchStatisticsAsync(request.MatchId, cancellationToken)).ToList();

            if (matchStats.Count == 0)
            {
                _logger.LogWarning("Match statistics not found for Match: {MatchId}", request.MatchId);
                return Result<List<FootballMatchTeamStatisticsDto>>.NotFound("Match statistics", request.MatchId.ToString());
            }

            List<FootballMatchTeamStatisticsDto> dtos = matchStats
                .Select(ms => FootballStatisticsMapper.ToDto(ms))
                .ToList();

            _logger.LogInformation("Successfully retrieved match statistics for Match: {MatchId} - {Count} team statistics", request.MatchId, dtos.Count);
            return Result<List<FootballMatchTeamStatisticsDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting match statistics for Match: {MatchId}", request.MatchId);
            return Result<List<FootballMatchTeamStatisticsDto>>.Failure("An error occurred while retrieving match statistics.");
        }
    }
}
