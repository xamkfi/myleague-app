using Application.Common;
using Application.Features.Floorball.Seasons.Commands;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Seasons.Handlers;

/// <summary>
/// Handler for deleting a floorball season.
/// A season may be removed together with its teams and fixtures when every match is still unplayed.
/// </summary>
public class DeleteFloorballSeasonHandler : IRequestHandler<DeleteFloorballSeasonCommand, Result>
{
    private const string PlayedMatchBlocksDelete =
        "Cannot delete a season that has a match that has started, finished, or been cancelled.";

    private readonly IFloorballCompetitionRepository _seasonRepository;
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFloorballSeasonHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the DeleteFloorballSeasonHandler class
    /// </summary>
    public DeleteFloorballSeasonHandler(
        IFloorballCompetitionRepository seasonRepository,
        IFloorballMatchRepository matchRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<DeleteFloorballSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DeleteFloorballSeasonCommand request
    /// </summary>
    public async Task<Result> Handle(DeleteFloorballSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool seasonExists = await _seasonRepository.ExistsAsync(request.Id);
            if (!seasonExists)
            {
                _logger.LogWarning("Attempt to delete non-existent floorball season with ID: {SeasonId}", request.Id);
                return Result.NotFound("FloorballSeason", request.Id);
            }

            bool playedMatchBlocksDelete = await _matchRepository.HasMatchThatBlocksSeasonDeleteAsync(
                request.Id,
                cancellationToken);
            if (playedMatchBlocksDelete)
            {
                _logger.LogWarning(
                    "Attempt to delete floorball season {SeasonId} that has a started or recorded match",
                    request.Id);
                return Result.Failure(PlayedMatchBlocksDelete);
            }

            int deletedMatches = await _matchRepository.DeleteAllByCompetitionIdAsync(request.Id, cancellationToken);
            if (deletedMatches > 0)
            {
                _logger.LogInformation(
                    "Deleted {DeletedMatches} unplayed floorball match(es) for season {SeasonId}",
                    deletedMatches,
                    request.Id);
            }

            _logger.LogInformation("Deleting floorball season with ID: {SeasonId}", request.Id);
            await _seasonRepository.DeleteAsync(request.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted floorball season with ID: {SeasonId}", request.Id);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule rejected deleting floorball season {SeasonId}", request.Id);
            return Result.Failure(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database rejected deleting floorball season {SeasonId}", request.Id);
            return Result.Failure("An error occurred while deleting the floorball season.");
        }
    }
}
