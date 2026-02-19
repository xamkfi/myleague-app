using Application.Features.Floorball.Seasons.Commands;
using Application.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Seasons.Handlers;

/// <summary>
/// Handler to add a division to a season
/// </summary>
public class AddDivisionToSeasonHandler : IRequestHandler<AddDivisionToSeasonCommand, Result>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly IFloorballSeasonDivisionRepository _seasonDivisionRepository;
    private readonly ILogger<AddDivisionToSeasonHandler> _logger;

    public AddDivisionToSeasonHandler(
        IFloorballSeasonRepository seasonRepository,
        IFloorballSeasonDivisionRepository seasonDivisionRepository,
        ILogger<AddDivisionToSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(AddDivisionToSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Ensure season exists
            if (!await _seasonRepository.ExistsAsync(request.SeasonId))
            {
                return Result.NotFound("FloorballSeason", request.SeasonId);
            }

            _logger.LogInformation("Adding division {DivisionId} to season {SeasonId}", request.DivisionId, request.SeasonId);
            await _seasonDivisionRepository.AddSeasonDivisionAsync(request.SeasonId, request.DivisionId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding division {DivisionId} to season {SeasonId}", request.DivisionId, request.SeasonId);
            return Result.Failure("Failed to add division to season.");
        }
    }
}


