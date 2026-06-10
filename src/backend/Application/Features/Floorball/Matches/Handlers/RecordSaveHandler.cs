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
using Application.Interfaces.Common;
using Application.Constants;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for recording a save in a floorball match
/// </summary>
public class RecordSaveHandler : IRequestHandler<RecordSaveCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly ILogger<RecordSaveHandler> _logger;

    public RecordSaveHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballPlayerRepository playerRepository,
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballUnitOfWork unitOfWork,
        INotificationSenderService notificationSenderService,
        ILogger<RecordSaveHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _notificationSenderService = notificationSenderService;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(RecordSaveCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            FloorballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                _logger.LogWarning("Team not found with ID: {TeamId}", request.TeamId);
                return Result<FloorballMatchDto>.Failure($"Team with ID {request.TeamId} not found.");
            }

            FloorballPlayer? goalie = await _playerRepository.GetByIdAsync(request.GoalieId);
            if (goalie == null)
            {
                _logger.LogWarning("Goalie not found with ID: {GoalieId}", request.GoalieId);
                return Result<FloorballMatchDto>.Failure($"Goalie with ID {request.GoalieId} not found.");
            }

            // Defensive clamp: the controller validates [1, 99] but legacy callers that
            // construct the command directly (older tests, future internal flows) might pass
            // Count = 0. Treating < 1 as 1 keeps the single-save semantics identical to
            // before this field existed.
            int saveCount = request.Count < 1 ? 1 : request.Count;

            _logger.LogInformation(
                "Recording {Count} save(s) in match {MatchId}", saveCount, request.MatchId);

            // Verify active goalie once (the discrepancy is the same for every save in the
            // bulk batch, so logging it N times would just be noise).
            Guid? activeGoalieId = match.GetActiveGoalieId(request.TeamId);
            if (activeGoalieId.HasValue && activeGoalieId.Value != goalie.Id)
            {
                _logger.LogWarning("Save recorded for goalie {GoalieId} but active goalie is {ActiveGoalieId} for team {TeamId}",
                    goalie.Id, activeGoalieId.Value, request.TeamId);
                // Still record the save(s) but log the discrepancy.
            }

            for (int i = 0; i < saveCount; i++)
            {
                FloorballSave saveEvent = match.RecordSave(
                    team,
                    goalie,
                    request.PeriodNumber,
                    request.TimeInSeconds,
                    request.WasInOvertime,
                    request.WasInShootout);

                _matchRepository.MarkEventAsAdded(saveEvent);
            }

            // Aggregate stats updates so we hit the statistics tables once per request rather
            // than once per save — the per-save UpdateGoalieSeasonStatistics / UpdateMatch...
            // calls are pure increments, so collapsing them into a single batched call yields
            // the same end state.
            await UpdateGoalieSeasonStatistics(goalie.Id, request.TeamId, match.CompetitionId, saveCount, saveCount, 0, cancellationToken);

            // Update match team statistics for the attacking team (the team that took the shot that was saved).
            // The save is for the defending team (request.TeamId), so we need to update the opposing team's stats.
            // A started match has both team IDs assigned, so .Value is safe here.
            Guid attackingTeamId = (request.TeamId == match.HomeTeamId ? match.AwayTeamId : match.HomeTeamId)!.Value;
            await UpdateMatchTeamStatistics(match.Id, attackingTeamId, saveCount, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Single notification regardless of bulk size — listeners (e.g. spectator pages,
            // analytics) only need one cue to refresh and we don't want to spam them with N
            // identical events.
            await _notificationSenderService.SendNotificationAsync(
                FloorballNotificationEvents.SaveRecorded,
                new MatchNotificationPayload(match.Id));

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording save in match {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while recording the save.");
        }
    }

    /// <summary>
    /// Updates goalie season statistics for saves and shots faced
    /// </summary>
    private async Task UpdateGoalieSeasonStatistics(Guid goalieId, Guid teamId, Guid seasonId, int saves, int shotsAgainst, int goalsAllowed, CancellationToken cancellationToken)
    {
        FloorballGoalieSeasonStatistics? goalieStats = await _statisticsRepository.GetGoalieSeasonStatisticsAsync(goalieId, teamId, seasonId, cancellationToken);
        if (goalieStats == null)
        {
            goalieStats = new FloorballGoalieSeasonStatistics(goalieId, teamId, seasonId);
        }

        goalieStats.RecordSaves(saves, shotsAgainst, goalsAllowed);
        await _statisticsRepository.SaveGoalieSeasonStatisticsAsync(goalieStats, cancellationToken);
    }

    /// <summary>
    /// Updates match team statistics for shots faced
    /// </summary>
    private async Task UpdateMatchTeamStatistics(Guid matchId, Guid teamId, int shotsAgainst, CancellationToken cancellationToken)
    {
        FloorballMatchTeamStatistics? matchStats = await _statisticsRepository.GetMatchTeamStatisticsAsync(matchId, teamId, cancellationToken);
        if (matchStats == null)
        {
            matchStats = new FloorballMatchTeamStatistics(matchId, teamId);
        }

        // Only shots against, no shots on goal (since it was saved)
        matchStats.UpdateShotStatistics(0, shotsAgainst);

        await _statisticsRepository.SaveMatchTeamStatisticsAsync(matchStats, cancellationToken);
    }
}


