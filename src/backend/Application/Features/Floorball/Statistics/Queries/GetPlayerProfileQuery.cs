using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Queries.Floorball.Statistics;

/// <summary>
/// Query for retrieving a player profile with career statistics
/// </summary>
/// <param name="playerId">The player ID</param>
public record GetPlayerProfileQuery(Guid playerId) : IRequest<Result<FloorballPlayerProfileDto>>;
