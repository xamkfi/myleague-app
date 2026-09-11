using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Teams;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Adds a dressed player to a match line.
/// </summary>
public record AddHockeyMatchLinePlayerCommand(
    Guid MatchId,
    Guid MatchTeamId,
    Guid MatchLineId,
    Guid MatchActivePlayerId,
    HockeyLineSlot? Slot = null,
    int? Order = null) : IRequest<Result<HockeyMatchDto>>;
