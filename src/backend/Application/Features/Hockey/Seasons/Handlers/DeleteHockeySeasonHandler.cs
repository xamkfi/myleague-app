using Application.Common;
using Application.Features.Hockey.Seasons.Commands;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Seasons.Handlers;

/// <summary>
/// Deletes a hockey season, its unplayed matches, and the rows that would block that delete.
/// </summary>
public class DeleteHockeySeasonHandler : IRequestHandler<DeleteHockeySeasonCommand, Result>
{
    private const string PlayedMatchBlocksDelete =
        "Cannot delete a season that has a match that has started, finished, or been cancelled.";

    private readonly IHockeyCompetitionRepository _seasonRepository;
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyStatisticsRepository _statisticsRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteHockeySeasonHandler> _logger;

    public DeleteHockeySeasonHandler(
        IHockeyCompetitionRepository seasonRepository,
        IHockeyMatchRepository matchRepository,
        IHockeyStatisticsRepository statisticsRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<DeleteHockeySeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _matchRepository = matchRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteHockeySeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool seasonExists = await _seasonRepository.ExistsAsync(request.Id, cancellationToken);
            if (!seasonExists)
            {
                _logger.LogWarning("Attempt to delete non-existent hockey season with ID: {SeasonId}", request.Id);
                return Result.NotFound("HockeySeason", request.Id);
            }

            bool playedMatchBlocksDelete = await _matchRepository.HasMatchThatBlocksSeasonDeleteAsync(
                request.Id,
                cancellationToken);
            if (playedMatchBlocksDelete)
            {
                _logger.LogWarning(
                    "Attempt to delete hockey season {SeasonId} that has a started or recorded match",
                    request.Id);
                return Result.Failure(PlayedMatchBlocksDelete);
            }

            int deletedMatches = await _matchRepository.DeleteAllByCompetitionIdAsync(request.Id, cancellationToken);
            if (deletedMatches > 0)
            {
                _logger.LogInformation(
                    "Deleted {DeletedMatches} unplayed hockey match(es) for season {SeasonId}",
                    deletedMatches,
                    request.Id);
            }

            // Competition statistics and the statistics cache restrict the competition.
            await _statisticsRepository.ResetCompetitionStatisticsAsync(request.Id);

            _logger.LogInformation("Deleting hockey season with ID: {SeasonId}", request.Id);
            await _seasonRepository.DeleteAsync(request.Id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted hockey season with ID: {SeasonId}", request.Id);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule rejected deleting hockey season {SeasonId}", request.Id);
            return Result.Failure(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database rejected deleting hockey season {SeasonId}", request.Id);
            return Result.Failure("An error occurred while deleting the hockey season.");
        }
    }
}
