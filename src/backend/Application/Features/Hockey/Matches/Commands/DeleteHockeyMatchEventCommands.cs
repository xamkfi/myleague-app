using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Deletes a goal event from a hockey match (live-ops undo).
/// </summary>
public record DeleteHockeyGoalCommand(Guid MatchId, Guid GoalEventId)
    : IRequest<Result<HockeyMatchDto>>;

/// <summary>
/// Deletes a penalty event from a hockey match (live-ops undo).
/// </summary>
public record DeleteHockeyPenaltyCommand(Guid MatchId, Guid PenaltyEventId)
    : IRequest<Result<HockeyMatchDto>>;

/// <summary>
/// Deletes a shot event from a hockey match (live-ops undo).
/// </summary>
public record DeleteHockeyShotCommand(Guid MatchId, Guid ShotEventId)
    : IRequest<Result<HockeyMatchDto>>;
