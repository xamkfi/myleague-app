using System;
using Application.Common;
using Domain.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Floorball.Teams.Queries
{
    /// <summary>
    /// Query for retrieving floorball teams without roster with pagination, search, and filtering support
    /// </summary>
    public record GetAllTeamsWithoutRosterQuery(
        int Page = 1,
        int PageSize = 0, // 0 means use default from configuration
        string? SearchTerm = null,
        TeamCategory? TeamCategory = null
    ) : IRequest<Result<PagedResult<FloorballTeamSummaryDto>>>
    {
        /// <summary>
        /// Resource key for pagination configuration
        /// </summary>
        public const string ResourceKey = "FloorballTeams";
    }
}

