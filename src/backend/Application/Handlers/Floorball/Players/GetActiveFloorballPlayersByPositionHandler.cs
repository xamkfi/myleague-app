using Application.Queries.Floorball;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Domain.Enums.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Floorball.Players;

/// <summary>
/// Handler for retrieving active floorball players by position
/// </summary>
public class GetActiveFloorballPlayersByPositionHandler : IRequestHandler<GetActiveFloorballPlayersByPositionQuery, Result<IEnumerable<FloorballPlayerDto>>>
{
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly ILogger<GetActiveFloorballPlayersByPositionHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetActiveFloorballPlayersByPositionHandler class
    /// </summary>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="logger">The logger</param>
    public GetActiveFloorballPlayersByPositionHandler(
        IFloorballPlayerRepository playerRepository, 
        ILogger<GetActiveFloorballPlayersByPositionHandler> logger)
    {
        _playerRepository = playerRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetActiveFloorballPlayersByPositionQuery request
    /// </summary>
    /// <param name="request">The query containing the position to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Active players in the specified position as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FloorballPlayerDto>>> Handle(GetActiveFloorballPlayersByPositionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving active floorball players for position: {Position}", request.Position);
            
            IEnumerable<FloorballPlayer> players = await _playerRepository.GetActiveByPositionAsync(request.Position);
            IEnumerable<FloorballPlayerDto> playerDtos = FloorballPlayerMapper.ToDtos(players);
            
            _logger.LogInformation("Successfully retrieved {PlayerCount} active players for position {Position}", 
                playerDtos.Count(), request.Position);
            
            return Result<IEnumerable<FloorballPlayerDto>>.Success(playerDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving active floorball players for position: {Position}", request.Position);
            return Result<IEnumerable<FloorballPlayerDto>>.Failure("An error occurred while retrieving players by position.");
        }
    }
} 