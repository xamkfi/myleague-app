using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Seasons.Queries;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Seasons.Handlers;

public class GetFloorballSeasonContentBlocksByYearHandler
    : IRequestHandler<GetFloorballSeasonContentBlocksByYearQuery, Result<FloorballSeasonContentBlocksDto>>
{
    private readonly IFloorballCompetitionRepository _competitionRepository;
    private readonly ILogger<GetFloorballSeasonContentBlocksByYearHandler> _logger;

    public GetFloorballSeasonContentBlocksByYearHandler(
        IFloorballCompetitionRepository competitionRepository,
        ILogger<GetFloorballSeasonContentBlocksByYearHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _logger = logger;
    }

    public async Task<Result<FloorballSeasonContentBlocksDto>> Handle(
        GetFloorballSeasonContentBlocksByYearQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            int? startYear = null;
            int? endYear = null;
            if (FloorballSeasonYear.TryParse(request.SeasonYear, out int parsedStart, out int parsedEnd))
            {
                startYear = parsedStart;
                endYear = parsedEnd;
            }

            FloorballSeason? season = await _competitionRepository.GetFeaturedSeasonWithContentBlocksAsync(
                startYear,
                endYear,
                cancellationToken);

            return Result<FloorballSeasonContentBlocksDto>.Success(FloorballSeasonContentBlockMapper.ToDtos(season));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to get featured floorball season content blocks");
            return Result<FloorballSeasonContentBlocksDto>.Failure(
                "An error occurred while retrieving the season content blocks.");
        }
    }
}
