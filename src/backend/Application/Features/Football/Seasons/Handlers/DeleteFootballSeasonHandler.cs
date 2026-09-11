using Application.Common;
using Application.Features.Football.Seasons.Commands;
using Domain.Entities.Football.Matches;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Seasons.Handlers;

public class DeleteFootballSeasonHandler : IRequestHandler<DeleteFootballSeasonCommand, Result>
{
    private readonly IFootballCompetitionRepository _seasonRepository;
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFootballSeasonHandler> _logger;

    public DeleteFootballSeasonHandler(
        IFootballCompetitionRepository seasonRepository,
        IFootballMatchRepository matchRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<DeleteFootballSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _matchRepository = matchRepository;
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

            IEnumerable<FootballMatch> seasonMatches = await _matchRepository.GetByCompetitionIdAsync(request.Id);
            if (seasonMatches.Any())
            {
                _logger.LogWarning("Attempt to delete season with existing matches: {SeasonId}", request.Id);
                return Result.Failure("Cannot delete a season that has matches. Delete the matches first.");
            }

            _logger.LogInformation("Deleting football season with ID: {SeasonId}", request.Id);
            await _seasonRepository.DeleteAsync(request.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted football season with ID: {SeasonId}", request.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting football season: {SeasonId}", request.Id);
            return Result.Failure("An error occurred while deleting the football season.");
        }
    }
}
