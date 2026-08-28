using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Seasons.Queries;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Seasons.Handlers;

public class GetFloorballSeasonContentBlocksHandler
    : IRequestHandler<GetFloorballSeasonContentBlocksQuery, Result<FloorballSeasonContentBlocksDto>>
{
    private readonly IFloorballCompetitionRepository _competitionRepository;
    private readonly ILogger<GetFloorballSeasonContentBlocksHandler> _logger;

    public GetFloorballSeasonContentBlocksHandler(
        IFloorballCompetitionRepository competitionRepository,
        ILogger<GetFloorballSeasonContentBlocksHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _logger = logger;
    }

    public async Task<Result<FloorballSeasonContentBlocksDto>> Handle(
        GetFloorballSeasonContentBlocksQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            FloorballSeason? season = await _competitionRepository.GetSeasonWithContentBlocksAsync(
                request.SeasonId,
                cancellationToken);

            if (season is null)
            {
                return Result<FloorballSeasonContentBlocksDto>.NotFound("FloorballSeason", request.SeasonId);
            }

            return Result<FloorballSeasonContentBlocksDto>.Success(FloorballSeasonContentBlockMapper.ToDtos(season));
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Request was cancelled while getting floorball season content blocks {SeasonId}", request.SeasonId);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to get floorball season content blocks {SeasonId}", request.SeasonId);
            return Result<FloorballSeasonContentBlocksDto>.Failure(
                "An error occurred while retrieving the season content blocks.");
        }
    }
}
