using Application.Common;
using Application.Features.Football.Seasons.Commands;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Seasons.Handlers;

public class RemoveDivisionFromSeasonHandler : IRequestHandler<RemoveDivisionFromSeasonCommand, Result>
{
    private readonly IFootballCompetitionRepository _seasonRepository;
    private readonly IFootballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly ILogger<RemoveDivisionFromSeasonHandler> _logger;

    public RemoveDivisionFromSeasonHandler(
        IFootballCompetitionRepository seasonRepository,
        IFootballCompetitionDivisionRepository seasonDivisionRepository,
        ILogger<RemoveDivisionFromSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveDivisionFromSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!await _seasonRepository.ExistsAsync(request.CompetitionId))
            {
                return Result.NotFound("FootballSeason", request.CompetitionId);
            }

            _logger.LogInformation("Removing division {DivisionId} from season {SeasonId}", request.DivisionId, request.CompetitionId);
            await _seasonDivisionRepository.RemoveCompetitionDivisionAsync(request.CompetitionId, request.DivisionId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing division {DivisionId} from season {SeasonId}", request.DivisionId, request.CompetitionId);
            return Result.Failure("Failed to remove division from season.");
        }
    }
}
