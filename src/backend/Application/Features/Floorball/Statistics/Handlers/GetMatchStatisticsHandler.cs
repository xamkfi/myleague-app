using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Queries.Floorball.Statistics;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Floorball.Statistics;

/// <summary>
/// Handler for retrieving match statistics
/// </summary>
public class GetMatchStatisticsHandler : IRequestHandler<GetMatchStatisticsQuery, Result<List<FloorballMatchTeamStatisticsDto>>>
{
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetMatchStatisticsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetMatchStatisticsHandler class
    /// </summary>
    /// <param name="statisticsRepository">The statistics repository</param>
    /// <param name="logger">The logger</param>
    public GetMatchStatisticsHandler(
        IFloorballStatisticsRepository statisticsRepository,
        ILogger<GetMatchStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetMatchStatisticsQuery request
    /// </summary>
    /// <param name="request">The query containing the match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing list of match team statistics DTOs</returns>
    public async Task<Result<List<FloorballMatchTeamStatisticsDto>>> Handle(GetMatchStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting match statistics for Match: {MatchId}", request.MatchId);

            List<Domain.Entities.Floorball.FloorballMatchTeamStatistics> matchStats = 
                (await _statisticsRepository.GetMatchStatisticsAsync(request.MatchId, cancellationToken)).ToList();

            if (matchStats == null || matchStats.Count == 0)
            {
                _logger.LogWarning("Match statistics not found for Match: {MatchId}", request.MatchId);
                return Result<List<FloorballMatchTeamStatisticsDto>>.NotFound("Match statistics", request.MatchId.ToString());
            }

            List<FloorballMatchTeamStatisticsDto> dtos = matchStats
                .Select(ms => FloorballStatisticsMapper.ToDto(ms))
                .ToList();
            
            _logger.LogInformation("Successfully retrieved match statistics for Match: {MatchId} - {Count} team statistics", request.MatchId, dtos.Count);
            return Result<List<FloorballMatchTeamStatisticsDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting match statistics for Match: {MatchId}", request.MatchId);
            return Result<List<FloorballMatchTeamStatisticsDto>>.Failure("An error occurred while retrieving match statistics.");
        }
    }
}
