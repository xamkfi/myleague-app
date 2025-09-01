using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Queries.Floorball.Statistics;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Floorball.Statistics;

/// <summary>
/// Handler for retrieving team season statistics
/// </summary>
public class GetTeamSeasonStatisticsHandler : IRequestHandler<GetTeamSeasonStatisticsQuery, Result<FloorballTeamSeasonStatisticsDto>>
{
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly ILogger<GetTeamSeasonStatisticsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetTeamSeasonStatisticsHandler class
    /// </summary>
    /// <param name="statisticsRepository">The statistics repository</param>
    /// <param name="teamRepository">The team repository</param>
    /// <param name="seasonRepository">The season repository</param>
    /// <param name="logger">The logger</param>
    public GetTeamSeasonStatisticsHandler(
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballSeasonRepository seasonRepository,
        ILogger<GetTeamSeasonStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _teamRepository = teamRepository;
        _seasonRepository = seasonRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetTeamSeasonStatisticsQuery request
    /// </summary>
    /// <param name="request">The query containing team and season IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Team statistics DTO wrapped in a Result</returns>
    public async Task<Result<FloorballTeamSeasonStatisticsDto>> Handle(GetTeamSeasonStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving team statistics for Team {TeamId} in Season {SeasonId}", request.TeamId, request.SeasonId);

            Domain.Entities.Floorball.FloorballTeamSeasonStatistics? statistics = await _statisticsRepository.GetTeamSeasonStatisticsAsync(request.TeamId, request.SeasonId, cancellationToken);
            
            if (statistics == null)
            {
                _logger.LogInformation("No statistics found for Team {TeamId} in Season {SeasonId}", request.TeamId, request.SeasonId);
                return Result<FloorballTeamSeasonStatisticsDto>.NotFound("TeamSeasonStatistics", $"TeamId: {request.TeamId}, SeasonId: {request.SeasonId}");
            }

            // Get team and season names for the DTO
            Domain.Entities.Floorball.FloorballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            Domain.Entities.Floorball.FloorballSeason? season = await _seasonRepository.GetByIdAsync(request.SeasonId);

            FloorballTeamSeasonStatisticsDto dto = FloorballStatisticsMapper.ToDto(statistics, team?.Name, season?.Name);
            
            _logger.LogInformation("Successfully retrieved team statistics for Team {TeamId} in Season {SeasonId}", request.TeamId, request.SeasonId);
            return Result<FloorballTeamSeasonStatisticsDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving team statistics for Team {TeamId} in Season {SeasonId}", request.TeamId, request.SeasonId);
            return Result<FloorballTeamSeasonStatisticsDto>.Failure("An error occurred while retrieving team statistics.");
        }
    }
}
