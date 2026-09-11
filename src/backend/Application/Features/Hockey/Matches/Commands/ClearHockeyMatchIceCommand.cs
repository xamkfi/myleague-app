using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Clears all players from the ice for a match side.
/// </summary>
public record ClearHockeyMatchIceCommand(
    Guid MatchId,
    Guid MatchTeamId,
    int? PeriodNumber = null,
    int? TimeInSeconds = null,
    Guid? UserId = null) : IRequest<Result<HockeyMatchDto>>;
