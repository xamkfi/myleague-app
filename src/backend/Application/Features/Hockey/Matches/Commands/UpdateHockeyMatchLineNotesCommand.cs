using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Updates match line notes.
/// </summary>
public record UpdateHockeyMatchLineNotesCommand(
    Guid MatchId,
    Guid MatchTeamId,
    Guid MatchLineId,
    string? Notes) : IRequest<Result<HockeyMatchDto>>;
