using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Queries.Floorball.Statistics;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Floorball.Statistics;

/// <summary>
/// Handler for retrieving top scorers for a season
/// </summary>
public class GetTopScorersHandler : IRequestHandler<GetTopScorersQuery, Result<List<FloorballPlayerSeasonStatisticsDto>>>
{
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetTopScorersHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetTopScorersHandler class
    /// </summary>
    /// <param name="statisticsRepository">The statistics repository</param>
    /// <param name="logger">The logger</param>
    public GetTopScorersHandler(
        IFloorballStatisticsRepository statisticsRepository,
        ILogger<GetTopScorersHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetTopScorersQuery request
    /// </summary>
    /// <param name="request">The query containing season ID and top N count</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of top scoring players wrapped in a Result</returns>
    public async Task<Result<List<FloorballPlayerSeasonStatisticsDto>>> Handle(GetTopScorersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving top {TopN} scorers for Season {SeasonId}", request.TopN, request.SeasonId);

            var topScorers = await _statisticsRepository.GetTopScorersAsync(request.SeasonId, request.TopN, cancellationToken);
            
            var dtos = topScorers.Select(stats => FloorballStatisticsMapper.ToDto(stats)).ToList();
            
            _logger.LogInformation("Successfully retrieved {Count} top scorers for Season {SeasonId}", dtos.Count, request.SeasonId);
            return Result<List<FloorballPlayerSeasonStatisticsDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving top scorers for Season {SeasonId}", request.SeasonId);
            return Result<List<FloorballPlayerSeasonStatisticsDto>>.Failure("An error occurred while retrieving top scorers.");
        }
    }
}
