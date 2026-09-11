using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Floorball.Matches.Queries
{
    /// <summary>
    /// Query for retrieving floorball matches by team
    /// </summary>
    /// <param name="TeamId"></param>
    public record GetFloorballMatchesByTeamQuery(
        int Page = 1,
        int PageSize = 0,
        Guid? TeamId = null,
        DateTime? StartDate = null,
        DateTime? EndDate = null
        ) : IRequest<Result<PagedResult<FloorballMatchDto>>>
    {
        /// <summary>
        /// Resource key for pagination configuration
        /// </summary>
        public const string ResourceKey = "FloorballMatches";
    }
}
