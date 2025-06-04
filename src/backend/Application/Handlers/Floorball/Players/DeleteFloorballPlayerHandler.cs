using Application.Commands.Floorball.Player;
using Application.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories.Common;

namespace Application.Handlers.Floorball.Players;

/// <summary>
/// Handler for deleting a floorball player
/// </summary>
public class DeleteFloorballPlayerHandler : IRequestHandler<DeleteFloorballPlayerCommand, Result>
{
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFloorballPlayerHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the DeleteFloorballPlayerHandler class
    /// </summary>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public DeleteFloorballPlayerHandler(
        IFloorballPlayerRepository playerRepository, 
        IUnitOfWork unitOfWork, 
        ILogger<DeleteFloorballPlayerHandler> logger)
    {
        _playerRepository = playerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DeleteFloorballPlayerCommand request
    /// </summary>
    /// <param name="request">The command containing the player ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A Result indicating success or failure</returns>
    public async Task<Result> Handle(DeleteFloorballPlayerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the player exists
            bool playerExists = await _playerRepository.ExistsAsync(request.Id);
            if (!playerExists)
            {
                _logger.LogWarning("Attempt to delete non-existent floorball player with ID: {PlayerId}", request.Id);
                return Result.NotFound("FloorballPlayer", request.Id);
            }

            _logger.LogInformation("Deleting floorball player with ID: {PlayerId}", request.Id);
            await _playerRepository.DeleteAsync(request.Id);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted floorball player with ID: {PlayerId}", request.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting floorball player: {PlayerId}", request.Id);
            return Result.Failure("An error occurred while deleting the floorball player.");
        }
    }
} 
