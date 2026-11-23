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
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for deleting a goal event from a floorball match (non-event-sourced)
/// </summary>
public class DeleteGoalHandler : IRequestHandler<DeleteFloorballGoalCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteGoalHandler> _logger;

    public DeleteGoalHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballPlayerRepository playerRepository,
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<DeleteGoalHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(DeleteFloorballGoalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            _logger.LogInformation("Deleting goal {GoalId} from match {MatchId}", request.GoalEventId, request.MatchId);

            // Delete the goal event and get the deleted goal
            FloorballGoal deletedGoal = match.DeleteGoalEvent(request.GoalEventId);
            _logger.LogInformation("Deleted goal period {Period} team {TeamId}; current period scores: H{Home}-A{Away}",
                deletedGoal.PeriodNumber,
                deletedGoal.TeamId,
                match.PeriodScores.FirstOrDefault(ps => ps.PeriodNumber == deletedGoal.PeriodNumber)?.HomeScore,
                match.PeriodScores.FirstOrDefault(ps => ps.PeriodNumber == deletedGoal.PeriodNumber)?.AwayScore);

            await FloorballMatchStatisticsSupport.UndoGoalAsync(
                _statisticsRepository, _playerRepository, match, deletedGoal, cancellationToken);
            await FloorballMatchStatisticsSupport.SyncMatchShotsAsync(_statisticsRepository, match, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected deleting goal {GoalId} from match {MatchId}", request.GoalEventId, request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Goal {GoalId} cannot be deleted from match {MatchId}", request.GoalEventId, request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
    }
}
