using Application.Common;
using Application.Features.Common.Deletion;
using Application.Features.Hockey.Players.Commands;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Players.Handlers;

/// <summary>
/// Handles deleting a hockey player after removing roster memberships.
/// </summary>
public class DeleteHockeyPlayerHandler : IRequestHandler<DeleteHockeyPlayerCommand, Result>
{
    private readonly IHockeyPlayerRepository _playerRepository;
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteHockeyPlayerHandler> _logger;

    public DeleteHockeyPlayerHandler(
        IHockeyPlayerRepository playerRepository,
        IHockeyTeamRepository teamRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<DeleteHockeyPlayerHandler> logger)
    {
        _playerRepository = playerRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteHockeyPlayerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool playerExists = await _playerRepository.ExistsAsync(request.Id);
            if (!playerExists)
            {
                _logger.LogWarning("Attempt to delete non-existent hockey player with ID: {PlayerId}", request.Id);
                return Result.NotFound("HockeyPlayer", request.Id);
            }

            if (await _playerRepository.HasCompetitionHistoryAsync(request.Id, cancellationToken))
            {
                _logger.LogWarning("Blocked hockey player delete for {PlayerId}: has competition history", request.Id);
                return Result.Failure(DeletionReasons.PlayerHasHistory);
            }

            IReadOnlyList<HockeyTeam> teams = await _teamRepository.GetByPlayerIdAsync(request.Id);
            foreach (HockeyTeam team in teams)
            {
                List<Guid?> competitionIds = team.Roster
                    .Where(membership => membership.PlayerId == request.Id && membership.IsActive)
                    .Select(membership => membership.CompetitionId)
                    .Distinct()
                    .ToList();

                foreach (Guid? competitionId in competitionIds)
                {
                    team.RemovePlayer(request.Id, competitionId);
                }
            }

            await _playerRepository.DeleteAsync(request.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted hockey player with ID: {PlayerId}", request.Id);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected DeleteHockeyPlayer for {PlayerId}", request.Id);
            return Result.Failure(ex.Message, ex.Flatten());
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error occurred while deleting hockey player: {PlayerId}", request.Id);
            return Result.Failure(
                "Cannot delete hockey player because related records exist.",
                ex.Flatten());
        }
    }
}
