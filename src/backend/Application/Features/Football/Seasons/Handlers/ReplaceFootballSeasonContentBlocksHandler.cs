using Application.Common;
using Application.Features.Football.Seasons.Commands;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Seasons.Mappings;
using Domain.Entities.Football.Competitions;
using Domain.Repositories.Football;
using MediatR;
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

            season.ReplaceContentBlocks(
                request.Items.Select(item => (item.Id, item.Title, item.ContentHtml)).ToList());

            await _competitionRepository.UpdateAsync(season);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FootballSeasonContentBlocksDto>.Success(FootballSeasonContentBlockMapper.ToDtos(season));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid football season content blocks for {SeasonId}", request.SeasonId);
            return Result<FootballSeasonContentBlocksDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to replace football season content blocks {SeasonId}", request.SeasonId);
            return Result<FootballSeasonContentBlocksDto>.Failure(
                "An error occurred while updating the season content blocks.");
        }
    }
}
