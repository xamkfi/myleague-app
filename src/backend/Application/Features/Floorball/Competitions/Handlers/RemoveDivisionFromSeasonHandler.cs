using Application.Features.Floorball.Competitions.Commands;
using Application.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Competitions.Handlers;

/// <summary>
/// Handler to remove a division from a season
/// </summary>
public class RemoveDivisionFromSeasonHandler : IRequestHandler<RemoveFloorballDivisionFromSeasonCommand, Result>
{
    private readonly IFloorballCompetitionRepository _seasonRepository;
    private readonly IFloorballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly ILogger<RemoveDivisionFromSeasonHandler> _logger;

    public RemoveDivisionFromSeasonHandler(
        IFloorballCompetitionRepository seasonRepository,
        IFloorballCompetitionDivisionRepository seasonDivisionRepository,
        ILogger<RemoveDivisionFromSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveFloorballDivisionFromSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Ensure season exists
            if (!await _seasonRepository.ExistsAsync(request.CompetitionId))
            {
                return Result.NotFound("FloorballSeason", request.CompetitionId);
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


