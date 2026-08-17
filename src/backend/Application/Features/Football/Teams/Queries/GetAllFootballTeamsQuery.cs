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

namespace Application.Features.Football.Teams.Queries
{
    /// <summary>
    /// Query for retrieving football teams with pagination and filtering support
    /// </summary>
    public record GetAllFootballTeamsQuery(
        int Page = 1,
        int PageSize = 0, // 0 means use default from configuration
        Guid? ClubId = null,
        string? Division = null,
        IReadOnlyCollection<Domain.Enums.Common.TeamCategory>? TeamCategories = null,
        string? SearchTerm = null
    ) : IRequest<Result<PagedResult<FootballTeamDto>>>
    {
        /// <summary>
        /// Resource key for pagination configuration
        /// </summary>
        public const string ResourceKey = "FootballTeams";
    }
}
