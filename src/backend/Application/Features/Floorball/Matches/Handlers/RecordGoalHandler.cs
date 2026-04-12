using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Teams.Mappings;
using Application.Features.Floorball.Players.Mappings;
using Application.Features.Floorball.Referees.Mappings;
using Application.Features.Floorball.TeamManagers.Mappings;
using Application.Features.Floorball.Statistics.Mappings;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories.Common;
using Application.Interfaces.Common;
using Application.Constants;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for recording a goal in a floorball match
/// </summary>
public class RecordGoalHandler : IRequestHandler<RecordGoalCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly ILogger<RecordGoalHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the RecordGoalHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="statisticsRepository">The statistics repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public RecordGoalHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballPlayerRepository playerRepository,
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballUnitOfWork unitOfWork,
        INotificationSenderService notificationSenderService,
        ILogger<RecordGoalHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _notificationSenderService = notificationSenderService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the RecordGoalCommand request
    /// </summary>
    /// <param name="request">The command containing goal information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated match as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballMatchDto>> Handle(RecordGoalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the match
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            // Get the scoring team
            FloorballTeam? scoringTeam = await _teamRepository.GetByIdAsync(request.ScoringTeamId);
            if (scoringTeam == null)
            {
                _logger.LogWarning("Scoring team not found with ID: {TeamId}", request.ScoringTeamId);
                return Result<FloorballMatchDto>.Failure($"Scoring team with ID {request.ScoringTeamId} not found.");
            }

            // Get the scoring player
            FloorballPlayer? scoringPlayer = await _playerRepository.GetByIdAsync(request.ScoringPlayerId);
            if (scoringPlayer == null)
            {
                _logger.LogWarning("Scoring player not found with ID: {PlayerId}", request.ScoringPlayerId);
                return Result<FloorballMatchDto>.Failure($"Scoring player with ID {request.ScoringPlayerId} not found.");
            }

            // Get the assisting player (optional)
            FloorballPlayer? assistingPlayer = null;
            if (request.AssistingPlayerId.HasValue)
            {
                assistingPlayer = await _playerRepository.GetByIdAsync(request.AssistingPlayerId.Value);
                if (assistingPlayer == null)
                {
                    _logger.LogWarning("Assisting player not found with ID: {PlayerId}", request.AssistingPlayerId.Value);
                    return Result<FloorballMatchDto>.Failure($"Assisting player with ID {request.AssistingPlayerId.Value} not found.");
                }
            }

            // Get the second assisting player (optional)
            FloorballPlayer? secondAssistingPlayer = null;
            if (request.SecondaryAssistingPlayerId.HasValue)
            {
                secondAssistingPlayer = await _playerRepository.GetByIdAsync(request.SecondaryAssistingPlayerId.Value);
                if (secondAssistingPlayer == null)
                {
                    _logger.LogWarning("Assisting player not found with ID: {PlayerId}", request.SecondaryAssistingPlayerId.Value);
                    return Result<FloorballMatchDto>.Failure($"Assisting player with ID {request.SecondaryAssistingPlayerId.Value} not found.");
                }
            }

            _logger.LogInformation("[RecordGoal] Recording goal. MatchId={MatchId}, TeamId={TeamId}, PlayerId={PlayerId}, Period={Period}, T={Time}", 
                request.MatchId, request.ScoringTeamId, request.ScoringPlayerId, request.PeriodNumber, request.TimeInSeconds);

            FloorballGoal goal = match.RecordGoal(scoringTeam, scoringPlayer,
                assistingPlayer, secondAssistingPlayer,
                request.PeriodNumber, request.TimeInSeconds,
                request.Description, request.GoalType);

            //Adding goals/assists to player statistics
            scoringPlayer.RecordGoal();
            if (assistingPlayer != null) assistingPlayer.RecordAssist();
            if (secondAssistingPlayer != null) secondAssistingPlayer.RecordAssist();

            // Update season statistics immediately
            _logger.LogInformation("[RecordGoal] UpdatePlayerSeasonStatistics scorer: PlayerId={PlayerId}, TeamId={TeamId}, SeasonId={SeasonId}",
                scoringPlayer.Id, request.ScoringTeamId, match.CompetitionId);
            await UpdatePlayerSeasonStatistics(scoringPlayer.Id, request.ScoringTeamId, match.CompetitionId, true, false, cancellationToken);
            if (assistingPlayer != null)
            {
                _logger.LogInformation("[RecordGoal] UpdatePlayerSeasonStatistics assister: PlayerId={PlayerId}, TeamId={TeamId}, SeasonId={SeasonId}",
                    assistingPlayer.Id, request.ScoringTeamId, match.CompetitionId);
                await UpdatePlayerSeasonStatistics(assistingPlayer.Id, request.ScoringTeamId, match.CompetitionId, false, true, cancellationToken);
            }
            if (secondAssistingPlayer != null)
            {
                _logger.LogInformation("[RecordGoal] UpdatePlayerSeasonStatistics secondAssister: PlayerId={PlayerId}, TeamId={TeamId}, SeasonId={SeasonId}",
                    secondAssistingPlayer.Id, request.ScoringTeamId, match.CompetitionId);
                await UpdatePlayerSeasonStatistics(secondAssistingPlayer.Id, request.ScoringTeamId, match.CompetitionId, false, true, cancellationToken);
            }

            // Update team season statistics (increment goals for scoring team, goals against for opposing team)
            await UpdateTeamSeasonGoalStatistics(request.ScoringTeamId, match.CompetitionId, true, cancellationToken);
            Guid opposingTeamId = request.ScoringTeamId == match.HomeTeamId ? match.AwayTeamId : match.HomeTeamId;
            await UpdateTeamSeasonGoalStatistics(opposingTeamId, match.CompetitionId, false, cancellationToken);

            // Update match team statistics
            await UpdateMatchTeamStatistics(match.Id, request.ScoringTeamId, cancellationToken);

            // Update goalie statistics (shots against and goals allowed for the opposing team)
            await UpdateGoalieSeasonStatistics(match, request.ScoringTeamId, cancellationToken);

            // Mark the goal event as added and ensure the match is tracked as modified
            _matchRepository.MarkEventAsAdded(goal);

            //Update match table (homescore/awayscore)
            await _matchRepository.UpdateAsync(match);

            // Save changes explicitly to persist score and period updates and trigger domain events
            _logger.LogInformation("[RecordGoal] Saving match changes (scores, events). MatchId={MatchId}", match.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationSenderService.SendNotificationAsync(
                FloorballNotificationEvents.GoalScored,
                new MatchNotificationPayload(match.Id));

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            _logger.LogInformation("Successfully recorded goal in match {MatchId} by player {PlayerId}", request.MatchId, request.ScoringPlayerId);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording goal in match {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while recording the goal.");
        }
    }

    /// <summary>
    /// Updates player season statistics for goals and assists
    /// </summary>
    private async Task UpdatePlayerSeasonStatistics(Guid playerId, Guid teamId, Guid seasonId, bool isGoal, bool isAssist, CancellationToken cancellationToken)
    {
        FloorballPlayerSeasonStatistics? playerStats = await _statisticsRepository.GetPlayerSeasonStatisticsAsync(playerId, teamId, seasonId, cancellationToken);
        if (playerStats == null)
        {
            playerStats = new FloorballPlayerSeasonStatistics(playerId, teamId, seasonId);
        }

        if (isGoal) playerStats.RecordGoal();
        if (isAssist) playerStats.RecordAssist();

        await _statisticsRepository.SavePlayerSeasonStatisticsAsync(playerStats, cancellationToken);
    }

    /// <summary>
    /// Updates team season statistics for goals scored
    /// </summary>
    private async Task UpdateTeamSeasonGoalStatistics(Guid teamId, Guid seasonId, bool isGoalFor, CancellationToken cancellationToken)
    {
        FloorballTeamSeasonStatistics? teamStats = await _statisticsRepository.GetTeamSeasonStatisticsAsync(teamId, seasonId, cancellationToken);
        if (teamStats == null)
        {
            teamStats = new FloorballTeamSeasonStatistics(teamId, seasonId);
        }

        // Update goals using the entity methods
        if (isGoalFor)
        {
            teamStats.IncrementGoalsFor();
        }
        else
        {
            teamStats.IncrementGoalsAgainst();
        }

        await _statisticsRepository.SaveTeamSeasonStatisticsAsync(teamStats, cancellationToken);
    }

    /// <summary>
    /// Updates match team statistics for shots and goals
    /// </summary>
    private async Task UpdateMatchTeamStatistics(Guid matchId, Guid teamId, CancellationToken cancellationToken)
    {
        FloorballMatchTeamStatistics? matchStats = await _statisticsRepository.GetMatchTeamStatisticsAsync(matchId, teamId, cancellationToken);
        if (matchStats == null)
        {
            matchStats = new FloorballMatchTeamStatistics(matchId, teamId);
        }

        // Goal counts as both a shot and a shot on goal
        matchStats.UpdateShotStatistics(1, 1);

        await _statisticsRepository.SaveMatchTeamStatisticsAsync(matchStats, cancellationToken);
    }

    /// <summary>
    /// Updates goalie season statistics when a goal is scored (shots against and goals allowed)
    /// </summary>
    private async Task UpdateGoalieSeasonStatistics(FloorballMatch match, Guid scoringTeamId, CancellationToken cancellationToken)
    {
        // Find the opposing team (the one that allowed the goal)
        Guid opposingTeamId = scoringTeamId == match.HomeTeamId ? match.AwayTeamId : match.HomeTeamId;

        // Note: When a goal is scored, only the scoring team gets a shot increment
        // The opposing team's goalie stats are updated below, but not their team shot stats

        // Update the specific goalie's statistics if we know who was active
        Guid? activeGoalieId = match.GetActiveGoalieId(opposingTeamId);
        if (activeGoalieId.HasValue)
        {
            FloorballGoalieSeasonStatistics? goalieStats = await _statisticsRepository.GetGoalieSeasonStatisticsAsync(
                activeGoalieId.Value, opposingTeamId, match.CompetitionId, cancellationToken);

            if (goalieStats == null)
            {
                goalieStats = new FloorballGoalieSeasonStatistics(activeGoalieId.Value, opposingTeamId, match.CompetitionId);
            }

            // Goal allowed: 1 shot against, 1 goal allowed, 0 saves
            goalieStats.RecordSaves(0, 1, 1);
            await _statisticsRepository.SaveGoalieSeasonStatisticsAsync(goalieStats, cancellationToken);
        }
    }
} 
