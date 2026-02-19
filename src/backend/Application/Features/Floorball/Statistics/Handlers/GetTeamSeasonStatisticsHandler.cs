using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Teams.Mappings;
using Application.Features.Floorball.Players.Mappings;
using Application.Features.Floorball.Referees.Mappings;
using Application.Features.Floorball.TeamManagers.Mappings;
using Application.Features.Floorball.Statistics.Mappings;
using Application.Features.Floorball.Statistics.Queries;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Statistics.Handlers;

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

            FloorballTeamSeasonStatisticsDto dto = FloorballStatisticsMapper.ToDto(statistics);
            
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
