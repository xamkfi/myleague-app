using Application.Features.Floorball.Matches.Commands;
using Application.Common;
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
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for deleting a goal event from a floorball match (non-event-sourced)
/// </summary>
public class DeleteGoalHandler : IRequestHandler<DeleteGoalCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteGoalHandler> _logger;

    public DeleteGoalHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballPlayerRepository playerRepository,
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<DeleteGoalHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(DeleteGoalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            _logger.LogInformation("Deleting goal {GoalId} from match {MatchId}", request.GoalEventId, request.MatchId);

            // Delete the goal event and get the deleted goal
            FloorballGoal deletedGoal = match.DeleteGoalEvent(request.GoalEventId);
            _logger.LogInformation("Deleted goal period {Period} team {TeamId}; current period scores: H{Home}-A{Away}",
                deletedGoal.PeriodNumber,
                deletedGoal.TeamId,
                match.PeriodScores.FirstOrDefault(ps => ps.PeriodNumber == deletedGoal.PeriodNumber)?.HomeScore,
                match.PeriodScores.FirstOrDefault(ps => ps.PeriodNumber == deletedGoal.PeriodNumber)?.AwayScore);

            // Decrement player statistics
            if (deletedGoal.ScoringPlayerId.HasValue)
            {
                FloorballPlayer? scoringPlayer = await _playerRepository.GetByIdAsync(deletedGoal.ScoringPlayerId.Value);
                if (scoringPlayer != null)
                {
                    scoringPlayer.RemoveGoal();
                }
            }

            if (deletedGoal.AssistingPlayerId.HasValue)
            {
                FloorballPlayer? assistingPlayer = await _playerRepository.GetByIdAsync(deletedGoal.AssistingPlayerId.Value);
                if (assistingPlayer != null)
                {
                    assistingPlayer.RemoveAssist();
                }
            }

            if (deletedGoal.SecondaryAssistingPlayerId.HasValue)
            {
                FloorballPlayer? secondaryAssistingPlayer = await _playerRepository.GetByIdAsync(deletedGoal.SecondaryAssistingPlayerId.Value);
                if (secondaryAssistingPlayer != null)
                {
                    secondaryAssistingPlayer.RemoveAssist();
                }
            }

            // Decrement season statistics
            await RemovePlayerSeasonStatistics(deletedGoal, match, cancellationToken);

            // Decrement team season statistics (decrement goals for scoring team, goals against for opposing team).
            // Goals can only be recorded on a started match, so both team IDs are non-null here.
            await RemoveTeamSeasonGoalStatistics(deletedGoal.TeamId, match.CompetitionId, true, cancellationToken);
            Guid opposingTeamId = (deletedGoal.TeamId == match.HomeTeamId ? match.AwayTeamId : match.HomeTeamId)!.Value;
            await RemoveTeamSeasonGoalStatistics(opposingTeamId, match.CompetitionId, false, cancellationToken);

            // Decrement match team statistics
            await RemoveMatchTeamStatistics(match.Id, deletedGoal.TeamId, cancellationToken);

            // Decrement goalie statistics (shots against and goals allowed)
            await RemoveGoalieSeasonStatistics(match, deletedGoal.TeamId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting goal {GoalId} from match {MatchId}", request.GoalEventId, request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Removes player season statistics for goals and assists
    /// </summary>
    private async Task RemovePlayerSeasonStatistics(FloorballGoal goal, FloorballMatch match, CancellationToken cancellationToken)
    {
        // Remove scoring player season statistics
        if (goal.ScoringPlayerId.HasValue)
        {
            FloorballPlayerSeasonStatistics? playerStats = await _statisticsRepository.GetPlayerSeasonStatisticsAsync(
                goal.ScoringPlayerId.Value, goal.TeamId, match.CompetitionId, cancellationToken);
            if (playerStats != null)
            {
                // Determine goal type flags (simplified - could be enhanced based on goal type)
                bool isPowerPlay = goal.GoalType == Domain.Enums.Floorball.FloorballGoalType.PowerPlay;
                bool isShortHanded = goal.GoalType == Domain.Enums.Floorball.FloorballGoalType.ShortHanded;
                bool isGameWinning = false; // Would need more context to determine
                bool isOvertime = goal.GoalType == Domain.Enums.Floorball.FloorballGoalType.Overtime;

                playerStats.RemoveGoal(isPowerPlay, isShortHanded, isGameWinning, isOvertime);
                await _statisticsRepository.SavePlayerSeasonStatisticsAsync(playerStats, cancellationToken);
            }
        }

        // Remove assisting player season statistics
        if (goal.AssistingPlayerId.HasValue)
        {
            FloorballPlayerSeasonStatistics? assistStats = await _statisticsRepository.GetPlayerSeasonStatisticsAsync(
                goal.AssistingPlayerId.Value, goal.TeamId, match.CompetitionId, cancellationToken);
            if (assistStats != null)
            {
                // Determine assist type flags (simplified)
                bool isPowerPlay = goal.GoalType == Domain.Enums.Floorball.FloorballGoalType.PowerPlay;
                bool isShortHanded = goal.GoalType == Domain.Enums.Floorball.FloorballGoalType.ShortHanded;

                assistStats.RemoveAssist(isPowerPlay, isShortHanded);
                await _statisticsRepository.SavePlayerSeasonStatisticsAsync(assistStats, cancellationToken);
            }
        }

        // Remove secondary assisting player season statistics
        if (goal.SecondaryAssistingPlayerId.HasValue)
        {
            FloorballPlayerSeasonStatistics? secondaryAssistStats = await _statisticsRepository.GetPlayerSeasonStatisticsAsync(
                goal.SecondaryAssistingPlayerId.Value, goal.TeamId, match.CompetitionId, cancellationToken);
            if (secondaryAssistStats != null)
            {
                // Determine assist type flags (simplified)
                bool isPowerPlay = goal.GoalType == Domain.Enums.Floorball.FloorballGoalType.PowerPlay;
                bool isShortHanded = goal.GoalType == Domain.Enums.Floorball.FloorballGoalType.ShortHanded;

                secondaryAssistStats.RemoveAssist(isPowerPlay, isShortHanded);
                await _statisticsRepository.SavePlayerSeasonStatisticsAsync(secondaryAssistStats, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Removes team season statistics for goals scored
    /// </summary>
    private async Task RemoveTeamSeasonGoalStatistics(Guid teamId, Guid seasonId, bool isGoalFor, CancellationToken cancellationToken)
    {
        FloorballTeamSeasonStatistics? teamStats = await _statisticsRepository.GetTeamSeasonStatisticsAsync(teamId, seasonId, cancellationToken);
        if (teamStats != null)
        {
            if (isGoalFor)
            {
                teamStats.DecrementGoalsFor();
            }
            else
            {
                teamStats.DecrementGoalsAgainst();
            }

            await _statisticsRepository.SaveTeamSeasonStatisticsAsync(teamStats, cancellationToken);
        }
    }

    /// <summary>
    /// Removes match team statistics for shots and goals
    /// </summary>
    private async Task RemoveMatchTeamStatistics(Guid matchId, Guid teamId, CancellationToken cancellationToken)
    {
        FloorballMatchTeamStatistics? matchStats = await _statisticsRepository.GetMatchTeamStatisticsAsync(matchId, teamId, cancellationToken);
        if (matchStats != null)
        {
            // Goal counts as both a shot and a shot on goal, so remove both
            matchStats.RemoveShotStatistics(1, 1);

            await _statisticsRepository.SaveMatchTeamStatisticsAsync(matchStats, cancellationToken);
        }
    }

    /// <summary>
    /// Removes goalie season statistics when a goal is deleted (shots against and goals allowed)
    /// </summary>
    private async Task RemoveGoalieSeasonStatistics(FloorballMatch match, Guid scoringTeamId, CancellationToken cancellationToken)
    {
        // Find the opposing team (the one that allowed the goal). A started match has both team IDs set.
        Guid opposingTeamId = (scoringTeamId == match.HomeTeamId ? match.AwayTeamId : match.HomeTeamId)!.Value;

        // Update match team statistics for the opposing team (remove shots against)
        FloorballMatchTeamStatistics? opposingMatchStats = await _statisticsRepository.GetMatchTeamStatisticsAsync(match.Id, opposingTeamId, cancellationToken);
        if (opposingMatchStats != null)
        {
            // Remove both shot and shot on goal from the opposing team
            opposingMatchStats.RemoveShotStatistics(1, 1);
            await _statisticsRepository.SaveMatchTeamStatisticsAsync(opposingMatchStats, cancellationToken);
        }

        // Update the specific goalie's statistics if we know who was active
        Guid? activeGoalieId = match.GetActiveGoalieId(opposingTeamId);
        if (activeGoalieId.HasValue)
        {
            FloorballGoalieSeasonStatistics? goalieStats = await _statisticsRepository.GetGoalieSeasonStatisticsAsync(
                activeGoalieId.Value, opposingTeamId, match.CompetitionId, cancellationToken);

            if (goalieStats != null)
            {
                // Remove goal allowed: 1 shot against removed, 1 goal allowed removed, 0 saves removed
                goalieStats.RecordSaves(0, -1, -1); // Negative values to decrement
                await _statisticsRepository.SaveGoalieSeasonStatisticsAsync(goalieStats, cancellationToken);
            }
        }
    }
}



