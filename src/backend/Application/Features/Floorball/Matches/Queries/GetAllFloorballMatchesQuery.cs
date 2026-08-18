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
using Domain.Enums.Floorball;

namespace Application.Features.Floorball.Matches.Queries
{
    /// <summary>
    /// Query for retrieving floorball matches with pagination and filtering support
    /// </summary>
    public record GetAllFloorballMatchesQuery(
        int Page = 1,
        int PageSize = 0, // 0 means use default from configuration
        Guid? CompetitionId = null,
        Guid? TeamId = null,
        DateTime? StartDate = null,
        DateTime? EndDate = null,
        string SortOrder = "desc", // "asc" or "desc"
        string? SearchQuery = null, // Search by team names (case-insensitive, partial match)
        FloorballMatchStatus? Status = null,
        Guid? TournamentGroupId = null,
        FloorballCompetitionType? CompetitionType = null,
        Domain.Enums.Common.TeamCategory? TeamCategory = null
    ) : IRequest<Result<PagedResult<FloorballMatchDto>>>
    {
        /// <summary>
        /// Resource key for pagination configuration
        /// </summary>
        public const string ResourceKey = "FloorballMatches";
    }
}
