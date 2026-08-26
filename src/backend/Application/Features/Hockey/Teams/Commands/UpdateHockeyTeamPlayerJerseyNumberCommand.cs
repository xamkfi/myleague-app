using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using MediatR;

namespace Application.Features.Hockey.Teams.Commands;

/// <summary>
/// Command for updating only the jersey number of a player in a hockey team roster.
/// Position, roster status, and captain role are preserved. Used by club admins.
/// </summary>
/// <param name="TeamId">The team ID</param>
/// <param name="PlayerId">The hockey player ID</param>
/// <param name="JerseyNumber">The new jersey number, or null to clear it</param>
public record UpdateHockeyTeamPlayerJerseyNumberCommand(
    Guid TeamId,
    Guid PlayerId,
    int? JerseyNumber) : IRequest<Result<HockeyTeamPlayerDto>>;
