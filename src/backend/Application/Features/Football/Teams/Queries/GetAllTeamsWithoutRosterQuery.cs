using System;
using Application.Common;
using Domain.Common;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Football.Teams.Queries
{
    /// <summary>
    /// Query for retrieving football teams without roster with pagination, search, and filtering support
    /// </summary>
    public record GetAllTeamsWithoutRosterQuery(
        int Page = 1,
        int PageSize = 0, // 0 means use default from configuration
        string? SearchTerm = null,
        TeamCategory? TeamCategory = null
    ) : IRequest<Result<PagedResult<FootballTeamSummaryDto>>>
    {
        /// <summary>
        /// Resource key for pagination configuration
        /// </summary>
        public const string ResourceKey = "FootballTeams";
    }
}

