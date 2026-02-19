using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Floorball.Matches.Commands;
using Application.Common;
using Application.Features.Common.MatchTimer.Services;
using Application.Constants;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Interfaces.Common;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Teams.Mappings;
using Application.Features.Floorball.Players.Mappings;
using Application.Features.Floorball.Referees.Mappings;
using Application.Features.Floorball.TeamManagers.Mappings;
using Application.Features.Floorball.Statistics.Mappings;
using Application.Services.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for completing a floorball match
/// </summary>
public class CompleteFloorballMatchHandler : IRequestHandler<CompleteFloorballMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly IMatchTimerService _timerService;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly ILogger<CompleteFloorballMatchHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CompleteFloorballMatchHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="statisticsRepository">The statistics repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="timerService">The timer service</param>
    /// <param name="logger">The logger</param>
    public CompleteFloorballMatchHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballUnitOfWork unitOfWork,
        IMatchTimerService timerService,
        INotificationSenderService notificationSenderService,
        ILogger<CompleteFloorballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _timerService = timerService;
        _notificationSenderService = notificationSenderService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CompleteFloorballMatchCommand request
    /// </summary>
    /// <param name="request">The command containing match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The completed match as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballMatchDto>> Handle(CompleteFloorballMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the match
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.Id);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.Id);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.Id} not found.");
            }

            _logger.LogInformation("Completing floorball match: {MatchId}", request.Id);
            match.Complete();

            // Update final team season statistics (wins/losses/ties)
            await UpdateFinalTeamSeasonStatistics(match, cancellationToken);

            // Update GamesPlayed for all players who participated in this match
            await UpdatePlayerGamesPlayed(match, cancellationToken);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Destroy the timer for this match to prevent background service queries
            try
            {
                _logger.LogInformation("Destroying timer for completed match: {MatchId}", request.Id);
                await _timerService.DestroyTimerAsync(request.Id);
                _logger.LogInformation("Successfully destroyed timer for completed match: {MatchId}", request.Id);
            }
            catch (Exception timerEx)
            {
                _logger.LogWarning(timerEx, "Failed to destroy timer for completed match: {MatchId}. This is non-critical.", request.Id);
                // Don't fail the match completion if timer destruction fails
            }

            await _notificationSenderService.SendNotificationAsync(
                FloorballNotificationEvents.MatchCompleted,
                new { MatchId = match.Id });

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            _logger.LogInformation("Successfully completed floorball match: {MatchId}", request.Id);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing floorball match: {MatchId}", request.Id);
            return Result<FloorballMatchDto>.Failure("An error occurred while completing the match.");
        }
    }

    /// <summary>
    /// Updates final team season statistics with match results (wins/losses/ties)
    /// </summary>
    private async Task UpdateFinalTeamSeasonStatistics(FloorballMatch match, CancellationToken cancellationToken)
    {
        // Update home team statistics
        await UpdateTeamMatchResult(match.HomeTeamId, match.SeasonId, match.HomeScore, match.AwayScore, true, match, cancellationToken);
        
        // Update away team statistics  
        await UpdateTeamMatchResult(match.AwayTeamId, match.SeasonId, match.AwayScore, match.HomeScore, false, match, cancellationToken);
    }

    /// <summary>
    /// Updates team season statistics with match result
    /// </summary>
    private async Task UpdateTeamMatchResult(Guid teamId, Guid seasonId, int teamScore, int opponentScore, bool isHomeGame, FloorballMatch match, CancellationToken cancellationToken)
    {
        FloorballTeamSeasonStatistics? teamStats = await _statisticsRepository.GetTeamSeasonStatisticsAsync(teamId, seasonId, cancellationToken);
        if (teamStats == null)
        {
            teamStats = new FloorballTeamSeasonStatistics(teamId, seasonId);
        }

        // Determine match result using enum
        FloorballGameResult gameResult;
        if (teamScore > opponentScore) gameResult = FloorballGameResult.Win;
        else if (teamScore < opponentScore) gameResult = FloorballGameResult.Loss;
        else gameResult = FloorballGameResult.Tie;

        // Update team statistics with match result
        teamStats.UpdateAfterMatch(
            gameResult: gameResult,
            isHomeGame: isHomeGame,
            goalsFor: teamScore,
            goalsAgainst: opponentScore);

        await _statisticsRepository.SaveTeamSeasonStatisticsAsync(teamStats, cancellationToken);
    }

    /// <summary>
    /// Collects all unique players from match events and goalies, then increments GamesPlayed
    /// </summary>
    private async Task UpdatePlayerGamesPlayed(FloorballMatch match, CancellationToken cancellationToken)
    {
        // (playerId, teamId) pairs to avoid double-counting a player
        HashSet<(Guid PlayerId, Guid TeamId)> participants = new();

        foreach (FloorballMatchEvent evt in match.Events)
        {
            switch (evt)
            {
                case FloorballGoal goal:
                    if (goal.ScoringPlayerId.HasValue)
                        participants.Add((goal.ScoringPlayerId.Value, goal.TeamId));
                    if (goal.AssistingPlayerId.HasValue)
                        participants.Add((goal.AssistingPlayerId.Value, goal.TeamId));
                    if (goal.SecondaryAssistingPlayerId.HasValue)
                        participants.Add((goal.SecondaryAssistingPlayerId.Value, goal.TeamId));
                    break;

                case FloorballPenalty penalty:
                    if (penalty.PlayerId.HasValue)
                        participants.Add((penalty.PlayerId.Value, penalty.TeamId));
                    break;

                case FloorballSave save:
                    participants.Add((save.GoalieId, save.TeamId));
                    break;
            }
        }

        if (match.HomeActiveGoalieId.HasValue)
            participants.Add((match.HomeActiveGoalieId.Value, match.HomeTeamId));
        if (match.AwayActiveGoalieId.HasValue)
            participants.Add((match.AwayActiveGoalieId.Value, match.AwayTeamId));

        _logger.LogInformation("[CompleteMatch] Updating GamesPlayed for {Count} players. MatchId={MatchId}",
            participants.Count, match.Id);

        foreach ((Guid playerId, Guid teamId) in participants)
        {
            FloorballPlayerSeasonStatistics? playerStats =
                await _statisticsRepository.GetPlayerSeasonStatisticsAsync(playerId, teamId, match.SeasonId, cancellationToken);

            if (playerStats == null)
            {
                playerStats = new FloorballPlayerSeasonStatistics(playerId, teamId, match.SeasonId);
            }

            playerStats.RecordGamePlayed();
            await _statisticsRepository.SavePlayerSeasonStatisticsAsync(playerStats, cancellationToken);
        }
    }
} 
