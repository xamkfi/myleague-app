using Application.Common;
using Application.Features.Floorball.Matches.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Floorball.Matches.Commands;

/// <summary>
/// Corrects the phase of play that decided a floorball match in progress.
/// </summary>
public record SetFloorballMatchDecisionCommand(
    Guid MatchId,
    MatchDecision Decision) : IRequest<Result<FloorballMatchDto>>;
