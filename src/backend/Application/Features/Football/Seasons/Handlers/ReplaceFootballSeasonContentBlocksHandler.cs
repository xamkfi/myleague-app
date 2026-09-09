using Application.Common;
using Application.Features.Football.Seasons.Commands;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Seasons.Mappings;
using Domain.Entities.Football.Competitions;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Seasons.Handlers;

public class ReplaceFootballSeasonContentBlocksHandler
    : IRequestHandler<ReplaceFootballSeasonContentBlocksCommand, Result<FootballSeasonContentBlocksDto>>
{
    private readonly IFootballCompetitionRepository _competitionRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<ReplaceFootballSeasonContentBlocksHandler> _logger;

    public ReplaceFootballSeasonContentBlocksHandler(
        IFootballCompetitionRepository competitionRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<ReplaceFootballSeasonContentBlocksHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballSeasonContentBlocksDto>> Handle(
        ReplaceFootballSeasonContentBlocksCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            FootballSeason? season = await _competitionRepository.GetSeasonWithContentBlocksAsync(
                request.SeasonId,
                cancellationToken);

            if (season is null)
            {
                return Result<FootballSeasonContentBlocksDto>.NotFound("FootballSeason", request.SeasonId);
            }

            List<Guid> existingBlockIds = season.ContentBlocks.Select(block => block.Id).ToList();
            season.ReplaceContentBlocks(
                request.Items.Select(item => (item.Id, item.Title, item.ContentHtml)).ToList());
            _competitionRepository.MarkNewContentBlocksAdded(season, existingBlockIds);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FootballSeasonContentBlocksDto>.Success(FootballSeasonContentBlockMapper.ToDtos(season));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid football season content blocks for {SeasonId}", request.SeasonId);
            return Result<FootballSeasonContentBlocksDto>.Failure(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to replace football season content blocks for {SeasonId}", request.SeasonId);
            return Result<FootballSeasonContentBlocksDto>.Failure(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error replacing football season content blocks for {SeasonId}", request.SeasonId);
            return Result<FootballSeasonContentBlocksDto>.Failure(
                "Season content blocks could not be saved. Please try again.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
    }
}
