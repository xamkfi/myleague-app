using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Queries.Floorball.Statistics;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Floorball.Statistics;

/// <summary>
/// Handler for retrieving player season statistics
/// </summary>
public class GetPlayerSeasonStatisticsHandler : IRequestHandler<GetPlayerSeasonStatisticsQuery, Result<FloorballPlayerSeasonStatisticsDto>>
{
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetPlayerSeasonStatisticsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetPlayerSeasonStatisticsHandler class
    /// </summary>
    /// <param name="statisticsRepository">The statistics repository</param>
    /// <param name="logger">The logger</param>
    public GetPlayerSeasonStatisticsHandler(
        IFloorballStatisticsRepository statisticsRepository,
        ILogger<GetPlayerSeasonStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetPlayerSeasonStatisticsQuery request
    /// </summary>
    /// <param name="request">The query containing player and season IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing player season statistics DTO</returns>
    public async Task<Result<FloorballPlayerSeasonStatisticsDto>> Handle(GetPlayerSeasonStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting player season statistics for Player: {PlayerId} in Season: {SeasonId}", request.PlayerId, request.SeasonId);

            // Note: We need to find the team ID for this player in this season
            // This is a limitation of our current design - we might need to adjust the repository method
            IEnumerable<Domain.Entities.Floorball.FloorballPlayerSeasonStatistics> allPlayerStats = 
                await _statisticsRepository.GetPlayerStatisticsBySeasonAsync(request.SeasonId, cancellationToken);

            Domain.Entities.Floorball.FloorballPlayerSeasonStatistics? playerStats = 
                allPlayerStats.FirstOrDefault(ps => ps.PlayerId == request.PlayerId);

            if (playerStats == null)
            {
                _logger.LogWarning("Player season statistics not found for Player: {PlayerId} in Season: {SeasonId}", request.PlayerId, request.SeasonId);
                return Result<FloorballPlayerSeasonStatisticsDto>.NotFound("Player season statistics", $"Player {request.PlayerId} in season {request.SeasonId}");
            }

            FloorballPlayerSeasonStatisticsDto dto = FloorballStatisticsMapper.ToDto(playerStats);
            
            _logger.LogInformation("Successfully retrieved player season statistics for Player: {PlayerId} in Season: {SeasonId}", request.PlayerId, request.SeasonId);
            return Result<FloorballPlayerSeasonStatisticsDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting player season statistics for Player: {PlayerId} in Season: {SeasonId}", request.PlayerId, request.SeasonId);
            return Result<FloorballPlayerSeasonStatisticsDto>.Failure("An error occurred while retrieving player season statistics.");
        }
    }
}
