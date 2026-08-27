using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Seasons.Mappings;
using Application.Features.Hockey.Seasons.Queries;
using Domain.Entities.Hockey.Competitions;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Seasons.Handlers;

public class GetHockeySeasonContentBlocksHandler
    : IRequestHandler<GetHockeySeasonContentBlocksQuery, Result<HockeySeasonContentBlocksDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly ILogger<GetHockeySeasonContentBlocksHandler> _logger;

    public GetHockeySeasonContentBlocksHandler(
        IHockeyCompetitionRepository competitionRepository,
        ILogger<GetHockeySeasonContentBlocksHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _logger = logger;
    }

    public async Task<Result<HockeySeasonContentBlocksDto>> Handle(
        GetHockeySeasonContentBlocksQuery request,
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

            return Result<HockeySeasonContentBlocksDto>.Success(HockeySeasonContentBlockMapper.ToDtos(season));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hockey season content blocks {SeasonId}", request.SeasonId);
            return Result<HockeySeasonContentBlocksDto>.Failure(
                "An error occurred while retrieving the season content blocks.");
        }
    }
}
