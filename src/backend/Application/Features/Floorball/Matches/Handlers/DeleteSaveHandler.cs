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
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for deleting a save event from a match
/// </summary>
public class DeleteSaveHandler : IRequestHandler<DeleteSaveCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteSaveHandler> _logger;

    public DeleteSaveHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballPlayerRepository playerRepository,
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<DeleteSaveHandler> logger)
    {
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(DeleteSaveCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            _logger.LogInformation("Deleting save {SaveId} from match {MatchId}", request.SaveEventId, request.MatchId);

            // Delete the save event and get the deleted save
            FloorballSave deletedSave = match.DeleteSaveEvent(request.SaveEventId);

            // Update goalie season statistics: decrement saves and shots against
            await RemoveGoalieSeasonStatistics(deletedSave.GoalieId, deletedSave.TeamId, match.CompetitionId, 1, 1, cancellationToken);

            // Update match team statistics: decrement shots against
            await RemoveMatchTeamStatistics(match.Id, deletedSave.TeamId, 1, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting save event with ID: {SaveEventId}", request.SaveEventId);
            return Result<FloorballMatchDto>.Failure("An error occurred while deleting the save event.");
        }
    }

    private async Task RemoveGoalieSeasonStatistics(Guid goalieId, Guid teamId, Guid seasonId, int saves, int shotsAgainst, CancellationToken cancellationToken)
    {
        FloorballGoalieSeasonStatistics? goalieStats = await _statisticsRepository.GetGoalieSeasonStatisticsAsync(goalieId, teamId, seasonId, cancellationToken);
        if (goalieStats != null)
        {
            goalieStats.RecordSaves(-saves, -shotsAgainst, 0);
            await _statisticsRepository.SaveGoalieSeasonStatisticsAsync(goalieStats, cancellationToken);
        }
    }

    private async Task RemoveMatchTeamStatistics(Guid matchId, Guid teamId, int shotsAgainst, CancellationToken cancellationToken)
    {
        FloorballMatchTeamStatistics? matchStats = await _statisticsRepository.GetMatchTeamStatisticsAsync(matchId, teamId, cancellationToken);
        if (matchStats != null)
        {
            // Use the dedicated remover to avoid passing negative values
            matchStats.RemoveShotStatistics(0, shotsAgainst);
            await _statisticsRepository.SaveMatchTeamStatisticsAsync(matchStats, cancellationToken);
        }
    }
}


