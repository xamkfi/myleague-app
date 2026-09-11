using Application.Common;
using Application.Features.Hockey.Seasons.Commands;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Seasons.Mappings;
using Domain.Entities.Hockey.Competitions;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Seasons.Handlers;

public class ReplaceHockeySeasonContentBlocksHandler
    : IRequestHandler<ReplaceHockeySeasonContentBlocksCommand, Result<HockeySeasonContentBlocksDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<ReplaceHockeySeasonContentBlocksHandler> _logger;

    public ReplaceHockeySeasonContentBlocksHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<ReplaceHockeySeasonContentBlocksHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeySeasonContentBlocksDto>> Handle(
        ReplaceHockeySeasonContentBlocksCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeySeason? season = await _competitionRepository.GetSeasonWithContentBlocksAsync(
                request.SeasonId,
                cancellationToken);

            if (season is null)
            {
                return Result<HockeySeasonContentBlocksDto>.NotFound("HockeySeason", request.SeasonId);
            }

            List<Guid> existingBlockIds = season.ContentBlocks.Select(block => block.Id).ToList();
            season.ReplaceContentBlocks(
                request.Items.Select(item => (item.Id, item.Title, item.ContentHtml)).ToList());
            _competitionRepository.MarkNewContentBlocksAdded(season, existingBlockIds);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<HockeySeasonContentBlocksDto>.Success(HockeySeasonContentBlockMapper.ToDtos(season));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid hockey season content blocks for {SeasonId}", request.SeasonId);
            return Result<HockeySeasonContentBlocksDto>.Failure(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to replace hockey season content blocks for {SeasonId}", request.SeasonId);
            return Result<HockeySeasonContentBlocksDto>.Failure(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error replacing hockey season content blocks for {SeasonId}", request.SeasonId);
            return Result<HockeySeasonContentBlocksDto>.Failure(
                "Season content blocks could not be saved. Please try again.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
    }
}
