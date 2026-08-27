using Application.Common;
using Application.Features.Floorball.Seasons.Commands;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Seasons.Handlers;

public class ReplaceFloorballSeasonContentBlocksHandler
    : IRequestHandler<ReplaceFloorballSeasonContentBlocksCommand, Result<FloorballSeasonContentBlocksDto>>
{
    private readonly IFloorballCompetitionRepository _competitionRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<ReplaceFloorballSeasonContentBlocksHandler> _logger;

    public ReplaceFloorballSeasonContentBlocksHandler(
        IFloorballCompetitionRepository competitionRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<ReplaceFloorballSeasonContentBlocksHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballSeasonContentBlocksDto>> Handle(
        ReplaceFloorballSeasonContentBlocksCommand request,
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

            season.ReplaceContentBlocks(
                request.Items.Select(item => (item.Id, item.Title, item.ContentHtml)).ToList());

            await _competitionRepository.UpdateAsync(season);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FloorballSeasonContentBlocksDto>.Success(FloorballSeasonContentBlockMapper.ToDtos(season));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid floorball season content blocks for {SeasonId}", request.SeasonId);
            return Result<FloorballSeasonContentBlocksDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to replace floorball season content blocks {SeasonId}", request.SeasonId);
            return Result<FloorballSeasonContentBlocksDto>.Failure(
                "An error occurred while updating the season content blocks.");
        }
    }
}
