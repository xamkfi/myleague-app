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
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;
using Domain.Enums.Floorball;
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
public class RecordGoalHandler : IRequestHandler<RecordFloorballGoalCommand, Result<FloorballMatchDto>>
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
    /// Handles the RecordFloorballGoalCommand request
    /// </summary>
    /// <param name="request">The command containing goal information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated match as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballMatchDto>> Handle(RecordFloorballGoalCommand request, CancellationToken cancellationToken)
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
                request.Description, request.GoalType, request.WasDelayedPenalty);

            bool isOwnGoal = goal.GoalType == FloorballGoalType.OwnGoal
                || request.GoalType == (int)FloorballGoalType.OwnGoal;

            // Own goals must not inflate the defending player's career/season goal totals
            // (parity with football RecordGoalHandler).
            if (!isOwnGoal)
            {
                scoringPlayer.RecordGoal();
                await UpdatePlayerSeasonStatistics(scoringPlayer.Id, request.ScoringTeamId, match.CompetitionId, true, false, cancellationToken);
                if (assistingPlayer != null)
                {
                    assistingPlayer.RecordAssist();
                    await UpdatePlayerSeasonStatistics(assistingPlayer.Id, request.ScoringTeamId, match.CompetitionId, false, true, cancellationToken);
                }
                if (secondAssistingPlayer != null)
                {
                    secondAssistingPlayer.RecordAssist();
                    await UpdatePlayerSeasonStatistics(secondAssistingPlayer.Id, request.ScoringTeamId, match.CompetitionId, false, true, cancellationToken);
                }
            }

            // Update team season statistics (increment goals for scoring team, goals against for opposing team).
            // Both team IDs are non-null on a started match.
            await UpdateTeamSeasonGoalStatistics(request.ScoringTeamId, match.CompetitionId, true, cancellationToken);
            Guid opposingTeamId = (request.ScoringTeamId == match.HomeTeamId ? match.AwayTeamId : match.HomeTeamId)!.Value;
            await UpdateTeamSeasonGoalStatistics(opposingTeamId, match.CompetitionId, false, cancellationToken);

            await FloorballMatchStatisticsSupport.SyncMatchShotsAsync(_statisticsRepository, match, cancellationToken);

            // The domain leaves the goalie empty for goals no goalie is charged with (own goal,
            // empty net, shootout).
            if (goal.GoalieId.HasValue)
            {
                await FloorballMatchStatisticsSupport.AdjustGoalieAsync(
                    _statisticsRepository, goal.GoalieId.Value, opposingTeamId, match.CompetitionId, saves: 0, goalsAgainst: 1, cancellationToken);
            }

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
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected RecordGoal for {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid RecordGoal for {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message, ex.Flatten());
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

}
