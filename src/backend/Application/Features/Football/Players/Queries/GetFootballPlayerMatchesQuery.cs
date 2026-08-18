using System;
using Application.Common;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using MediatR;

namespace Application.Features.Football.Players.Queries
{
    /// <summary>
    /// Query for retrieving a football player's match history with performance statistics
    /// </summary>
    /// <param name="PlayerId">The ID of the player</param>
    /// <param name="Limit">Maximum number of recent matches to return (default: 10)</param>
    public record GetFootballPlayerMatchesQuery(
        Guid PlayerId,
        int Limit = 10
    ) : IRequest<Result<FootballPlayerWithMatchesDto>>;
} 
