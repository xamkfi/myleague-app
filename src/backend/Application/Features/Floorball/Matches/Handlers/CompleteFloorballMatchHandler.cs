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
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly IMatchTimerService _timerService;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly ILogger<CompleteFloorballMatchHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CompleteFloorballMatchHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="statisticsRepository">The statistics repository</param>
    /// <param name="tournamentRepository">The tournament repository (used for playoff advancement)</param>
    /// <param name="teamRepository">The team repository (used to resolve playoff winner/loser entities)</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="timerService">The timer service</param>
    /// <param name="logger">The logger</param>
    public CompleteFloorballMatchHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballTournamentRepository tournamentRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballUnitOfWork unitOfWork,
        IMatchTimerService timerService,
        INotificationSenderService notificationSenderService,
        ILogger<CompleteFloorballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _statisticsRepository = statisticsRepository;
        _tournamentRepository = tournamentRepository;
        _teamRepository = teamRepository;
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

            // Update GamesPlayed for active goalies on their goalie season statistics
            await UpdateGoalieGamesPlayed(match, cancellationToken);

            // If this is a playoff match, advance the winner to the next match's correct slot.
            // For the final, also mark the tournament Completed and record the champion.
            if (match.PlayoffRound.HasValue)
            {
                await AdvancePlayoffWinnerAsync(match, cancellationToken);
            }

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
                new MatchNotificationPayload(match.Id));

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            _logger.LogInformation("Successfully completed floorball match: {MatchId}", request.Id);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error occurred while completing floorball match: {MatchId}", request.Id);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Error occurred while completing floorball match: {MatchId}", request.Id);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Updates final team season statistics with match results (wins/losses/ties)
    /// </summary>
    private async Task UpdateFinalTeamSeasonStatistics(FloorballMatch match, CancellationToken cancellationToken)
    {
        // A completed match has gone through Start() which guarantees both team IDs are populated;
        // .Value is therefore safe here. Skip statistics gracefully if for some reason a slot is
        // still null (e.g. a manual data edit) rather than throwing.
        if (match.HomeTeamId.HasValue)
            await UpdateTeamMatchResult(match.HomeTeamId.Value, match.CompetitionId, match.HomeScore, match.AwayScore, true, match, cancellationToken);

        if (match.AwayTeamId.HasValue)
            await UpdateTeamMatchResult(match.AwayTeamId.Value, match.CompetitionId, match.AwayScore, match.HomeScore, false, match, cancellationToken);
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

        // Goals are already tracked incrementally via RecordGoalHandler (IncrementGoalsFor/IncrementGoalsAgainst),
        // so pass 0 here to avoid double-counting.
        teamStats.UpdateAfterMatch(
            gameResult: gameResult,
            isHomeGame: isHomeGame,
            goalsFor: 0,
            goalsAgainst: 0);

        await _statisticsRepository.SaveTeamSeasonStatisticsAsync(teamStats, cancellationToken);
    }

    /// <summary>
    /// Collects all unique players who participated in the match — anyone on the team's active
    /// lineup, the active goalies, and any player that produced an event (defensively, in case
    /// the active roster was not maintained). Every participant gets their GamesPlayed counter
    /// incremented (and a season statistics row created when one does not yet exist), so even
    /// players who did not score or take a penalty still appear on their team / career stats.
    /// </summary>
    private async Task UpdatePlayerGamesPlayed(FloorballMatch match, CancellationToken cancellationToken)
    {
        HashSet<(Guid PlayerId, Guid TeamId)> participants = CollectMatchParticipants(match);

        _logger.LogInformation("[CompleteMatch] Updating GamesPlayed for {Count} players. MatchId={MatchId}",
            participants.Count, match.Id);

        foreach ((Guid playerId, Guid teamId) in participants)
        {
            FloorballPlayerSeasonStatistics? playerStats =
                await _statisticsRepository.GetPlayerSeasonStatisticsAsync(playerId, teamId, match.CompetitionId, cancellationToken);

            if (playerStats == null)
            {
                playerStats = new FloorballPlayerSeasonStatistics(playerId, teamId, match.CompetitionId);
            }

            playerStats.RecordGamePlayed();
            await _statisticsRepository.SavePlayerSeasonStatisticsAsync(playerStats, cancellationToken);
        }
    }

    /// <summary>
    /// Builds the set of unique (player, team) tuples that took part in <paramref name="match"/>:
    /// every entry on the active field lineup, both active goalies, and (as a safety net) every
    /// player referenced by goal / penalty / save events. The two sides need an identical view of
    /// who counts as a participant — <see cref="ReopenFloorballMatchHandler"/> mirrors this logic
    /// when reversing the per-match aggregates.
    /// </summary>
    internal static HashSet<(Guid PlayerId, Guid TeamId)> CollectMatchParticipants(FloorballMatch match)
    {
        HashSet<(Guid PlayerId, Guid TeamId)> participants = new();

        foreach (FloorballMatchActivePlayer active in match.ActivePlayers)
        {
            participants.Add((active.PlayerId, active.TeamId));
        }

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

        // An active goalie can only have been set after the team was assigned, so the team ID
        // must be non-null when the goalie ID is present.
        if (match.HomeActiveGoalieId.HasValue && match.HomeTeamId.HasValue)
            participants.Add((match.HomeActiveGoalieId.Value, match.HomeTeamId.Value));
        if (match.AwayActiveGoalieId.HasValue && match.AwayTeamId.HasValue)
            participants.Add((match.AwayActiveGoalieId.Value, match.AwayTeamId.Value));

        return participants;
    }

    /// <summary>
    /// Updates goalie-specific season statistics (GamesPlayed, wins/losses) for active goalies
    /// </summary>
    private async Task UpdateGoalieGamesPlayed(FloorballMatch match, CancellationToken cancellationToken)
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

        if (match.HomeActiveGoalieId.HasValue && match.HomeTeamId.HasValue)
        {
            await UpdateSingleGoalieGamePlayed(
                match.HomeActiveGoalieId.Value, match.HomeTeamId.Value, match.CompetitionId,
                homeResult, matchDurationMinutes, homeGoalieShutout, cancellationToken);
        }

        if (match.AwayActiveGoalieId.HasValue && match.AwayTeamId.HasValue)
        {
            await UpdateSingleGoalieGamePlayed(
                match.AwayActiveGoalieId.Value, match.AwayTeamId.Value, match.CompetitionId,
                awayResult, matchDurationMinutes, awayGoalieShutout, cancellationToken);
        }
    }

    /// <summary>
    /// Advances the winner of a playoff match into the next bracket slot, and updates the tournament
    /// when the final is completed. Loser propagation into the optional 3rd-place match is handled
    /// here for semifinals as well.
    /// </summary>
    private async Task AdvancePlayoffWinnerAsync(FloorballMatch completed, CancellationToken cancellationToken)
    {
        // Determine winner. Domain guard in FloorballMatch.Complete() forbids playoff draws, but we
        // keep the defensive null check here in case the match arrived via a different completion path.
        Guid? winnerTeamId = completed.HomeScore > completed.AwayScore
            ? completed.HomeTeamId
            : completed.AwayScore > completed.HomeScore
                ? completed.AwayTeamId
                : (Guid?)null;
        if (!winnerTeamId.HasValue)
        {
            _logger.LogWarning(
                "Playoff match {MatchId} ended in a draw. No winner advanced. Round={Round}",
                completed.Id, completed.PlayoffRound);
            return;
        }
        // Both team IDs are guaranteed non-null at this point: the match was Completed (which
        // implies Start() succeeded and both teams were assigned), and the winnerTeamId branch
        // above already verified at least one of HomeTeamId/AwayTeamId matched the score.
        Guid loserTeamId = (winnerTeamId.Value == completed.HomeTeamId ? completed.AwayTeamId : completed.HomeTeamId)!.Value;

        // Pull all tournament matches once so both the SF→3rd-place propagation and the final
        // auto-complete check can share the lookup.
        List<FloorballMatch> tournamentMatches = (await _matchRepository.GetByCompetitionIdAsync(completed.CompetitionId)).ToList();

        // Advance winner forward.
        if (completed.NextMatchId.HasValue && completed.NextMatchSlot.HasValue)
        {
            FloorballMatch? nextMatch = await _matchRepository.GetByIdAsync(completed.NextMatchId.Value);
            if (nextMatch != null && nextMatch.Status == FloorballMatchStatus.Scheduled)
            {
                FloorballTeam? winnerTeam = await _teamRepository.GetByIdAsync(winnerTeamId.Value);
                if (winnerTeam != null)
                {
                    nextMatch.AssignPlayoffTeam(completed.NextMatchSlot.Value, winnerTeam);
                    await _matchRepository.UpdateAsync(nextMatch);
                }
            }
        }

        // For semifinals, also propagate the loser into the 3rd-place match (if one exists).
        if (completed.PlayoffRound == FloorballPlayoffRound.SemiFinal)
        {
            FloorballMatch? thirdPlace = tournamentMatches.FirstOrDefault(m => m.PlayoffRound == FloorballPlayoffRound.ThirdPlaceMatch);
            if (thirdPlace != null && thirdPlace.Status == FloorballMatchStatus.Scheduled)
            {
                FloorballTeam? loserTeam = await _teamRepository.GetByIdAsync(loserTeamId);
                if (loserTeam != null)
                {
                    // Place SF1's loser in the 3rd-place HomeTeam slot, SF2's in the AwayTeam slot.
                    FloorballPlayoffSlot slot = completed.PlayoffMatchOrder == 0
                        ? FloorballPlayoffSlot.Home
                        : FloorballPlayoffSlot.Away;
                    thirdPlace.AssignPlayoffTeam(slot, loserTeam);
                    await _matchRepository.UpdateAsync(thirdPlace);
                }
            }
        }

        // Final completion -> always record the champion. Auto-complete the tournament only when no
        // other group/playoff match is still pending; otherwise the admin completes it manually after
        // e.g. the 3rd-place match wraps up. The `completed` match is excluded because its status is
        // updated in memory but the repository fetch above might still see the pre-Save value.
        if (completed.PlayoffRound == FloorballPlayoffRound.Final)
        {
            FloorballTournament? tournament = await _tournamentRepository.GetByIdAsync(completed.CompetitionId, cancellationToken);
            if (tournament != null)
            {
                tournament.SetChampion(winnerTeamId.Value);

                bool anyOtherUnfinished = tournamentMatches.Any(m =>
                    m.Id != completed.Id &&
                    m.Status != FloorballMatchStatus.Completed &&
                    m.Status != FloorballMatchStatus.Cancelled);

                if (!anyOtherUnfinished && tournament.TournamentStatus != FloorballTournamentStatus.Completed)
                {
                    tournament.CompleteTournament();
                }
                else if (anyOtherUnfinished)
                {
                    _logger.LogInformation(
                        "Final completed for tournament {TournamentId}, but other matches remain. Tournament left in {Status} for manual completion.",
                        completed.CompetitionId, tournament.TournamentStatus);
                }
            }
        }
    }

    private async Task UpdateSingleGoalieGamePlayed(
        Guid goalieId, Guid teamId, Guid seasonId,
        FloorballGameResult result, int minutesPlayed, bool wasShutout,
        CancellationToken cancellationToken)
    {
        FloorballGoalieSeasonStatistics? goalieStats =
            await _statisticsRepository.GetGoalieSeasonStatisticsAsync(goalieId, teamId, seasonId, cancellationToken);

        if (goalieStats == null)
        {
            goalieStats = new FloorballGoalieSeasonStatistics(goalieId, teamId, seasonId);
        }

        goalieStats.RecordGamePlayed(wasStarter: true, result, minutesPlayed, wasShutout);
        await _statisticsRepository.SaveGoalieSeasonStatisticsAsync(goalieStats, cancellationToken);
    }
} 
