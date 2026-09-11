using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Removes a match line from one match side.
/// </summary>
public record RemoveHockeyMatchLineCommand(
    Guid MatchId,
    Guid MatchTeamId,
    Guid MatchLineId) : IRequest<Result<HockeyMatchDto>>;
