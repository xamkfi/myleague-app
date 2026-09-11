using Application.Common;
using Application.Features.Football.Statistics.Mappings;
using Application.Features.Football.Statistics.Queries;
using Application.Features.Football.Teams.DTOs;
using Domain.Entities.Football.Statistics;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Statistics.Handlers;

/// <summary>
/// Returns a team's combined statistics aggregated across every competition (regular seasons +
/// tournaments) the team has participated in.
/// </summary>
public class GetAggregatedTeamStatisticsHandler : IRequestHandler<GetAggregatedTeamStatisticsQuery, Result<FootballTeamSeasonStatisticsDto>>
{
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly ILogger<GetAggregatedTeamStatisticsHandler> _logger;

    public GetAggregatedTeamStatisticsHandler(
        IFootballStatisticsRepository statisticsRepository,
        IFootballTeamRepository teamRepository,
        ILogger<GetAggregatedTeamStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _teamRepository = teamRepository;
        _logger = logger;
    }

    public async Task<Result<FootballTeamSeasonStatisticsDto>> Handle(GetAggregatedTeamStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Aggregating team statistics across all competitions for Team {TeamId}", request.TeamId);

            FootballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                _logger.LogWarning("Team not found when aggregating stats: {TeamId}", request.TeamId);
                return Result<FootballTeamSeasonStatisticsDto>.NotFound("Team", request.TeamId);
            }

            List<FootballTeamSeasonStatistics> rows = await _statisticsRepository.GetTeamSeasonStatisticsForTeamAsync(request.TeamId, cancellationToken);

            FootballTeamSeasonStatisticsDto dto = FootballStatisticsMapper.AggregateTeamStatistics(
                teamId: request.TeamId,
                rows: rows,
                teamName: team.Name,
                teamLogo: team.LogoUrl);

            _logger.LogInformation("Aggregated {RowCount} team stat rows for Team {TeamId}", rows.Count, request.TeamId);
            return Result<FootballTeamSeasonStatisticsDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aggregating team statistics for Team {TeamId}", request.TeamId);
            return Result<FootballTeamSeasonStatisticsDto>.Failure("An error occurred while retrieving aggregated team statistics.");
        }
    }
}
