using System;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Queries.Floorball.Player
{
    /// <summary>
    /// Query for retrieving a floorball player's match history with performance statistics
    /// </summary>
    /// <param name="PlayerId">The ID of the player</param>
    /// <param name="Limit">Maximum number of recent matches to return (default: 10)</param>
    public record GetFloorballPlayerMatchesQuery(
        Guid PlayerId,
        int Limit = 10
    ) : IRequest<Result<FloorballPlayerWithMatchesDto>>;
} 