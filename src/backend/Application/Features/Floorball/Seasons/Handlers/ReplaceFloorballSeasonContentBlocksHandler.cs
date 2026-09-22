using Application.Common;
using Application.Features.Floorball.Seasons.Commands;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

            List<Guid> existingBlockIds = season.ContentBlocks.Select(block => block.Id).ToList();
            season.ReplaceContentBlocks(
                request.Items.Select(item => (item.Id, item.Title, item.ContentHtml)).ToList());
            _competitionRepository.MarkNewContentBlocksAdded(season, existingBlockIds);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FloorballSeasonContentBlocksDto>.Success(FloorballSeasonContentBlockMapper.ToDtos(season));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid floorball season content blocks for {SeasonId}", request.SeasonId);
            return Result<FloorballSeasonContentBlocksDto>.Failure(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to replace floorball season content blocks for {SeasonId}", request.SeasonId);
            return Result<FloorballSeasonContentBlocksDto>.Failure(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error replacing floorball season content blocks for {SeasonId}", request.SeasonId);
            return Result<FloorballSeasonContentBlocksDto>.Failure(
                "Season content blocks could not be saved. Please try again.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
    }
}
