using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Removes a player from a match line.
/// </summary>
public record RemoveHockeyMatchLinePlayerCommand(
    Guid MatchId,
    Guid MatchTeamId,
    Guid MatchLineId,
    Guid MatchActivePlayerId) : IRequest<Result<HockeyMatchDto>>;
