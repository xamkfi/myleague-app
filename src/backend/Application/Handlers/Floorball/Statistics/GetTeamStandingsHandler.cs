using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Queries.Floorball.Statistics;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Floorball.Statistics;

/// <summary>
/// Handler for retrieving team standings
/// </summary>
public class GetTeamStandingsHandler : IRequestHandler<GetTeamStandingsQuery, Result<List<FloorballTeamSeasonStatisticsDto>>>
{
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetTeamStandingsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetTeamStandingsHandler class
    /// </summary>
    /// <param name="statisticsRepository">The statistics repository</param>
    /// <param name="logger">The logger</param>
    public GetTeamStandingsHandler(
        IFloorballStatisticsRepository statisticsRepository,
        ILogger<GetTeamStandingsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetTeamStandingsQuery request
    /// </summary>
    /// <param name="request">The query containing the season ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing list of team season statistics DTOs ordered by standings</returns>
    public async Task<Result<List<FloorballTeamSeasonStatisticsDto>>> Handle(GetTeamStandingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting team standings for Season: {SeasonId}", request.SeasonId);

            List<Domain.Entities.Floorball.FloorballTeamSeasonStatistics> standings = 
                (await _statisticsRepository.GetTeamStandingsAsync(request.SeasonId, cancellationToken)).ToList();

            if (standings.Count == 0)
            {
                _logger.LogWarning("Team standings not found for Season: {SeasonId}", request.SeasonId);
                return Result<List<FloorballTeamSeasonStatisticsDto>>.NotFound("Team standings", request.SeasonId.ToString());
            }

            List<FloorballTeamSeasonStatisticsDto> standingsDtos = standings
                .Select(ts => FloorballStatisticsMapper.ToDto(ts))
                .ToList();
            
            _logger.LogInformation("Successfully retrieved team standings for Season: {SeasonId} - {Count} teams", request.SeasonId, standingsDtos.Count);
            return Result<List<FloorballTeamSeasonStatisticsDto>>.Success(standingsDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting team standings for Season: {SeasonId}", request.SeasonId);
            return Result<List<FloorballTeamSeasonStatisticsDto>>.Failure("An error occurred while retrieving team standings.");
        }
    }
}
