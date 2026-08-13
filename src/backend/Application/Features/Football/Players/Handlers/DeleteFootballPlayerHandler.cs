using Application.Features.Football.Players.Commands;
using Application.Common;
using Domain.Repositories.Football;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories.Common;
using Domain.Entities.Football.Teams;

namespace Application.Features.Football.Players.Handlers;

/// <summary>
/// Handler for deleting a football player
/// </summary>
public class DeleteFootballPlayerHandler : IRequestHandler<DeleteFootballPlayerCommand, Result>
{
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballUnitOfWork _footballUnitOfWork;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFootballPlayerHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the DeleteFootballPlayerHandler class
    /// </summary>
    /// <param name="playerRepository">The football player repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public DeleteFootballPlayerHandler(
        IFootballPlayerRepository playerRepository, 
        IFootballTeamRepository teamRepository,
        IFootballUnitOfWork footballUnitOfWork,
        IUnitOfWork unitOfWork, 
        ILogger<DeleteFootballPlayerHandler> logger)
    {
        _playerRepository = playerRepository;
        _teamRepository = teamRepository;
        _footballUnitOfWork = footballUnitOfWork;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DeleteFootballPlayerCommand request
    /// </summary>
    /// <param name="request">The command containing the player ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A Result indicating success or failure</returns>
    public async Task<Result> Handle(DeleteFootballPlayerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the player exists
            bool playerExists = await _playerRepository.ExistsAsync(request.Id);
            if (!playerExists)
            {
                _logger.LogWarning("Attempt to delete non-existent football player with ID: {PlayerId}", request.Id);
                return Result.NotFound("FootballPlayer", request.Id);
            }

            _logger.LogInformation("Deleting football player with ID: {PlayerId}", request.Id);
            await _playerRepository.DeleteAsync(request.Id);

            // Check if the player is in any team
            IEnumerable<FootballTeam> teams = await _teamRepository.GetTeamsByPlayerIdAsync(request.Id);
            if (teams == null)
            {
                _logger.LogWarning("Player {PlayerId} is in teams, cannot delete", request.Id);
                return Result.NotFound("FootballPlayer", request.Id);
            }
            else
            {
                foreach (FootballTeam team in teams)
                {
                    team.RemovePlayer(request.Id);
                }
            }

            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _footballUnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted football player with ID: {PlayerId}", request.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting football player: {PlayerId}", request.Id);
            return Result.Failure("An error occurred while deleting the football player.");
        }
    }
} 
