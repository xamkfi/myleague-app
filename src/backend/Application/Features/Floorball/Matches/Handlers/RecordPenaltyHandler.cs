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
using Application.Interfaces.Common;
using Application.Constants;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for recording a penalty in a floorball match
/// </summary>
public class RecordPenaltyHandler : IRequestHandler<RecordPenaltyCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly ILogger<RecordPenaltyHandler> _logger;

    public RecordPenaltyHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballPlayerRepository playerRepository,
        IFloorballStatisticsRepository statisticsRepository,
        INotificationSenderService notificationSenderService,
        IFloorballUnitOfWork unitOfWork,

        ILogger<RecordPenaltyHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _notificationSenderService = notificationSenderService;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(RecordPenaltyCommand request, CancellationToken cancellationToken)
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

            FloorballPlayer? player = null;
            if (request.PlayerId.HasValue)
            {
                player = await _playerRepository.GetByIdAsync(request.PlayerId.Value);
                if (player == null)
                {
                    _logger.LogWarning("Player not found with ID: {PlayerId}", request.PlayerId);
                    return Result<FloorballMatchDto>.Failure($"Player with ID {request.PlayerId} not found.");
                }
            }

            _logger.LogInformation("Recording penalty in match {MatchId}", request.MatchId);

            FloorballPenalty penaltyEvent = match.RecordPenalty(
                team,
                player!,
                request.PenaltyType,
                request.Minutes,
                request.PeriodNumber,
                request.TimeInSeconds,
                request.Description);

            // Update player season statistics for penalty minutes
            if (player != null)
            {
                await UpdatePlayerSeasonPenaltyStatistics(player.Id, request.TeamId, match.SeasonId, request.Minutes, cancellationToken);
            }

            // Update match team statistics for penalty minutes
            await UpdateMatchTeamPenaltyStatistics(match.Id, request.TeamId, request.Minutes, cancellationToken);

            _matchRepository.MarkEventAsAdded(penaltyEvent);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationSenderService.SendNotificationAsync(
                FloorballNotificationEvents.PenaltyAssigned,
                new MatchNotificationPayload(match.Id));

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording penalty in match {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while recording the penalty.");
        }
    }

    /// <summary>
    /// Updates player season statistics for penalty minutes
    /// </summary>
    private async Task UpdatePlayerSeasonPenaltyStatistics(Guid playerId, Guid teamId, Guid seasonId, int penaltyMinutes, CancellationToken cancellationToken)
    {
        FloorballPlayerSeasonStatistics? playerStats = await _statisticsRepository.GetPlayerSeasonStatisticsAsync(playerId, teamId, seasonId, cancellationToken);
        if (playerStats == null)
        {
            playerStats = new FloorballPlayerSeasonStatistics(playerId, teamId, seasonId);
        }

        playerStats.RecordPenaltyMinutes(penaltyMinutes);

        await _statisticsRepository.SavePlayerSeasonStatisticsAsync(playerStats, cancellationToken);
    }

    /// <summary>
    /// Updates match team statistics for penalty minutes
    /// </summary>
    private async Task UpdateMatchTeamPenaltyStatistics(Guid matchId, Guid teamId, int penaltyMinutes, CancellationToken cancellationToken)
    {
        FloorballMatchTeamStatistics? matchStats = await _statisticsRepository.GetMatchTeamStatisticsAsync(matchId, teamId, cancellationToken);
        if (matchStats == null)
        {
            matchStats = new FloorballMatchTeamStatistics(matchId, teamId);
        }

        matchStats.UpdatePenaltyMinutes(penaltyMinutes);

        await _statisticsRepository.SaveMatchTeamStatisticsAsync(matchStats, cancellationToken);
    }
}


