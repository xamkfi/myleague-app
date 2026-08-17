using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Disables on-ice tracking for a match side.
/// </summary>
public record DisableHockeyMatchOnIceTrackingCommand(
    Guid MatchId,
    Guid MatchTeamId,
    Guid? UserId = null) : IRequest<Result<HockeyMatchDto>>;
