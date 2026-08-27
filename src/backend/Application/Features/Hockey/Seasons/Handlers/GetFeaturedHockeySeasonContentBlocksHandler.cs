using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Seasons.Mappings;
using Application.Features.Hockey.Seasons.Queries;
using Domain.Entities.Hockey.Competitions;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Seasons.Handlers;

public class GetFeaturedHockeySeasonContentBlocksHandler
    : IRequestHandler<GetFeaturedHockeySeasonContentBlocksQuery, Result<HockeySeasonContentBlocksDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly ILogger<GetFeaturedHockeySeasonContentBlocksHandler> _logger;

    public GetFeaturedHockeySeasonContentBlocksHandler(
        IHockeyCompetitionRepository competitionRepository,
        ILogger<GetFeaturedHockeySeasonContentBlocksHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _logger = logger;
    }

    public async Task<Result<HockeySeasonContentBlocksDto>> Handle(
        GetFeaturedHockeySeasonContentBlocksQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeySeason? season = await _competitionRepository.GetFeaturedSeasonWithContentBlocksAsync(cancellationToken);
            return Result<HockeySeasonContentBlocksDto>.Success(HockeySeasonContentBlockMapper.ToDtos(season));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get featured hockey season content blocks");
            return Result<HockeySeasonContentBlocksDto>.Failure(
                "An error occurred while retrieving the season content blocks.");
        }
    }
}
