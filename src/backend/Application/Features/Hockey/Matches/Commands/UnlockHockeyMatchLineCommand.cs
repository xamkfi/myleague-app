using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Unlocks a match line for edits.
/// </summary>
public record UnlockHockeyMatchLineCommand(
    Guid MatchId,
    Guid MatchTeamId,
    Guid MatchLineId) : IRequest<Result<HockeyMatchDto>>;
