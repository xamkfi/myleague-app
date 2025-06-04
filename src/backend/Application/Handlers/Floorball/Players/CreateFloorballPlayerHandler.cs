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
/// Handler for creating a new floorball player
/// </summary>
public class CreateFloorballPlayerHandler : IRequestHandler<CreateFloorballPlayerCommand, Result<FloorballPlayerDto>>
{
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFloorballPlayerHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateFloorballPlayerHandler class
    /// </summary>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public CreateFloorballPlayerHandler(
        IFloorballPlayerRepository playerRepository, 
        IUnitOfWork unitOfWork, 
        ILogger<CreateFloorballPlayerHandler> logger)
    {
        _playerRepository = playerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateFloorballPlayerCommand request
    /// </summary>
    /// <param name="request">The command containing player information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created player as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballPlayerDto>> Handle(CreateFloorballPlayerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create the player entity
            FloorballPlayer player = FloorballPlayerMapper.ToEntity(request);

            _logger.LogInformation("Creating new floorball player for person: {PersonId}", request.PersonId);
            await _playerRepository.AddAsync(player);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballPlayerDto playerDto = FloorballPlayerMapper.ToDto(player);
            _logger.LogInformation("Successfully created floorball player with ID: {PlayerId}", player.Id);

            return Result<FloorballPlayerDto>.Success(playerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating floorball player for person: {PersonId}", request.PersonId);
            return Result<FloorballPlayerDto>.Failure("An error occurred while creating the floorball player.");
        }
    }
} 
