using System;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using MediatR;

namespace Application.Features.Floorball.Players.Queries
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
