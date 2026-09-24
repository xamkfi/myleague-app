using Application.Common;
using Application.Features.Football.Seasons.Commands;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Seasons.Handlers;

/// <summary>
/// Handler for deleting a football season.
/// A season may be removed together with its teams and fixtures when every match is still unplayed.
/// </summary>
public class DeleteFootballSeasonHandler : IRequestHandler<DeleteFootballSeasonCommand, Result>
{
    private const string PlayedMatchBlocksDelete =
        "Cannot delete a season that has a match that has started, finished, or been cancelled.";

    private readonly IFootballCompetitionRepository _seasonRepository;
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFootballSeasonHandler> _logger;

    public DeleteFootballSeasonHandler(
        IFootballCompetitionRepository seasonRepository,
        IFootballMatchRepository matchRepository,
        IFootballStatisticsRepository statisticsRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<DeleteFootballSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _matchRepository = matchRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteFootballSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool seasonExists = await _seasonRepository.ExistsAsync(request.Id);
            if (!seasonExists)
            {
                _logger.LogWarning("Attempt to delete non-existent football season with ID: {SeasonId}", request.Id);
                return Result.NotFound("FootballSeason", request.Id);
            }

            bool playedMatchBlocksDelete = await _matchRepository.HasMatchThatBlocksSeasonDeleteAsync(
                request.Id,
                cancellationToken);
            if (playedMatchBlocksDelete)
            {
                _logger.LogWarning(
                    "Attempt to delete football season {SeasonId} that has a started or recorded match",
                    request.Id);
                return Result.Failure(PlayedMatchBlocksDelete);
            }

            int deletedMatches = await _matchRepository.DeleteAllByCompetitionIdAsync(request.Id, cancellationToken);
            if (deletedMatches > 0)
            {
                _logger.LogInformation(
                    "Deleted {DeletedMatches} unplayed football match(es) for season {SeasonId}",
                    deletedMatches,
                    request.Id);
            }

            // Team and player season statistics reference the competition with Restrict, including
            // the zero-game rows created when a team is added. Remove them before the season row.
            await _statisticsRepository.ResetCompetitionStatisticsAsync(request.Id, cancellationToken);

            _logger.LogInformation("Deleting football season with ID: {SeasonId}", request.Id);
            await _seasonRepository.DeleteAsync(request.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted football season with ID: {SeasonId}", request.Id);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule rejected deleting football season {SeasonId}", request.Id);
            return Result.Failure(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database rejected deleting football season {SeasonId}", request.Id);
            return Result.Failure("An error occurred while deleting the football season.");
        }
    }
}
