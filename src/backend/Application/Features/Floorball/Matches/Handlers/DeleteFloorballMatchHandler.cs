using Application.Common;
using Application.Features.Floorball.Matches.Commands;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for permanently deleting a floorball match.
/// Only matches in the <see cref="FloorballMatchStatus.Scheduled"/> state may be deleted; this keeps the
/// endpoint safe to expose for revert flows (e.g. the tournament JSON import) without ever destroying
/// recorded match events, statistics, or completed match history.
/// </summary>
public class DeleteFloorballMatchHandler : IRequestHandler<DeleteFloorballMatchCommand, Result>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFloorballMatchHandler> _logger;

    public DeleteFloorballMatchHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<DeleteFloorballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteFloorballMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Attempt to delete non-existent floorball match with ID: {MatchId}", request.MatchId);
                return Result.NotFound("FloorballMatch", request.MatchId);
            }

            // Safety: refuse to delete matches that have started, finished, or been cancelled. Those carry
            // event history and statistics. The import-revert flow only ever needs to delete freshly created
            // Scheduled matches.
            if (match.Status != FloorballMatchStatus.Scheduled)
            {
                _logger.LogWarning(
                    "Refusing to delete floorball match {MatchId} because its status is {Status} (only Scheduled may be deleted).",
                    request.MatchId,
                    match.Status);
                return Result.Failure(
                    $"Only matches in the Scheduled state can be deleted (current status: {match.Status}).");
            }

            _logger.LogInformation("Deleting floorball match with ID: {MatchId}", request.MatchId);
            await _matchRepository.DeleteAsync(match.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting floorball match {MatchId}", request.MatchId);
            return Result.Failure("An error occurred while deleting the floorball match.", ex.Flatten());
        }
    }
}
