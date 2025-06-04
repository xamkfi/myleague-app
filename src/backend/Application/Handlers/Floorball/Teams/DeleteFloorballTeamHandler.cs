using Application.Commands.Floorball.Team;
using Application.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories.Common;

namespace Application.Handlers.Floorball.Teams;

/// <summary>
/// Handler for deleting a floorball team
/// </summary>
public class DeleteFloorballTeamHandler : IRequestHandler<DeleteFloorballTeamCommand, Result>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFloorballTeamHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the DeleteFloorballTeamHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public DeleteFloorballTeamHandler(
        IFloorballTeamRepository teamRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteFloorballTeamHandler> logger)
    {
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DeleteFloorballTeamCommand request
    /// </summary>
    /// <param name="request">The command containing the team ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A Result indicating success or failure</returns>
    public async Task<Result> Handle(DeleteFloorballTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the team exists
            bool teamExists = await _teamRepository.ExistsAsync(request.Id);
            if (!teamExists)
            {
                _logger.LogWarning("Attempt to delete non-existent floorball team with ID: {TeamId}", request.Id);
                return Result.NotFound("FloorballTeam", request.Id);
            }

            _logger.LogInformation("Deleting floorball team with ID: {TeamId}", request.Id);
            await _teamRepository.DeleteAsync(request.Id);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted floorball team with ID: {TeamId}", request.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting floorball team: {TeamId}", request.Id);
            return Result.Failure("An error occurred while deleting the floorball team.");
        }
    }
} 
