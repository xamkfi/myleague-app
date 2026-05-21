using Application.Common;
using Application.Constants;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Mappings;
using Application.Interfaces.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Reopens a previously completed match back into <see cref="FloorballMatchStatus.InProgress"/>.
/// Mirrors the work that <see cref="CompleteFloorballMatchHandler"/> performs at completion time
/// in reverse: per-match aggregates are decremented on team / player / goalie season statistics
/// so the totals do not double-count when the match is later re-completed. Playoff matches are
/// intentionally rejected because their completion side-effects (winner propagation into the next
/// bracket slot, optional 3rd-place feed, tournament champion / auto-completion) are not safe to
/// blindly reverse without knowing whether the downstream matches have already started.
/// </summary>
public class ReopenFloorballMatchHandler : IRequestHandler<ReopenFloorballMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly ILogger<ReopenFloorballMatchHandler> _logger;

    public ReopenFloorballMatchHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballUnitOfWork unitOfWork,
        INotificationSenderService notificationSenderService,
        ILogger<ReopenFloorballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _notificationSenderService = notificationSenderService;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(ReopenFloorballMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.Id);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.Id);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.Id} not found.");
            }

            if (match.PlayoffRound.HasValue)
            {
                _logger.LogWarning("Refusing to reopen playoff match {MatchId} (round={Round}). Playoff propagation rollback is not supported.", request.Id, match.PlayoffRound);
                return Result<FloorballMatchDto>.Failure("Playoff matches cannot be reopened automatically. Please contact an administrator if a playoff result was recorded by mistake.");
            }

            _logger.LogInformation("Reopening floorball match: {MatchId}", request.Id);

            // Reverse the per-match aggregates that CompleteFloorballMatchHandler applied. Goal/save
            // event totals are tracked incrementally elsewhere and stay intact.
            await UndoTeamSeasonStatistics(match, cancellationToken);
            await UndoPlayerGamesPlayed(match, cancellationToken);
            await UndoGoalieGamesPlayed(match, cancellationToken);

            match.ReopenFromCompleted();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationSenderService.SendNotificationAsync(
                FloorballNotificationEvents.MatchReopened,
                new MatchNotificationPayload(match.Id));

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            _logger.LogInformation("Successfully reopened floorball match: {MatchId}", request.Id);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot reopen match {MatchId}: {Message}", request.Id, ex.Message);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reopening floorball match: {MatchId}", request.Id);
            return Result<FloorballMatchDto>.Failure("An error occurred while reopening the match.");
        }
    }

    private async Task UndoTeamSeasonStatistics(FloorballMatch match, CancellationToken cancellationToken)
    {
        FloorballGameResult homeResult;
        FloorballGameResult awayResult;

        if (match.HomeScore > match.AwayScore)
        {
            homeResult = FloorballGameResult.Win;
            awayResult = FloorballGameResult.Loss;
        }
        else if (match.HomeScore < match.AwayScore)
        {
            homeResult = FloorballGameResult.Loss;
            awayResult = FloorballGameResult.Win;
        }
        else
        {
            homeResult = FloorballGameResult.Tie;
            awayResult = FloorballGameResult.Tie;
        }

        await UndoSingleTeamSeasonStatistics(match.HomeTeamId, match.CompetitionId, homeResult, isHomeGame: true, cancellationToken);
        await UndoSingleTeamSeasonStatistics(match.AwayTeamId, match.CompetitionId, awayResult, isHomeGame: false, cancellationToken);
    }

    private async Task UndoSingleTeamSeasonStatistics(Guid teamId, Guid competitionId, FloorballGameResult result, bool isHomeGame, CancellationToken cancellationToken)
    {
        FloorballTeamSeasonStatistics? teamStats = await _statisticsRepository.GetTeamSeasonStatisticsAsync(teamId, competitionId, cancellationToken);
        if (teamStats == null)
        {
            _logger.LogWarning("No team season statistics found to undo for team {TeamId} / competition {CompetitionId}.", teamId, competitionId);
            return;
        }

        teamStats.UndoMatchResult(result, isHomeGame);
        await _statisticsRepository.SaveTeamSeasonStatisticsAsync(teamStats, cancellationToken);
    }

    private async Task UndoPlayerGamesPlayed(FloorballMatch match, CancellationToken cancellationToken)
    {
        HashSet<(Guid PlayerId, Guid TeamId)> participants = CollectMatchParticipants(match);

        _logger.LogInformation("[ReopenMatch] Decrementing GamesPlayed for {Count} players. MatchId={MatchId}", participants.Count, match.Id);

        foreach ((Guid playerId, Guid teamId) in participants)
        {
            FloorballPlayerSeasonStatistics? playerStats =
                await _statisticsRepository.GetPlayerSeasonStatisticsAsync(playerId, teamId, match.CompetitionId, cancellationToken);

            if (playerStats == null) continue;

            playerStats.RemoveGamePlayed();
            await _statisticsRepository.SavePlayerSeasonStatisticsAsync(playerStats, cancellationToken);
        }
    }

    private async Task UndoGoalieGamesPlayed(FloorballMatch match, CancellationToken cancellationToken)
    {
        FloorballGameResult homeResult;
        FloorballGameResult awayResult;

        if (match.HomeScore > match.AwayScore)
        {
            homeResult = FloorballGameResult.Win;
            awayResult = FloorballGameResult.Loss;
        }
        else if (match.HomeScore < match.AwayScore)
        {
            homeResult = FloorballGameResult.Loss;
            awayResult = FloorballGameResult.Win;
        }
        else
        {
            homeResult = FloorballGameResult.Tie;
            awayResult = FloorballGameResult.Tie;
        }

        int matchDurationMinutes = match.MatchRules.PeriodDurationMinutes * match.MatchRules.NumberOfPeriods;

        bool homeGoalieShutout = match.AwayScore == 0;
        bool awayGoalieShutout = match.HomeScore == 0;

        if (match.HomeActiveGoalieId.HasValue)
        {
            await UndoSingleGoalieGamePlayed(
                match.HomeActiveGoalieId.Value, match.HomeTeamId, match.CompetitionId,
                homeResult, matchDurationMinutes, homeGoalieShutout, cancellationToken);
        }

        if (match.AwayActiveGoalieId.HasValue)
        {
            await UndoSingleGoalieGamePlayed(
                match.AwayActiveGoalieId.Value, match.AwayTeamId, match.CompetitionId,
                awayResult, matchDurationMinutes, awayGoalieShutout, cancellationToken);
        }
    }

    private async Task UndoSingleGoalieGamePlayed(
        Guid goalieId, Guid teamId, Guid seasonId,
        FloorballGameResult result, int minutesPlayed, bool wasShutout,
        CancellationToken cancellationToken)
    {
        FloorballGoalieSeasonStatistics? goalieStats =
            await _statisticsRepository.GetGoalieSeasonStatisticsAsync(goalieId, teamId, seasonId, cancellationToken);

        if (goalieStats == null) return;

        goalieStats.UndoGamePlayed(wasStarter: true, result, minutesPlayed, wasShutout);
        await _statisticsRepository.SaveGoalieSeasonStatisticsAsync(goalieStats, cancellationToken);
    }

    private static HashSet<(Guid PlayerId, Guid TeamId)> CollectMatchParticipants(FloorballMatch match)
    {
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

        return participants;
    }
}
