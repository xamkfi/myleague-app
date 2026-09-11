using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Teams;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Adds a match-specific line to one match side.
/// </summary>
public record AddHockeyMatchLineCommand(
    Guid MatchId,
    Guid MatchTeamId,
    string Name,
    HockeyLineType LineType,
    int? LineNumber = null,
    string? Notes = null) : IRequest<Result<HockeyMatchDto>>;
