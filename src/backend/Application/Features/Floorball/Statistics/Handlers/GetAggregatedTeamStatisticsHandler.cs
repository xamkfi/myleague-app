using Application.Common;
using Application.Features.Floorball.Statistics.Mappings;
using Application.Features.Floorball.Statistics.Queries;
using Application.Features.Floorball.Teams.DTOs;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Statistics.Handlers;

/// <summary>
/// Returns a team's combined statistics aggregated across every competition (regular seasons +
/// tournaments) the team has participated in. The team page used to call the per-competition
/// endpoint with only the current league season ID, which silently dropped any tournament games.
/// This handler walks all stat rows for the team and folds them into a single DTO so the
/// "Overall Record" block reflects every match the team has played.
/// </summary>
public class GetAggregatedTeamStatisticsHandler : IRequestHandler<GetAggregatedTeamStatisticsQuery, Result<FloorballTeamSeasonStatisticsDto>>
{
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly ILogger<GetAggregatedTeamStatisticsHandler> _logger;

    public GetAggregatedTeamStatisticsHandler(
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballTeamRepository teamRepository,
        ILogger<GetAggregatedTeamStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _teamRepository = teamRepository;
        _logger = logger;
    }

    public async Task<Result<FloorballTeamSeasonStatisticsDto>> Handle(GetAggregatedTeamStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Aggregating team statistics across all competitions for Team {TeamId}", request.TeamId);

            FloorballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                _logger.LogWarning("Team not found when aggregating stats: {TeamId}", request.TeamId);
                return Result<FloorballTeamSeasonStatisticsDto>.NotFound("Team", request.TeamId);
            }

            List<FloorballTeamSeasonStatistics> rows = await _statisticsRepository.GetTeamSeasonStatisticsForTeamAsync(request.TeamId, cancellationToken);

            FloorballTeamSeasonStatisticsDto dto = FloorballStatisticsMapper.AggregateTeamStatistics(
                teamId: request.TeamId,
                rows: rows,
                teamName: team.Name,
                teamLogo: team.LogoUrl);

            _logger.LogInformation("Aggregated {RowCount} team stat rows for Team {TeamId}", rows.Count, request.TeamId);
            return Result<FloorballTeamSeasonStatisticsDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aggregating team statistics for Team {TeamId}", request.TeamId);
            return Result<FloorballTeamSeasonStatisticsDto>.Failure("An error occurred while retrieving aggregated team statistics.");
        }
    }
}
