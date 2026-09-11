using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Deactivates a match line.
/// </summary>
public record DeactivateHockeyMatchLineCommand(
    Guid MatchId,
    Guid MatchTeamId,
    Guid MatchLineId) : IRequest<Result<HockeyMatchDto>>;
