using Application.Queries.Floorball;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Domain.Entities.Common;
using Application.Mappings.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Queries.Floorball.Player;

namespace Application.Handlers.Floorball.Players;

/// <summary>
/// Handler for retrieving a floorball player by ID
/// </summary>
public class GetFloorballPlayerByIdHandler : IRequestHandler<GetFloorballPlayerByIdQuery, Result<FloorballPlayerDto>>
{
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetFloorballPlayerByIdHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFloorballPlayerByIdHandler class
    /// </summary>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="logger">The logger</param>
    public GetFloorballPlayerByIdHandler(
        IFloorballPlayerRepository playerRepository,
        IPersonRepository personRepository,
        ILogger<GetFloorballPlayerByIdHandler> logger)
    {
        _playerRepository = playerRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFloorballPlayerByIdQuery request
    /// </summary>
    /// <param name="request">The query containing the player ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The player as a DTO wrapped in a Result, or a not found result</returns>
    public async Task<Result<FloorballPlayerDto>> Handle(GetFloorballPlayerByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving floorball player with ID: {PlayerId}", request.Id);
            
            FloorballPlayer? player = await _playerRepository.GetByIdAsync(request.Id);
            if (player == null)
            {
                _logger.LogWarning("Floorball player with ID {PlayerId} not found", request.Id);
                return Result<FloorballPlayerDto>.NotFound("FloorballPlayer", request.Id);
            }

            // Get the associated person
            Person? person = await _personRepository.GetByIdAsync(player.PersonId);
            if (person == null)
            {
                _logger.LogWarning("Person with ID {PersonId} not found for player {PlayerId}", player.PersonId, player.Id);
                return Result<FloorballPlayerDto>.Failure("Associated person not found");
            }

            // Create DTO with real person data
            FloorballPlayerDto playerDto = new FloorballPlayerDto(
                player.Id,
                player.PersonId,
                PersonMapper.ToDto(person),
                player.IsActive,
                player.Position.PrimaryPosition,
                player.CareerGoals,
                player.CareerAssists
            );

            _logger.LogInformation("Successfully retrieved floorball player: {PlayerId}", player.Id);

            return Result<FloorballPlayerDto>.Success(playerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball player: {PlayerId}", request.Id);
            return Result<FloorballPlayerDto>.Failure("An error occurred while retrieving the floorball player.");
        }
    }
} 
