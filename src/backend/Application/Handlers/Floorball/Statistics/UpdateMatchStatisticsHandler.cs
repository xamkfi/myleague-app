using Application.Commands.Floorball.Statistics;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Floorball.Statistics;

/// <summary>
/// Handler for updating statistics after a match is completed
/// </summary>
public class UpdateMatchStatisticsHandler : IRequestHandler<UpdateMatchStatisticsCommand, Result>
{
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateMatchStatisticsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateMatchStatisticsHandler class
    /// </summary>
    /// <param name="statisticsRepository">The statistics repository</param>
    /// <param name="matchRepository">The match repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateMatchStatisticsHandler(
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballMatchRepository matchRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<UpdateMatchStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateMatchStatisticsCommand request
    /// </summary>
    /// <param name="request">The command containing the match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Result> Handle(UpdateMatchStatisticsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating statistics for Match {MatchId}", request.MatchId);

            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match {MatchId} not found", request.MatchId);
                return Result.NotFound("Match", request.MatchId);
            }

            if (match.Status != FloorballMatchStatus.Completed)
            {
                _logger.LogInformation("Match {MatchId} is not completed, skipping statistics update", request.MatchId);
                return Result.Success();
            }

            // Calculate and update match team statistics
            await UpdateMatchTeamStatistics(match, cancellationToken);

            // Update team season statistics
            await UpdateTeamSeasonStatistics(match, cancellationToken);

            // Update player statistics based on match events
            await UpdatePlayerStatistics(match, cancellationToken);

            // Invalidate season cache
            await _statisticsRepository.RemoveSeasonCacheAsync(match.SeasonId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully updated statistics for Match {MatchId}", request.MatchId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating statistics for Match {MatchId}", request.MatchId);
            return Result.Failure("An error occurred while updating match statistics.");
        }
    }

    private async Task UpdateMatchTeamStatistics(FloorballMatch match, CancellationToken cancellationToken)
    {
        // Create basic match statistics for both teams
        FloorballMatchTeamStatistics homeStats = new FloorballMatchTeamStatistics(match.Id, match.HomeTeamId);
        FloorballMatchTeamStatistics awayStats = new FloorballMatchTeamStatistics(match.Id, match.AwayTeamId);

        // Count events for each team
        foreach (FloorballMatchEvent matchEvent in match.Events)
        {
            FloorballMatchTeamStatistics stats = matchEvent.TeamId == match.HomeTeamId ? homeStats : awayStats;
            
            if (matchEvent is FloorballGoal)
            {
                stats.UpdateShotStatistics(1, 1); // Goal counts as shot on goal
            }
            else if (matchEvent is FloorballPenalty penalty)
            {
                stats.UpdatePenaltyMinutes(penalty.DurationInMinutes);
            }
        }

        await _statisticsRepository.SaveMatchTeamStatisticsAsync(homeStats, cancellationToken);
        await _statisticsRepository.SaveMatchTeamStatisticsAsync(awayStats, cancellationToken);
    }

    private async Task UpdateTeamSeasonStatistics(FloorballMatch match, CancellationToken cancellationToken)
    {
        await UpdateTeamSeasonStatisticsForTeam(match, match.HomeTeamId, true, cancellationToken);
        await UpdateTeamSeasonStatisticsForTeam(match, match.AwayTeamId, false, cancellationToken);
    }

    private async Task UpdateTeamSeasonStatisticsForTeam(FloorballMatch match, Guid teamId, bool isHomeTeam, CancellationToken cancellationToken)
    {
        FloorballTeamSeasonStatistics? teamStats = await _statisticsRepository.GetTeamSeasonStatisticsAsync(teamId, match.SeasonId, cancellationToken);
        if (teamStats == null)
        {
            teamStats = new FloorballTeamSeasonStatistics(teamId, match.SeasonId);
        }

        // Determine match result for this team
        string gameResult = "loss";
        if (isHomeTeam && match.HomeScore > match.AwayScore) gameResult = "win";
        else if (!isHomeTeam && match.AwayScore > match.HomeScore) gameResult = "win";
        else if (match.HomeScore == match.AwayScore) gameResult = "tie";

        int goalsFor = isHomeTeam ? match.HomeScore : match.AwayScore;
        int goalsAgainst = isHomeTeam ? match.AwayScore : match.HomeScore;

        teamStats.UpdateAfterMatch(
            gameResult: gameResult,
            isHomeGame: isHomeTeam,
            goalsFor: goalsFor,
            goalsAgainst: goalsAgainst);

        await _statisticsRepository.SaveTeamSeasonStatisticsAsync(teamStats, cancellationToken);
    }

    private async Task UpdatePlayerStatistics(FloorballMatch match, CancellationToken cancellationToken)
    {
        // Group events by player and update their statistics
        List<FloorballMatchEvent> playerEvents = match.Events
            .Where(e => e is FloorballGoal || e is FloorballPenalty)
            .ToList();

        foreach (FloorballMatchEvent playerEvent in playerEvents)
        {
            if (playerEvent is FloorballGoal goal)
            {
                if (goal.ScoringPlayerId.HasValue)
                    await UpdatePlayerSeasonStatisticsForGoalOrAssist(goal.ScoringPlayerId.Value, match, true, false, cancellationToken);
                if (goal.AssistingPlayerId.HasValue)
                    await UpdatePlayerSeasonStatisticsForGoalOrAssist(goal.AssistingPlayerId.Value, match, false, true, cancellationToken);
                if (goal.SecondaryAssistingPlayerId.HasValue)
                    await UpdatePlayerSeasonStatisticsForGoalOrAssist(goal.SecondaryAssistingPlayerId.Value, match, false, true, cancellationToken);
            }
            else if (playerEvent is FloorballPenalty penalty && penalty.PlayerId.HasValue)
            {
                await UpdatePlayerSeasonStatisticsForPenalty(penalty.PlayerId.Value, match, penalty.DurationInMinutes, cancellationToken);
            }
        }
    }

    private async Task UpdatePlayerSeasonStatisticsForGoalOrAssist(Guid playerId, FloorballMatch match, bool isGoal, bool isAssist, CancellationToken cancellationToken)
    {
        // Determine which team the player is on by checking the goal events
        Guid? teamId = DeterminePlayerTeam(playerId, match);
        if (teamId == null) return;

        FloorballPlayerSeasonStatistics? playerStats = await _statisticsRepository.GetPlayerSeasonStatisticsAsync(playerId, teamId.Value, match.SeasonId, cancellationToken);
        if (playerStats == null)
        {
            playerStats = new FloorballPlayerSeasonStatistics(playerId, teamId.Value, match.SeasonId);
        }

        if (isGoal) playerStats.RecordGoal();
        if (isAssist) playerStats.RecordAssist();

        await _statisticsRepository.SavePlayerSeasonStatisticsAsync(playerStats, cancellationToken);
    }

    private async Task UpdatePlayerSeasonStatisticsForPenalty(Guid playerId, FloorballMatch match, int penaltyMinutes, CancellationToken cancellationToken)
    {
        Guid? teamId = DeterminePlayerTeam(playerId, match);
        if (teamId == null) return;

        FloorballPlayerSeasonStatistics? playerStats = await _statisticsRepository.GetPlayerSeasonStatisticsAsync(playerId, teamId.Value, match.SeasonId, cancellationToken);
        if (playerStats == null)
        {
            playerStats = new FloorballPlayerSeasonStatistics(playerId, teamId.Value, match.SeasonId);
        }

        playerStats.RecordPenaltyMinutes(penaltyMinutes);

        await _statisticsRepository.SavePlayerSeasonStatisticsAsync(playerStats, cancellationToken);
    }

    private Guid? DeterminePlayerTeam(Guid playerId, FloorballMatch match)
    {
        // Check if player appears in any events for home or away team
        IEnumerable<FloorballMatchEvent> playerEvents = match.Events.Where(e => 
            (e is FloorballGoal goal && (goal.ScoringPlayerId == playerId || goal.AssistingPlayerId == playerId || goal.SecondaryAssistingPlayerId == playerId)) ||
            (e is FloorballPenalty penalty && penalty.PlayerId == playerId));

        FloorballMatchEvent? teamEvent = playerEvents.FirstOrDefault();
        return teamEvent?.TeamId;
    }
}
