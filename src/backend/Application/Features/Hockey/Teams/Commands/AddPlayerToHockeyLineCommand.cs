using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Hockey.Teams;
using MediatR;

namespace Application.Features.Hockey.Teams.Commands;

/// <summary>
/// Command to place a team player onto a hockey line.
/// </summary>
public record AddPlayerToHockeyLineCommand(
    Guid TeamId,
    Guid LineId,
    Guid TeamPlayerId,
    HockeyLineSlot Slot,
    int Order) : IRequest<Result<HockeyTeamDto>>;
