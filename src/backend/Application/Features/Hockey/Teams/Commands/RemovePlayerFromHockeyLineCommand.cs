using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using MediatR;

namespace Application.Features.Hockey.Teams.Commands;

/// <summary>
/// Command to remove a team player from a hockey line.
/// </summary>
public record RemovePlayerFromHockeyLineCommand(
    Guid TeamId,
    Guid LineId,
    Guid TeamPlayerId) : IRequest<Result<HockeyTeamDto>>;
