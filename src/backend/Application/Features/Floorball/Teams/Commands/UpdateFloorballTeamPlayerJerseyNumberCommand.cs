using Application.Common;
using Application.Features.Floorball.Teams.DTOs;
using MediatR;

namespace Application.Features.Floorball.Teams.Commands;

/// <summary>
/// Command for updating only the jersey number of a player in a floorball team roster.
/// Position and active status are preserved. Used by team leaders.
/// </summary>
/// <param name="TeamId">The team ID</param>
/// <param name="PlayerId">The player ID</param>
/// <param name="JerseyNumber">The new jersey number, or null to clear it</param>
public record UpdateFloorballTeamPlayerJerseyNumberCommand(
    Guid TeamId,
    Guid PlayerId,
    int? JerseyNumber,
    Guid? CompetitionId = null) : IRequest<Result<FloorballTeamPlayerDto>>;
