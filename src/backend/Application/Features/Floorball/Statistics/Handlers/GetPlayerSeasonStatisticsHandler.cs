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
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Statistics.Handlers;

/// <summary>
/// Handler for retrieving player season statistics
/// </summary>
public class GetPlayerSeasonStatisticsHandler : IRequestHandler<GetPlayerSeasonStatisticsQuery, Result<FloorballPlayerSeasonStatisticsDto>>
{
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballPlayerRepository _floorballPlayerRepository;
    private readonly IFloorballTeamRepository _floorballTeamRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetPlayerSeasonStatisticsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetPlayerSeasonStatisticsHandler class
    /// </summary>
    /// <param name="statisticsRepository">The statistics repository</param>
    /// <param name="logger">The logger</param>
    public GetPlayerSeasonStatisticsHandler(
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballPlayerRepository floorballPlayerRepository,
        IFloorballTeamRepository floorballTeamRepository,
        IPersonRepository personRepository,
        ILogger<GetPlayerSeasonStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _floorballPlayerRepository = floorballPlayerRepository;
        _floorballTeamRepository = floorballTeamRepository;
        _personRepository = personRepository;
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
            _logger.LogInformation("Getting player season statistics for Player: {PlayerId} in Season: {SeasonId}", request.PlayerId, request.CompetitionId);

            // Note: We need to find the team ID for this player in this season
            // This is a limitation of our current design - we might need to adjust the repository method
            IEnumerable<Domain.Entities.Floorball.FloorballPlayerSeasonStatistics> allPlayerStats = 
                await _statisticsRepository.GetPlayerStatisticsByCompetitionAsync(request.CompetitionId, cancellationToken);

            Domain.Entities.Floorball.FloorballPlayerSeasonStatistics? playerStats = 
                allPlayerStats.FirstOrDefault(ps => ps.PlayerId == request.PlayerId);

            if (playerStats == null)
            {
                _logger.LogWarning("Player season statistics not found for Player: {PlayerId} in Season: {SeasonId}", request.PlayerId, request.CompetitionId);
                return Result<FloorballPlayerSeasonStatisticsDto>.NotFound("Player season statistics", $"Player {request.PlayerId} in season {request.CompetitionId}");
            }

            Person? person = await _personRepository.GetByIdAsync(playerStats.Player.PersonId); 

            if(person == null)
            {
                _logger.LogWarning("Player season statistics not found for Player: {PlayerId} in Season: {SeasonId}", request.PlayerId, request.CompetitionId);
                return Result<FloorballPlayerSeasonStatisticsDto>.NotFound("Player season statistics", $"Player {request.PlayerId} in season {request.CompetitionId}");
            }

            FloorballPlayerSeasonStatisticsDto dto = FloorballStatisticsMapper.ToDto(playerStats, person.FullName);
            
            _logger.LogInformation("Successfully retrieved player season statistics for Player: {PlayerId} in Season: {SeasonId}", request.PlayerId, request.CompetitionId);
            return Result<FloorballPlayerSeasonStatisticsDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting player season statistics for Player: {PlayerId} in Season: {SeasonId}", request.PlayerId, request.CompetitionId);
            return Result<FloorballPlayerSeasonStatisticsDto>.Failure("An error occurred while retrieving player season statistics.");
        }
    }
}
