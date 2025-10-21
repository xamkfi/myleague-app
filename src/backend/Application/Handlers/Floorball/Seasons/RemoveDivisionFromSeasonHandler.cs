using Application.Commands.Floorball.Season;
using Application.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Floorball.Seasons;

/// <summary>
/// Handler to remove a division from a season
/// </summary>
public class RemoveDivisionFromSeasonHandler : IRequestHandler<RemoveDivisionFromSeasonCommand, Result>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly IFloorballSeasonDivisionRepository _seasonDivisionRepository;
    private readonly ILogger<RemoveDivisionFromSeasonHandler> _logger;

    public RemoveDivisionFromSeasonHandler(
        IFloorballSeasonRepository seasonRepository,
        IFloorballSeasonDivisionRepository seasonDivisionRepository,
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
            // Ensure season exists
            if (!await _seasonRepository.ExistsAsync(request.SeasonId))
            {
                return Result.NotFound("FloorballSeason", request.SeasonId);
            }

            _logger.LogInformation("Removing division {DivisionId} from season {SeasonId}", request.DivisionId, request.SeasonId);
            await _seasonDivisionRepository.RemoveSeasonDivisionAsync(request.SeasonId, request.DivisionId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing division {DivisionId} from season {SeasonId}", request.DivisionId, request.SeasonId);
            return Result.Failure("Failed to remove division from season.");
        }
    }
}


