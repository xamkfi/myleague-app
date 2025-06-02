using Application.Queries.Floorball;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Floorball.Players;

/// <summary>
/// Handler for retrieving floorball players by team ID
/// </summary>
public class GetFloorballPlayersByTeamIdHandler : IRequestHandler<GetFloorballPlayersByTeamIdQuery, Result<IEnumerable<FloorballPlayerDto>>>
{
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly ILogger<GetFloorballPlayersByTeamIdHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFloorballPlayersByTeamIdHandler class
    /// </summary>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="logger">The logger</param>
    public GetFloorballPlayersByTeamIdHandler(
        IFloorballPlayerRepository playerRepository,
        IFloorballTeamRepository teamRepository,
        ILogger<GetFloorballPlayersByTeamIdHandler> logger)
    {
        _playerRepository = playerRepository;
        _teamRepository = teamRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFloorballPlayersByTeamIdQuery request
    /// </summary>
    /// <param name="request">The query containing the team ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Players in the team as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FloorballPlayerDto>>> Handle(GetFloorballPlayersByTeamIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify team exists
            bool teamExists = await _teamRepository.ExistsAsync(request.TeamId);
            if (!teamExists)
            {
                _logger.LogWarning("Attempt to get players for non-existent team with ID: {TeamId}", request.TeamId);
                return Result<IEnumerable<FloorballPlayerDto>>.NotFound("FloorballTeam", request.TeamId);
            }

            _logger.LogInformation("Retrieving floorball players for team: {TeamId}", request.TeamId);
            
            IEnumerable<FloorballPlayer> players = await _playerRepository.GetByTeamIdAsync(request.TeamId);
            IEnumerable<FloorballPlayerDto> playerDtos = FloorballPlayerMapper.ToDtos(players);
            
            _logger.LogInformation("Successfully retrieved {PlayerCount} players for team {TeamId}", 
                playerDtos.Count(), request.TeamId);
            
            return Result<IEnumerable<FloorballPlayerDto>>.Success(playerDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball players for team: {TeamId}", request.TeamId);
            return Result<IEnumerable<FloorballPlayerDto>>.Failure("An error occurred while retrieving the team's players.");
        }
    }
} 