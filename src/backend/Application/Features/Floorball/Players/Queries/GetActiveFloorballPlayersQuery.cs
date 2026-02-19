using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Domain.Common;
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
    /// Query for retrieving active floorball players with pagination support
    /// </summary>
    public record GetActiveFloorballPlayersQuery(
        int Page = 1,
        int PageSize = 0, // 0 means use default from configuration
        string? Position = null,
        Guid? TeamId = null
    ) : IRequest<Result<PagedResult<FloorballPlayerDto>>>
    {
        /// <summary>
        /// Resource key for pagination configuration
        /// </summary>
        public const string ResourceKey = "FloorballPlayers";
    }
}
