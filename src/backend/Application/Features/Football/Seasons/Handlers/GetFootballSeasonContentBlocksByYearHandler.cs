using Application.Common;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Seasons.Mappings;
using Application.Features.Football.Seasons.Queries;
using Domain.Entities.Football.Competitions;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Seasons.Handlers;

public class GetFootballSeasonContentBlocksByYearHandler
    : IRequestHandler<GetFootballSeasonContentBlocksByYearQuery, Result<FootballSeasonContentBlocksDto>>
{
    private readonly IFootballCompetitionRepository _competitionRepository;
    private readonly ILogger<GetFootballSeasonContentBlocksByYearHandler> _logger;

    public GetFootballSeasonContentBlocksByYearHandler(
        IFootballCompetitionRepository competitionRepository,
        ILogger<GetFootballSeasonContentBlocksByYearHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _logger = logger;
    }

    public async Task<Result<FootballSeasonContentBlocksDto>> Handle(
        GetFootballSeasonContentBlocksByYearQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            int? startYear = null;
            int? endYear = null;
            if (FootballSeasonYear.TryParse(request.SeasonYear, out int parsedStart, out int parsedEnd))
            {
                startYear = parsedStart;
                endYear = parsedEnd;
            }

            FootballSeason? season = await _competitionRepository.GetFeaturedSeasonWithContentBlocksAsync(
                startYear,
                endYear,
                cancellationToken);

            return Result<FootballSeasonContentBlocksDto>.Success(FootballSeasonContentBlockMapper.ToDtos(season));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to get featured football season content blocks");
            return Result<FootballSeasonContentBlocksDto>.Failure(
                "An error occurred while retrieving the season content blocks.");
        }
    }
}
