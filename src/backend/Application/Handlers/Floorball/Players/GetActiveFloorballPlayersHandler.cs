using Application.Queries.Floorball.Player;
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
using System.Linq;

namespace Application.Handlers.Floorball.Players;

/// <summary>
/// Handler for retrieving active floorball players
/// </summary>
public class GetActiveFloorballPlayersHandler : IRequestHandler<GetActiveFloorballPlayersQuery, Result<IEnumerable<FloorballPlayerDto>>>
{
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly ILogger<GetActiveFloorballPlayersHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetActiveFloorballPlayersHandler class
    /// </summary>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="logger">The logger</param>
    public GetActiveFloorballPlayersHandler(
        IFloorballPlayerRepository playerRepository,
        ILogger<GetActiveFloorballPlayersHandler> logger)
    {
        _playerRepository = playerRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetActiveFloorballPlayersQuery request
    /// </summary>
    /// <param name="request">The query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Active floorball players as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FloorballPlayerDto>>> Handle(GetActiveFloorballPlayersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving active floorball players");
            
            IEnumerable<FloorballPlayer> allPlayers = await _playerRepository.GetAllAsync();
            IEnumerable<FloorballPlayer> activePlayers = allPlayers.Where(p => p.IsActive);
            IEnumerable<FloorballPlayerDto> playerDtos = FloorballPlayerMapper.ToDtos(activePlayers);
            
            _logger.LogInformation("Successfully retrieved {PlayerCount} active floorball players", playerDtos.Count());
            
            return Result<IEnumerable<FloorballPlayerDto>>.Success(playerDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving active floorball players");
            return Result<IEnumerable<FloorballPlayerDto>>.Failure("An error occurred while retrieving active floorball players.");
        }
    }
} 