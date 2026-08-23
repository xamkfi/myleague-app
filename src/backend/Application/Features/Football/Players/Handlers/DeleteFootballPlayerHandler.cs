using Application.Common;
using Application.Features.Common.Deletion;
using Application.Features.Football.Players.Commands;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Players.Handlers;

/// <summary>
/// Handler for deleting a football player that has no competition history.
/// </summary>
public class DeleteFootballPlayerHandler : IRequestHandler<DeleteFootballPlayerCommand, Result>
{
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballUnitOfWork _footballUnitOfWork;
    private readonly ILogger<DeleteFootballPlayerHandler> _logger;

    public DeleteFootballPlayerHandler(
        IFootballPlayerRepository playerRepository,
        IFootballTeamRepository teamRepository,
        IFootballUnitOfWork footballUnitOfWork,
        ILogger<DeleteFootballPlayerHandler> logger)
    {
        _playerRepository = playerRepository;
        _teamRepository = teamRepository;
        _footballUnitOfWork = footballUnitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteFootballPlayerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool playerExists = await _playerRepository.ExistsAsync(request.Id);
            if (!playerExists)
            {
                _logger.LogWarning("Attempt to delete non-existent football player with ID: {PlayerId}", request.Id);
                return Result.NotFound("FootballPlayer", request.Id);
            }

            if (await _playerRepository.HasCompetitionHistoryAsync(request.Id, cancellationToken))
            {
                _logger.LogWarning("Blocked football player delete for {PlayerId}: has competition history", request.Id);
                return Result.Failure(DeletionReasons.PlayerHasHistory);
            }

            IEnumerable<FootballTeam> teams =
                await _teamRepository.GetTeamsByPlayerIdAsync(request.Id)
                ?? Array.Empty<FootballTeam>();
            foreach (FootballTeam team in teams)
            {
                team.RemovePlayer(request.Id);
            }

            _logger.LogInformation("Deleting football player with ID: {PlayerId}", request.Id);
            await _playerRepository.DeleteAsync(request.Id);
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
