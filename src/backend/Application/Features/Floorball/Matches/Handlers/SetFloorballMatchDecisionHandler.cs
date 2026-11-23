using Application.Common;
using Application.Constants;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Mappings;
using Application.Interfaces.Common;
using Domain.Entities.Floorball.Matches;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for correcting the phase of play that decided a floorball match
/// </summary>
public class SetFloorballMatchDecisionHandler : IRequestHandler<SetFloorballMatchDecisionCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly INotificationSenderService _notificationSenderService;

    public SetFloorballMatchDecisionHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballUnitOfWork unitOfWork,
        INotificationSenderService notificationSenderService)
    {
        _matchRepository = matchRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _notificationSenderService = notificationSenderService;
    }

    public async Task<Result<FloorballMatchDto>> Handle(SetFloorballMatchDecisionCommand request, CancellationToken cancellationToken)
    {
        FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match == null)
            return Result<FloorballMatchDto>.NotFound("FloorballMatch", request.MatchId);

        FloorballMovedGoal? moved;
        try
        {
            moved = match.SetDecidedIn(request.Decision);
        }
        catch (InvalidOperationException ex)
        {
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }

        if (moved != null && moved.PreviousGoalieId != moved.Goal.GoalieId)
            await MoveConcededGoalAsync(match, moved, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _notificationSenderService.SendNotificationAsync(
            FloorballNotificationEvents.GoalScored,
            new MatchNotificationPayload(match.Id));

        return Result<FloorballMatchDto>.Success(FloorballMatchMapper.ToDto(match));
    }

    private async Task MoveConcededGoalAsync(FloorballMatch match, FloorballMovedGoal moved, CancellationToken cancellationToken)
    {
        Guid defendingTeamId = FloorballMatchStatisticsSupport.OpposingTeamId(match, moved.Goal.TeamId);

        if (moved.PreviousGoalieId.HasValue)
        {
            await FloorballMatchStatisticsSupport.AdjustGoalieAsync(
                _statisticsRepository, moved.PreviousGoalieId.Value, defendingTeamId, match.CompetitionId,
                saves: 0, goalsAgainst: -1, cancellationToken);
        }

        if (moved.Goal.GoalieId.HasValue)
        {
            await FloorballMatchStatisticsSupport.AdjustGoalieAsync(
                _statisticsRepository, moved.Goal.GoalieId.Value, defendingTeamId, match.CompetitionId,
                saves: 0, goalsAgainst: 1, cancellationToken);
        }
    }
}
