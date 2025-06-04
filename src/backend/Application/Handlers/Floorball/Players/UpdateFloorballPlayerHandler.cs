using Application.Commands.Floorball.Player;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories.Common;

namespace Application.Handlers.Floorball.Players;

/// <summary>
/// Handler for updating an existing floorball player
/// </summary>
public class UpdateFloorballPlayerHandler : IRequestHandler<UpdateFloorballPlayerCommand, Result<FloorballPlayerDto>>
{
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFloorballPlayerHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateFloorballPlayerHandler class
    /// </summary>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateFloorballPlayerHandler(
        IFloorballPlayerRepository playerRepository, 
        IUnitOfWork unitOfWork, 
        ILogger<UpdateFloorballPlayerHandler> logger)
    {
        _playerRepository = playerRepository;
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
            await _playerRepository.UpdateAsync(existingPlayer);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballPlayerDto playerDto = FloorballPlayerMapper.ToDto(existingPlayer);
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
