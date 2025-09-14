using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Queries.Floorball.Statistics;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Floorball.Statistics;

/// <summary>
/// Handler for retrieving season statistics summary
/// </summary>
public class GetSeasonStatisticsSummaryHandler : IRequestHandler<GetSeasonStatisticsSummaryQuery, Result<FloorballSeasonStatisticsSummaryDto>>
{
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetSeasonStatisticsSummaryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetSeasonStatisticsSummaryHandler class
    /// </summary>
    /// <param name="statisticsRepository">The statistics repository</param>
    /// <param name="logger">The logger</param>
    public GetSeasonStatisticsSummaryHandler(
        IFloorballStatisticsRepository statisticsRepository,
        ILogger<GetSeasonStatisticsSummaryHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetSeasonStatisticsSummaryQuery request
    /// </summary>
    /// <param name="request">The query containing the season ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing season statistics summary DTO</returns>
    public async Task<Result<FloorballSeasonStatisticsSummaryDto>> Handle(GetSeasonStatisticsSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting season statistics summary for Season: {SeasonId}", request.SeasonId);

            // Get team standings
            List<Domain.Entities.Floorball.FloorballTeamSeasonStatistics> teamStats = 
                (await _statisticsRepository.GetTeamStandingsAsync(request.SeasonId, cancellationToken)).ToList();

            // Get top scorers
            List<Domain.Entities.Floorball.FloorballPlayerSeasonStatistics> topScorers = 
                (await _statisticsRepository.GetTopScorersAsync(request.SeasonId, 10, cancellationToken)).ToList();

            // Get top assist leaders
            List<Domain.Entities.Floorball.FloorballPlayerSeasonStatistics> topAssists = 
                (await _statisticsRepository.GetTopAssistsAsync(request.SeasonId, 10, cancellationToken)).ToList();

            // Get top goalies (minimum 5 games played)
            List<Domain.Entities.Floorball.FloorballGoalieSeasonStatistics> topGoalies = 
                (await _statisticsRepository.GetTopGoaliesAsync(request.SeasonId, 10, 5, cancellationToken)).ToList();

            if (teamStats.Count == 0)
            {
                _logger.LogWarning("Season statistics not found for Season: {SeasonId}", request.SeasonId);
                return Result<FloorballSeasonStatisticsSummaryDto>.NotFound("Season statistics", request.SeasonId.ToString());
            }

            // Calculate summary statistics
            int totalGames = teamStats.Sum(ts => ts.GamesPlayed) / 2; // Divide by 2 since each game involves 2 teams
            int totalGoals = teamStats.Sum(ts => ts.GoalsFor);
            decimal averageGoalsPerGame = totalGames > 0 ? (decimal)totalGoals / totalGames : 0;

            FloorballSeasonStatisticsSummaryDto summaryDto = new FloorballSeasonStatisticsSummaryDto
            {
                SeasonId = request.SeasonId,
                TeamStandings = teamStats.Select(ts => FloorballStatisticsMapper.ToDto(ts)).ToList(),
                TopScorers = topScorers.Select(ps => FloorballStatisticsMapper.ToDto(ps)).ToList(),
                TopAssists = topAssists.Select(ps => FloorballStatisticsMapper.ToDto(ps)).ToList(),
                TopGoalies = topGoalies.Select(gs => FloorballStatisticsMapper.ToDto(gs)).ToList(),
                TotalGames = totalGames,
                TotalGoals = totalGoals,
                AverageGoalsPerGame = averageGoalsPerGame
            };
            
            _logger.LogInformation("Successfully retrieved season statistics summary for Season: {SeasonId}", request.SeasonId);
            return Result<FloorballSeasonStatisticsSummaryDto>.Success(summaryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting season statistics summary for Season: {SeasonId}", request.SeasonId);
            return Result<FloorballSeasonStatisticsSummaryDto>.Failure("An error occurred while retrieving season statistics summary.");
        }
    }
}
