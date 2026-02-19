using Application.Commands.Floorball.Match;
using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Floorball.Matches;

/// <summary>
/// Handler for deleting a penalty event from a floorball match (non-event-sourced)
/// </summary>
public class DeletePenaltyHandler : IRequestHandler<DeletePenaltyCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<DeletePenaltyHandler> _logger;

    public DeletePenaltyHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballPlayerRepository playerRepository,
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<DeletePenaltyHandler> logger)
    {
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(DeletePenaltyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            _logger.LogInformation("Deleting penalty {PenaltyId} from match {MatchId}", request.PenaltyEventId, request.MatchId);

            // Delete the penalty event and get the deleted penalty
            FloorballPenalty deletedPenalty = match.DeletePenaltyEvent(request.PenaltyEventId);

            // Decrement player statistics
            if (deletedPenalty.PlayerId.HasValue)
            {
                FloorballPlayer? player = await _playerRepository.GetByIdAsync(deletedPenalty.PlayerId.Value);
                if (player != null)
                {
                    // Remove penalty minutes from the player's team player record
                    // This would require finding the correct FloorballTeamPlayer entity
                    // For now, we'll focus on season statistics
                }
            }

            // Decrement season statistics for penalty minutes
            if (deletedPenalty.PlayerId.HasValue)
            {
                await RemovePlayerSeasonPenaltyStatistics(deletedPenalty.PlayerId.Value, deletedPenalty.TeamId, match.SeasonId, deletedPenalty.DurationInMinutes, cancellationToken);
            }

            // Decrement match team statistics for penalty minutes
            await RemoveMatchTeamPenaltyStatistics(match.Id, deletedPenalty.TeamId, deletedPenalty.DurationInMinutes, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting penalty {PenaltyId} from match {MatchId}", request.PenaltyEventId, request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while deleting the penalty.");
        }
    }

    /// <summary>
    /// Removes player season statistics for penalty minutes
    /// </summary>
    private async Task RemovePlayerSeasonPenaltyStatistics(Guid playerId, Guid teamId, Guid seasonId, int penaltyMinutes, CancellationToken cancellationToken)
    {
        FloorballPlayerSeasonStatistics? playerStats = await _statisticsRepository.GetPlayerSeasonStatisticsAsync(playerId, teamId, seasonId, cancellationToken);
        if (playerStats != null)
        {
            playerStats.RemovePenaltyMinutes(penaltyMinutes);
            await _statisticsRepository.SavePlayerSeasonStatisticsAsync(playerStats, cancellationToken);
        }
    }

    /// <summary>
    /// Removes match team statistics for penalty minutes
    /// </summary>
    private async Task RemoveMatchTeamPenaltyStatistics(Guid matchId, Guid teamId, int penaltyMinutes, CancellationToken cancellationToken)
    {
        FloorballMatchTeamStatistics? matchStats = await _statisticsRepository.GetMatchTeamStatisticsAsync(matchId, teamId, cancellationToken);
        if (matchStats != null)
        {
            matchStats.RemovePenaltyMinutes(penaltyMinutes);
            await _statisticsRepository.SaveMatchTeamStatisticsAsync(matchStats, cancellationToken);
        }
    }
}


