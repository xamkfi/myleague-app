using Application.Commands.Floorball.Player;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Mappings.Common;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Floorball.Players;

/// <summary>
/// Handler for updating an existing floorball player
/// </summary>
public class UpdateFloorballPlayerHandler : IRequestHandler<UpdateFloorballPlayerCommand, Result<FloorballPlayerDto>>
{
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFloorballPlayerHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateFloorballPlayerHandler class
    /// </summary>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateFloorballPlayerHandler(
        IFloorballPlayerRepository playerRepository,
        IPersonRepository personRepository,
        IFloorballUnitOfWork unitOfWork, 
        ILogger<UpdateFloorballPlayerHandler> logger)
    {
        _playerRepository = playerRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateFloorballPlayerCommand request
    /// </summary>
    /// <param name="request">The command containing updated player information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated player as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballPlayerDto>> Handle(UpdateFloorballPlayerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find the existing player
            FloorballPlayer? existingPlayer = await _playerRepository.GetByIdAsync(request.Id);
            if (existingPlayer == null)
            {
                _logger.LogWarning("Attempt to update non-existent floorball player with ID: {PlayerId}", request.Id);
                return Result<FloorballPlayerDto>.NotFound("FloorballPlayer", request.Id);
            }

            // Update the player
            FloorballPlayerMapper.UpdateFromCommand(existingPlayer, request);
            
            _logger.LogInformation("Updating floorball player: {PlayerId}", existingPlayer.Id);
            
            // Save changes explicitly to trigger domain events
            // Note: No need to call UpdateAsync since the entity is already tracked by EF Core
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Load Person data to create proper DTO response
            Person? person = await _personRepository.GetByIdAsync(existingPlayer.PersonId);
            FloorballPlayerDto playerDto;
            
            if (person != null)
            {
                // Create DTO with real person data
                playerDto = new FloorballPlayerDto(
                    existingPlayer.Id,
                    existingPlayer.PersonId,
                    PersonMapper.ToDto(person),
                    existingPlayer.IsActive,
                    existingPlayer.Position.PrimaryPosition,
                    existingPlayer.CareerGoals,
                    existingPlayer.CareerAssists,
                    null // Team information not retrieved in update handler
                );
            }
            else
            {
                // Fallback to placeholder if person not found
                playerDto = FloorballPlayerMapper.ToDto(existingPlayer);
            }
            
            _logger.LogInformation("Successfully updated floorball player with ID: {PlayerId}", existingPlayer.Id);

            return Result<FloorballPlayerDto>.Success(playerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating floorball player: {PlayerId}", request.Id);
            return Result<FloorballPlayerDto>.Failure("An error occurred while updating the floorball player.");
        }
    }
} 