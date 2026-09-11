using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Removes a player from the ice.
/// </summary>
public record RemoveHockeyMatchPlayerFromIceCommand(
    Guid MatchId,
    Guid MatchTeamId,
    Guid MatchActivePlayerId,
    int? PeriodNumber = null,
    int? TimeInSeconds = null,
    Guid? UserId = null) : IRequest<Result<HockeyMatchDto>>;
