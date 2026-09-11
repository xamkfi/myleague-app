using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Sets the active goalie for a match side.
/// </summary>
public record SetHockeyMatchActiveGoalieCommand(
    Guid MatchId,
    Guid MatchTeamId,
    Guid MatchActivePlayerId) : IRequest<Result<HockeyMatchDto>>;
