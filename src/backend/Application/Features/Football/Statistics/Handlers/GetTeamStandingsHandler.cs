using Application.Common;
using Application.Features.Football.Statistics.Mappings;
using Application.Features.Football.Statistics.Queries;
using Application.Features.Football.Teams.DTOs;
using Domain.Entities.Football.Statistics;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Statistics.Handlers;

/// <summary>
/// Handler for retrieving team standings
/// </summary>
public class GetTeamStandingsHandler : IRequestHandler<GetTeamStandingsQuery, Result<List<FootballTeamSeasonStatisticsDto>>>
{
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetTeamStandingsHandler> _logger;

    public GetTeamStandingsHandler(
        IFootballStatisticsRepository statisticsRepository,
        ILogger<GetTeamStandingsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    public async Task<Result<List<FootballTeamSeasonStatisticsDto>>> Handle(GetTeamStandingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting team standings for Season: {SeasonId}", request.CompetitionId);

            List<FootballTeamSeasonStatistics> standings =
                (await _statisticsRepository.GetTeamStandingsAsync(request.CompetitionId, cancellationToken)).ToList();

            if (standings.Count == 0)
            {
                _logger.LogWarning("Team standings not found for Season: {SeasonId}", request.CompetitionId);
                return Result<List<FootballTeamSeasonStatisticsDto>>.NotFound("Team standings", request.CompetitionId.ToString());
            }

            List<FootballTeamSeasonStatisticsDto> standingsDtos = standings
                .Select(ts => FootballStatisticsMapper.ToDto(ts))
                .ToList();

            _logger.LogInformation("Successfully retrieved team standings for Season: {SeasonId} - {Count} teams", request.CompetitionId, standingsDtos.Count);
            return Result<List<FootballTeamSeasonStatisticsDto>>.Success(standingsDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting team standings for Season: {SeasonId}", request.CompetitionId);
            return Result<List<FootballTeamSeasonStatisticsDto>>.Failure("An error occurred while retrieving team standings.");
        }
    }
}
