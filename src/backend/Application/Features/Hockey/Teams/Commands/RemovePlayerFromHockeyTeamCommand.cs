using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using MediatR;

namespace Application.Features.Hockey.Teams.Commands;

/// <summary>
/// Command to remove a player from a hockey team roster.
/// </summary>
public record RemovePlayerFromHockeyTeamCommand(
    Guid TeamId,
    Guid PlayerId,
    Guid? CompetitionId = null) : IRequest<Result<HockeyTeamDto>>;
