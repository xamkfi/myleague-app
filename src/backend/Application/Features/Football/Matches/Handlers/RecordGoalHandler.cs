using Application.Common;
using Application.Constants;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Application.Interfaces.Common;
using Application.Services.Common;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Statistics;
using Domain.Entities.Football.Teams;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class RecordGoalHandler : IRequestHandler<RecordGoalCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly ILogger<RecordGoalHandler> _logger;

    public RecordGoalHandler(
        IFootballMatchRepository matchRepository,
        IFootballTeamRepository teamRepository,
        IFootballPlayerRepository playerRepository,
        IFootballStatisticsRepository statisticsRepository,
        IFootballUnitOfWork unitOfWork,
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

    public async Task<Result<FootballMatchDto>> Handle(RecordGoalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FootballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            FootballTeam? scoringTeam = await _teamRepository.GetByIdAsync(request.ScoringTeamId);
            if (scoringTeam == null)
            {
                _logger.LogWarning("Scoring team not found with ID: {TeamId}", request.ScoringTeamId);
                return Result<FootballMatchDto>.Failure($"Scoring team with ID {request.ScoringTeamId} not found.");
            }

            FootballPlayer? scoringPlayer = await _playerRepository.GetByIdAsync(request.ScoringPlayerId);
            if (scoringPlayer == null)
            {
                _logger.LogWarning("Scoring player not found with ID: {PlayerId}", request.ScoringPlayerId);
                return Result<FootballMatchDto>.Failure($"Scoring player with ID {request.ScoringPlayerId} not found.");
            }

            FootballPlayer? assistingPlayer = null;
            if (request.AssistingPlayerId.HasValue)
            {
                assistingPlayer = await _playerRepository.GetByIdAsync(request.AssistingPlayerId.Value);
                if (assistingPlayer == null)
                {
                    _logger.LogWarning("Assisting player not found with ID: {PlayerId}", request.AssistingPlayerId.Value);
                    return Result<FootballMatchDto>.Failure($"Assisting player with ID {request.AssistingPlayerId.Value} not found.");
                }
            }

            _logger.LogInformation(
                "[RecordGoal] Recording goal. MatchId={MatchId}, TeamId={TeamId}, PlayerId={PlayerId}, Period={Period}, T={Time}",
                request.MatchId,
                request.ScoringTeamId,
                request.ScoringPlayerId,
                request.PeriodNumber,
                request.TimeInSeconds);

            FootballGoal goal = match.RecordGoal(
                scoringTeam,
                scoringPlayer,
                assistingPlayer,
                request.PeriodNumber,
                request.TimeInSeconds,
                request.GoalType,
                request.Description);

            bool isOwnGoal = goal.IsOwnGoal || request.GoalType == FootballGoalType.OwnGoal;
            if (!isOwnGoal)
            {
                scoringPlayer.RecordGoal();
                await UpdatePlayerSeasonStatistics(
                    scoringPlayer.Id,
                    request.ScoringTeamId,
                    match.CompetitionId,
                    isGoal: true,
                    isAssist: false,
                    cancellationToken);
            }

            if (assistingPlayer != null && !isOwnGoal)
            {
                assistingPlayer.RecordAssist();
                await UpdatePlayerSeasonStatistics(
                    assistingPlayer.Id,
                    request.ScoringTeamId,
                    match.CompetitionId,
                    isGoal: false,
                    isAssist: true,
                    cancellationToken);
            }

            _matchRepository.MarkEventAsAdded(goal);
            await _matchRepository.UpdateAsync(match);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationSenderService.SendNotificationAsync(
                FootballNotificationEvents.GoalScored,
                new MatchNotificationPayload(match.Id));

            FootballMatchDto matchDto = FootballMatchMapper.ToDto(match);
            _logger.LogInformation(
                "Successfully recorded goal in match {MatchId} by player {PlayerId}",
                request.MatchId,
                request.ScoringPlayerId);

            return Result<FootballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording goal in match {MatchId}", request.MatchId);
            return Result<FootballMatchDto>.Failure("An error occurred while recording the goal.");
        }
    }

    private async Task UpdatePlayerSeasonStatistics(
        Guid playerId,
        Guid teamId,
        Guid seasonId,
        bool isGoal,
        bool isAssist,
        CancellationToken cancellationToken)
    {
        FootballPlayerSeasonStatistics? playerStats =
            await _statisticsRepository.GetPlayerSeasonStatisticsAsync(playerId, teamId, seasonId, cancellationToken);
        playerStats ??= new FootballPlayerSeasonStatistics(playerId, teamId, seasonId);

        if (isGoal)
        {
            playerStats.RecordGoal();
        }

        if (isAssist)
        {
            playerStats.RecordAssist();
        }

        await _statisticsRepository.SavePlayerSeasonStatisticsAsync(playerStats, cancellationToken);
    }
}
