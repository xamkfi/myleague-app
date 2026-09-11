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
/// Handler for retrieving team season statistics
/// </summary>
public class GetTeamSeasonStatisticsHandler : IRequestHandler<GetTeamSeasonStatisticsQuery, Result<FootballTeamSeasonStatisticsDto>>
{
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetTeamSeasonStatisticsHandler> _logger;

    public GetTeamSeasonStatisticsHandler(
        IFootballStatisticsRepository statisticsRepository,
        ILogger<GetTeamSeasonStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    public async Task<Result<FootballTeamSeasonStatisticsDto>> Handle(GetTeamSeasonStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving team statistics for Team {TeamId} in Competition {CompetitionId}", request.TeamId, request.CompetitionId);

            FootballTeamSeasonStatistics? statistics = await _statisticsRepository.GetTeamSeasonStatisticsAsync(request.TeamId, request.CompetitionId, cancellationToken);

            if (statistics == null)
            {
                _logger.LogInformation("No statistics found for Team {TeamId} in Competition {CompetitionId}", request.TeamId, request.CompetitionId);
                return Result<FootballTeamSeasonStatisticsDto>.NotFound("TeamSeasonStatistics", $"TeamId: {request.TeamId}, CompetitionId: {request.CompetitionId}");
            }

            FootballTeamSeasonStatisticsDto dto = FootballStatisticsMapper.ToDto(statistics);

            _logger.LogInformation("Successfully retrieved team statistics for Team {TeamId} in Competition {CompetitionId}", request.TeamId, request.CompetitionId);
            return Result<FootballTeamSeasonStatisticsDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving team statistics for Team {TeamId} in Season {SeasonId}", request.TeamId, request.CompetitionId);
            return Result<FootballTeamSeasonStatisticsDto>.Failure("An error occurred while retrieving team statistics.");
        }
    }
}
