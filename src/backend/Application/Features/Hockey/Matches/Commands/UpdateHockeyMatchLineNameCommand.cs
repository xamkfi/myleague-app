using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Updates a match line name.
/// </summary>
public record UpdateHockeyMatchLineNameCommand(
    Guid MatchId,
    Guid MatchTeamId,
    Guid MatchLineId,
    string Name) : IRequest<Result<HockeyMatchDto>>;
