using Application.Common;
using Application.Features.Football.Teams.DTOs;
using MediatR;

namespace Application.Features.Football.Teams.Commands;

/// <summary>
/// Command for updating only the jersey number of a player in a football team roster.
/// Position and active status are preserved. Used by team leaders.
/// </summary>
/// <param name="TeamId">The team ID</param>
/// <param name="PlayerId">The player ID</param>
/// <param name="JerseyNumber">The new jersey number, or null to clear it</param>
public record UpdateTeamPlayerJerseyNumberCommand(
    Guid TeamId,
    Guid PlayerId,
    int? JerseyNumber) : IRequest<Result<FootballTeamPlayerDto>>;
