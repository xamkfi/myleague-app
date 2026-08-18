using Application.Features.Football.Teams.Commands;
using Application.Common;
using Domain.Repositories.Football;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Football.Teams.Handlers;

/// <summary>
/// Handler for deleting a football team
/// </summary>
public class DeleteFootballTeamHandler : IRequestHandler<DeleteFootballTeamCommand, Result>
{
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballUnitOfWork _footballUnitOfWork;
    private readonly ILogger<DeleteFootballTeamHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the DeleteFootballTeamHandler class
    /// </summary>
    /// <param name="teamRepository">The football team repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public DeleteFootballTeamHandler(
        IFootballTeamRepository teamRepository,
        IFootballUnitOfWork footballUnitOfWork,
        ILogger<DeleteFootballTeamHandler> logger)
    {
        _teamRepository = teamRepository;
        _footballUnitOfWork = footballUnitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DeleteFootballTeamCommand request
    /// </summary>
    /// <param name="request">The command containing the team ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A Result indicating success or failure</returns>
    public async Task<Result> Handle(DeleteFootballTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the team exists
            bool teamExists = await _teamRepository.ExistsAsync(request.Id);
            if (!teamExists)
            {
                _logger.LogWarning("Attempt to delete non-existent football team with ID: {TeamId}", request.Id);
                return Result.NotFound("FootballTeam", request.Id);
            }

            _logger.LogInformation("Deleting football team with ID: {TeamId}", request.Id);
            await _teamRepository.DeleteAsync(request.Id);
            
            // Save changes explicitly to trigger domain events
            await _footballUnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted football team with ID: {TeamId}", request.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting football team: {TeamId}", request.Id);
            return Result.Failure("An error occurred while deleting the football team.");
        }
    }
} 
