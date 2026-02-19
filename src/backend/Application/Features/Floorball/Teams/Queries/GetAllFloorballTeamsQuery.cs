using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Domain.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Queries.Floorball.Team
{
    /// <summary>
    /// Query for retrieving floorball teams with pagination and filtering support
    /// </summary>
    public record GetAllFloorballTeamsQuery(
        int Page = 1,
        int PageSize = 0, // 0 means use default from configuration
        Guid? ClubId = null,
        string? Division = null
    ) : IRequest<Result<PagedResult<FloorballTeamDto>>>
    {
        /// <summary>
        /// Resource key for pagination configuration
        /// </summary>
        public const string ResourceKey = "FloorballTeams";
    }
}
