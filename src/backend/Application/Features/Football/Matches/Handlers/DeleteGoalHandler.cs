using Application.Common;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Statistics;
using Domain.Entities.Football.Teams;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class DeleteGoalHandler : IRequestHandler<DeleteGoalCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteGoalHandler> _logger;

    public DeleteGoalHandler(
        IFootballMatchRepository matchRepository,
        IFootballPlayerRepository playerRepository,
        IFootballStatisticsRepository statisticsRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<DeleteGoalHandler> logger)
    {
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(DeleteGoalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FootballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            _logger.LogInformation("Deleting goal {GoalId} from match {MatchId}", request.GoalEventId, request.MatchId);

            FootballGoal deletedGoal = match.DeleteGoalEvent(request.GoalEventId);
            bool isOwnGoal = deletedGoal.IsOwnGoal || deletedGoal.GoalType == FootballGoalType.OwnGoal;

            if (deletedGoal.ScoringPlayerId.HasValue && !isOwnGoal)
            {
                FootballPlayer? scoringPlayer = await _playerRepository.GetByIdAsync(deletedGoal.ScoringPlayerId.Value);
                scoringPlayer?.RemoveGoal();

                await RemovePlayerSeasonStatistic(
                    deletedGoal.ScoringPlayerId.Value,
                    deletedGoal.TeamId,
                    match.CompetitionId,
                    isGoal: true,
                    isAssist: false,
                    cancellationToken);
            }

            if (deletedGoal.AssistingPlayerId.HasValue)
            {
                FootballPlayer? assistingPlayer = await _playerRepository.GetByIdAsync(deletedGoal.AssistingPlayerId.Value);
                assistingPlayer?.RemoveAssist();

                await RemovePlayerSeasonStatistic(
                    deletedGoal.AssistingPlayerId.Value,
                    deletedGoal.TeamId,
                    match.CompetitionId,
                    isGoal: false,
                    isAssist: true,
                    cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FootballMatchDto>.Success(FootballMatchMapper.ToDto(match));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting goal {GoalId} from match {MatchId}", request.GoalEventId, request.MatchId);
            return Result<FootballMatchDto>.Failure(ex.Message);
        }
    }

    private async Task RemovePlayerSeasonStatistic(
        Guid playerId,
        Guid teamId,
        Guid competitionId,
        bool isGoal,
        bool isAssist,
        CancellationToken cancellationToken)
    {
        FootballPlayerSeasonStatistics? playerStats =
            await _statisticsRepository.GetPlayerSeasonStatisticsAsync(playerId, teamId, competitionId, cancellationToken);
        if (playerStats == null)
        {
            return;
        }

        if (isGoal)
        {
            playerStats.RemoveGoal();
        }

        if (isAssist)
        {
            playerStats.RemoveAssist();
        }

        await _statisticsRepository.SavePlayerSeasonStatisticsAsync(playerStats, cancellationToken);
    }
}
