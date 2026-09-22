using Application.Common;
using Application.Features.Common.Organization.Deletion;
using Application.Features.Floorball.Players.Commands;
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Players.Handlers;

/// <summary>
/// Handler for deleting a floorball player that has no competition history.
/// </summary>
public class DeleteFloorballPlayerHandler : IRequestHandler<DeleteFloorballPlayerCommand, Result>
{
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballUnitOfWork _floorballUnitOfWork;
    private readonly ILogger<DeleteFloorballPlayerHandler> _logger;

    public DeleteFloorballPlayerHandler(
        IFloorballPlayerRepository playerRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballUnitOfWork floorballUnitOfWork,
        ILogger<DeleteFloorballPlayerHandler> logger)
    {
        _playerRepository = playerRepository;
        _teamRepository = teamRepository;
        _floorballUnitOfWork = floorballUnitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteFloorballPlayerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool playerExists = await _playerRepository.ExistsAsync(request.Id);
            if (!playerExists)
            {
                _logger.LogWarning("Attempt to delete non-existent floorball player with ID: {PlayerId}", request.Id);
                return Result.NotFound("FloorballPlayer", request.Id);
            }

            if (await _playerRepository.HasCompetitionHistoryAsync(request.Id, cancellationToken))
            {
                _logger.LogWarning("Blocked floorball player delete for {PlayerId}: has competition history", request.Id);
                return Result.Failure(DeletionReasons.PlayerHasHistory);
            }

            IEnumerable<FloorballTeam> teams =
                await _teamRepository.GetTeamsByPlayerIdAsync(request.Id)
                ?? Array.Empty<FloorballTeam>();
            foreach (FloorballTeam team in teams)
            {
                team.RemovePlayer(request.Id);
            }

            _logger.LogInformation("Deleting floorball player with ID: {PlayerId}", request.Id);
            await _playerRepository.DeleteAsync(request.Id);
            await _floorballUnitOfWork.SaveChangesAsync(cancellationToken);

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
