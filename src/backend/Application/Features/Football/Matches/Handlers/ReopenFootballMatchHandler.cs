using Application.Common;
using Application.Constants;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Application.Interfaces.Common;
using Application.Services.Common;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Statistics;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using Domain.ValueObjects.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class ReopenFootballMatchHandler : IRequestHandler<ReopenFootballMatchCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly ILogger<ReopenFootballMatchHandler> _logger;

    public ReopenFootballMatchHandler(
        IFootballMatchRepository matchRepository,
        IFootballStatisticsRepository statisticsRepository,
        IFootballUnitOfWork unitOfWork,
        INotificationSenderService notificationSenderService,
        ILogger<ReopenFootballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _notificationSenderService = notificationSenderService;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(ReopenFootballMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.Id);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.Id);
                return Result<FootballMatchDto>.Failure($"Match with ID {request.Id} not found.");
            }

            if (match.PlayoffRound.HasValue)
            {
                _logger.LogWarning(
                    "Refusing to reopen playoff match {MatchId} (round={Round}). Playoff propagation rollback is not supported.",
                    request.Id,
                    match.PlayoffRound);
                return Result<FootballMatchDto>.Failure(
                    "Playoff matches cannot be reopened automatically. Please contact an administrator if a playoff result was recorded by mistake.");
            }

            _logger.LogInformation("Reopening football match: {MatchId}", request.Id);

            await UndoTeamSeasonStatistics(match, cancellationToken);
            await UndoPlayerGamesPlayed(match, cancellationToken);

            match.ReopenFromCompleted();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationSenderService.SendNotificationAsync(
                FootballNotificationEvents.MatchReopened,
                new MatchNotificationPayload(match.Id));

            FootballMatchDto matchDto = FootballMatchMapper.ToDto(match);
            _logger.LogInformation("Successfully reopened football match: {MatchId}", request.Id);

            return Result<FootballMatchDto>.Success(matchDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot reopen match {MatchId}: {Message}", request.Id, ex.Message);
            return Result<FootballMatchDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reopening football match: {MatchId}", request.Id);
            return Result<FootballMatchDto>.Failure("An error occurred while reopening the match.");
        }
    }

    private async Task UndoTeamSeasonStatistics(FootballMatch match, CancellationToken cancellationToken)
    {
        if (match.HomeTeamId.HasValue)
        {
            await UndoSingleTeamSeasonStatistics(
                match.HomeTeamId.Value,
                match,
                match.HomeScore,
                match.AwayScore,
                isHomeGame: true,
                cancellationToken);
        }

        if (match.AwayTeamId.HasValue)
        {
            await UndoSingleTeamSeasonStatistics(
                match.AwayTeamId.Value,
                match,
                match.AwayScore,
                match.HomeScore,
                isHomeGame: false,
                cancellationToken);
        }
    }

    private async Task UndoSingleTeamSeasonStatistics(
        Guid teamId,
        FootballMatch match,
        int teamScore,
        int opponentScore,
        bool isHomeGame,
        CancellationToken cancellationToken)
    {
        FootballTeamSeasonStatistics? teamStats =
            await _statisticsRepository.GetTeamSeasonStatisticsAsync(teamId, match.CompetitionId, cancellationToken);
        if (teamStats == null)
        {
            _logger.LogWarning(
                "No team season statistics found to undo for team {TeamId} / competition {CompetitionId}.",
                teamId,
                match.CompetitionId);
            return;
        }

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
        teamStats.RevertAfterMatch(
            gameResult,
            isHomeGame,
            teamScore,
            opponentScore,
            standingRules,
            yellowCards,
            redCards);

        await _statisticsRepository.SaveTeamSeasonStatisticsAsync(teamStats, cancellationToken);
    }

    private async Task UndoPlayerGamesPlayed(FootballMatch match, CancellationToken cancellationToken)
    {
        HashSet<(Guid PlayerId, Guid TeamId)> participants = CompleteFootballMatchHandler.CollectMatchParticipants(match);

        _logger.LogInformation(
            "[ReopenMatch] Decrementing GamesPlayed for {Count} players. MatchId={MatchId}",
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

            if (playerStats == null)
            {
                continue;
            }

            playerStats.RemoveGamePlayed();
            await _statisticsRepository.SavePlayerSeasonStatisticsAsync(playerStats, cancellationToken);
        }
    }
}
