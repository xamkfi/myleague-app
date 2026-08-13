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

namespace Application.Features.Floorball.Teams.Queries
{
    /// <summary>
    /// Query for retrieving floorball teams with pagination and filtering support
    /// </summary>
    public record GetAllFloorballTeamsQuery(
        int Page = 1,
        int PageSize = 0, // 0 means use default from configuration
        Guid? ClubId = null,
        string? Division = null,
        IReadOnlyCollection<Domain.Enums.Common.TeamCategory>? TeamCategories = null
    ) : IRequest<Result<PagedResult<FloorballTeamDto>>>
    {
        /// <summary>
        /// Resource key for pagination configuration
        /// </summary>
        public const string ResourceKey = "FloorballTeams";
    }
}
