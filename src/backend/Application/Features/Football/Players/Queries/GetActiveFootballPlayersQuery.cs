using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Domain.Common;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using MediatR;

namespace Application.Features.Football.Players.Queries
{
    /// <summary>
    /// Query for retrieving active football players with pagination support
    /// </summary>
    public record GetActiveFootballPlayersQuery(
        int Page = 1,
        int PageSize = 0, // 0 means use default from configuration
        string? Position = null,
        Guid? TeamId = null
    ) : IRequest<Result<PagedResult<FootballPlayerDto>>>
    {
        /// <summary>
        /// Resource key for pagination configuration
        /// </summary>
        public const string ResourceKey = "FootballPlayers";
    }
}
