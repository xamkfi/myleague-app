using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Locks a match line against edits.
/// </summary>
public record LockHockeyMatchLineCommand(
    Guid MatchId,
    Guid MatchTeamId,
    Guid MatchLineId) : IRequest<Result<HockeyMatchDto>>;
