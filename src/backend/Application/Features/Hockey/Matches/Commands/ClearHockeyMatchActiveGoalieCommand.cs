using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Clears the active goalie for a match side.
/// </summary>
public record ClearHockeyMatchActiveGoalieCommand(
    Guid MatchId,
    Guid MatchTeamId) : IRequest<Result<HockeyMatchDto>>;
