using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Applies a match line onto the ice.
/// </summary>
public record ApplyHockeyMatchLineToIceCommand(
    Guid MatchId,
    Guid MatchTeamId,
    Guid MatchLineId,
    int? PeriodNumber = null,
    int? TimeInSeconds = null,
    Guid? UserId = null) : IRequest<Result<HockeyMatchDto>>;
