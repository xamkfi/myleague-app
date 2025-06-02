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
/// Handler for retrieving all floorball players
/// </summary>
public class GetAllFloorballPlayersHandler : IRequestHandler<GetAllFloorballPlayersQuery, Result<IEnumerable<FloorballPlayerDto>>>
{
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly ILogger<GetAllFloorballPlayersHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetAllFloorballPlayersHandler class
    /// </summary>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="logger">The logger</param>
    public GetAllFloorballPlayersHandler(
        IFloorballPlayerRepository playerRepository, 
        ILogger<GetAllFloorballPlayersHandler> logger)
    {
        _playerRepository = playerRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetAllFloorballPlayersQuery request
    /// </summary>
    /// <param name="request">The query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All floorball players as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FloorballPlayerDto>>> Handle(GetAllFloorballPlayersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving all floorball players");
            
            IEnumerable<FloorballPlayer> players = await _playerRepository.GetAllAsync();
            IEnumerable<FloorballPlayerDto> playerDtos = FloorballPlayerMapper.ToDtos(players);
            
            _logger.LogInformation("Successfully retrieved {PlayerCount} floorball players", playerDtos.Count());
            
            return Result<IEnumerable<FloorballPlayerDto>>.Success(playerDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all floorball players");
            return Result<IEnumerable<FloorballPlayerDto>>.Failure("An error occurred while retrieving floorball players.");
        }
    }
} 