using Application.Common;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Seasons.Mappings;
using Application.Features.Football.Seasons.Queries;
using Domain.Entities.Football.Competitions;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace Application.Features.Football.Seasons.Handlers;

public class GetFootballSeasonContentBlocksHandler
    : IRequestHandler<GetFootballSeasonContentBlocksQuery, Result<FootballSeasonContentBlocksDto>>
{
    private readonly IFootballCompetitionRepository _competitionRepository;
    private readonly ILogger<GetFootballSeasonContentBlocksHandler> _logger;

    public GetFootballSeasonContentBlocksHandler(
        IFootballCompetitionRepository competitionRepository,
        ILogger<GetFootballSeasonContentBlocksHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _logger = logger;
    }

    public async Task<Result<FootballSeasonContentBlocksDto>> Handle(
        GetFootballSeasonContentBlocksQuery request,
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

            return Result<FootballSeasonContentBlocksDto>.Success(FootballSeasonContentBlockMapper.ToDtos(season));
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "Failed to get football season content blocks {SeasonId}", request.SeasonId);
            return Result<FootballSeasonContentBlocksDto>.Failure(
                "An error occurred while retrieving the season content blocks.");
        }
    }
}
