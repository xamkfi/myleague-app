using Application.Common;
using Application.Constants;
using Application.Features.Common.MatchTimer.Services;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Application.Interfaces.Common;
using Application.Services.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Statistics;
using Domain.Entities.Football.Teams;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using Domain.ValueObjects.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class CompleteFootballMatchHandler : IRequestHandler<CompleteFootballMatchCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly IFootballTournamentRepository _tournamentRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly IMatchTimerService _timerService;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly ILogger<CompleteFootballMatchHandler> _logger;

    public CompleteFootballMatchHandler(
        IFootballMatchRepository matchRepository,
        IFootballStatisticsRepository statisticsRepository,
        IFootballTournamentRepository tournamentRepository,
        IFootballTeamRepository teamRepository,
        IFootballUnitOfWork unitOfWork,
        IMatchTimerService timerService,
        INotificationSenderService notificationSenderService,
        ILogger<CompleteFootballMatchHandler> logger)
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

    public async Task<Result<FootballMatchDto>> Handle(CompleteFootballMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.Id);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.Id);
                return Result<FootballMatchDto>.Failure($"Match with ID {request.Id} not found.");
            }

            _logger.LogInformation("Completing football match: {MatchId}", request.Id);
            match.Complete();

            await UpdateFinalTeamSeasonStatistics(match, cancellationToken);
            await UpdatePlayerGamesPlayed(match, cancellationToken);

            if (match.PlayoffRound.HasValue)
            {
                await AdvancePlayoffWinnerAsync(match, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                _logger.LogInformation("Destroying timer for completed match: {MatchId}", request.Id);
                await _timerService.DestroyTimerAsync(request.Id);
            }
            catch (Exception timerEx)
            {
                _logger.LogWarning(timerEx, "Failed to destroy timer for completed match: {MatchId}. This is non-critical.", request.Id);
            }

            await _notificationSenderService.SendNotificationAsync(
                FootballNotificationEvents.MatchCompleted,
                new MatchNotificationPayload(match.Id));

            FootballMatchDto matchDto = FootballMatchMapper.ToDto(match);
            _logger.LogInformation("Successfully completed football match: {MatchId}", request.Id);

            return Result<FootballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing football match: {MatchId}", request.Id);
            return Result<FootballMatchDto>.Failure("An error occurred while completing the match.");
        }
    }

    private async Task UpdateFinalTeamSeasonStatistics(FootballMatch match, CancellationToken cancellationToken)
    {
        if (match.HomeTeamId.HasValue)
        {
            await UpdateTeamMatchResult(
                match.HomeTeamId.Value,
                match,
                match.HomeScore,
                match.AwayScore,
                isHomeGame: true,
                cancellationToken);
        }

        if (match.AwayTeamId.HasValue)
        {
            await UpdateTeamMatchResult(
                match.AwayTeamId.Value,
                match,
                match.AwayScore,
                match.HomeScore,
                isHomeGame: false,
                cancellationToken);
        }
    }

    private async Task UpdateTeamMatchResult(
        Guid teamId,
        FootballMatch match,
        int teamScore,
        int opponentScore,
        bool isHomeGame,
        CancellationToken cancellationToken)
    {
        FootballTeamSeasonStatistics? teamStats =
            await _statisticsRepository.GetTeamSeasonStatisticsAsync(teamId, match.CompetitionId, cancellationToken);
        teamStats ??= new FootballTeamSeasonStatistics(teamId, match.CompetitionId);

        FootballGameResult gameResult = teamScore > opponentScore
            ? FootballGameResult.Win
            : teamScore < opponentScore
                ? FootballGameResult.Loss
                : FootballGameResult.Draw;

        int yellowCards = match.CardEvents.Count(c =>
            c.TeamId == teamId && c.CardType == FootballCardType.Yellow);
        int redCards = match.CardEvents.Count(c =>
            c.TeamId == teamId && c.ResultsInSendingOff);

        FootballStandingRules standingRules = match.Competition?.StandingRules ?? FootballStandingRules.Default();
        teamStats.UpdateAfterMatch(
            gameResult,
            isHomeGame,
            teamScore,
            opponentScore,
            standingRules,
            yellowCards,
            redCards);

        await _statisticsRepository.SaveTeamSeasonStatisticsAsync(teamStats, cancellationToken);
    }

    private async Task UpdatePlayerGamesPlayed(FootballMatch match, CancellationToken cancellationToken)
    {
        HashSet<(Guid PlayerId, Guid TeamId)> participants = CollectMatchParticipants(match);

        _logger.LogInformation(
            "[CompleteMatch] Updating GamesPlayed for {Count} players. MatchId={MatchId}",
            participants.Count,
            match.Id);

        foreach ((Guid playerId, Guid teamId) in participants)
        {
            FootballPlayerSeasonStatistics? playerStats =
                await _statisticsRepository.GetPlayerSeasonStatisticsAsync(
                    playerId,
                    teamId,
                    match.CompetitionId,
                    cancellationToken);

            playerStats ??= new FootballPlayerSeasonStatistics(playerId, teamId, match.CompetitionId);
            playerStats.RecordGamePlayed();
            await _statisticsRepository.SavePlayerSeasonStatisticsAsync(playerStats, cancellationToken);
        }
    }

    internal static HashSet<(Guid PlayerId, Guid TeamId)> CollectMatchParticipants(FootballMatch match)
    {
        HashSet<(Guid PlayerId, Guid TeamId)> participants = new();

        foreach (FootballMatchLineupPlayer lineupPlayer in match.Lineup)
        {
            participants.Add((lineupPlayer.PlayerId, lineupPlayer.TeamId));
        }

        foreach (FootballMatchEvent evt in match.Events)
        {
            switch (evt)
            {
                case FootballGoal goal:
                    if (goal.ScoringPlayerId.HasValue)
                    {
                        participants.Add((goal.ScoringPlayerId.Value, goal.TeamId));
                    }

                    if (goal.AssistingPlayerId.HasValue)
                    {
                        participants.Add((goal.AssistingPlayerId.Value, goal.TeamId));
                    }

                    break;

                case FootballCard card:
                    participants.Add((card.PlayerId, card.TeamId));
                    break;

                case FootballSubstitution substitution:
                    participants.Add((substitution.PlayerOffId, substitution.TeamId));
                    participants.Add((substitution.PlayerOnId, substitution.TeamId));
                    break;
            }
        }

        return participants;
    }

    private async Task AdvancePlayoffWinnerAsync(FootballMatch completed, CancellationToken cancellationToken)
    {
        Guid? winnerTeamId = completed.HomeScore > completed.AwayScore
            ? completed.HomeTeamId
            : completed.AwayScore > completed.HomeScore
                ? completed.AwayTeamId
                : null;

        if (!winnerTeamId.HasValue)
        {
            _logger.LogWarning(
                "Playoff match {MatchId} ended in a draw. No winner advanced. Round={Round}",
                completed.Id,
                completed.PlayoffRound);
            return;
        }

        Guid loserTeamId = (winnerTeamId.Value == completed.HomeTeamId ? completed.AwayTeamId : completed.HomeTeamId)!.Value;
        List<FootballMatch> tournamentMatches = (await _matchRepository.GetByCompetitionIdAsync(completed.CompetitionId)).ToList();

        if (completed.NextMatchId.HasValue && completed.NextMatchSlot.HasValue)
        {
            FootballMatch? nextMatch = await _matchRepository.GetByIdAsync(completed.NextMatchId.Value);
            if (nextMatch != null && nextMatch.Status == FootballMatchStatus.Scheduled)
            {
                FootballTeam? winnerTeam = await _teamRepository.GetByIdAsync(winnerTeamId.Value);
                if (winnerTeam != null)
                {
                    nextMatch.AssignPlayoffTeam(completed.NextMatchSlot.Value, winnerTeam);
                    await _matchRepository.UpdateAsync(nextMatch);
                }
            }
        }

        if (completed.PlayoffRound == FootballPlayoffRound.SemiFinal)
        {
            FootballMatch? thirdPlace = tournamentMatches.FirstOrDefault(m => m.PlayoffRound == FootballPlayoffRound.ThirdPlaceMatch);
            if (thirdPlace != null && thirdPlace.Status == FootballMatchStatus.Scheduled)
            {
                FootballTeam? loserTeam = await _teamRepository.GetByIdAsync(loserTeamId);
                if (loserTeam != null)
                {
                    FootballPlayoffSlot slot = completed.PlayoffMatchOrder == 0
                        ? FootballPlayoffSlot.Home
                        : FootballPlayoffSlot.Away;
                    thirdPlace.AssignPlayoffTeam(slot, loserTeam);
                    await _matchRepository.UpdateAsync(thirdPlace);
                }
            }
        }

        if (completed.PlayoffRound == FootballPlayoffRound.Final)
        {
            FootballTournament? tournament = await _tournamentRepository.GetByIdAsync(completed.CompetitionId, cancellationToken);
            if (tournament != null)
            {
                tournament.SetChampion(winnerTeamId.Value);

                bool anyOtherUnfinished = tournamentMatches.Any(m =>
                    m.Id != completed.Id &&
                    m.Status != FootballMatchStatus.Completed &&
                    m.Status != FootballMatchStatus.Cancelled);

                if (!anyOtherUnfinished && tournament.TournamentStatus != FootballTournamentStatus.Completed)
                {
                    tournament.CompleteTournament();
                }
                else if (anyOtherUnfinished)
                {
                    _logger.LogInformation(
                        "Final completed for tournament {TournamentId}, but other matches remain. Tournament left in {Status} for manual completion.",
                        completed.CompetitionId,
                        tournament.TournamentStatus);
                }
            }
        }
    }
}
