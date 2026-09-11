using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Deactivates a dressed player on a match-side roster.
/// </summary>
public record DeactivateHockeyMatchRosterPlayerCommand(
    Guid MatchId,
    Guid MatchTeamId,
    Guid MatchActivePlayerId) : IRequest<Result<HockeyMatchDto>>;
