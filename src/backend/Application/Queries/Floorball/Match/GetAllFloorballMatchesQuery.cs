using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Domain.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Queries.Floorball.Match
{
    /// <summary>
    /// Query for retrieving floorball matches with pagination and filtering support
    /// </summary>
    public record GetAllFloorballMatchesQuery(
        int Page = 1,
        int PageSize = 0, // 0 means use default from configuration
        Guid? SeasonId = null,
        Guid? TeamId = null,
        DateTime? StartDate = null,
        DateTime? EndDate = null,
        string SortOrder = "desc" // "asc" or "desc"
    ) : IRequest<Result<PagedResult<FloorballMatchDto>>>
    {
        /// <summary>
        /// Resource key for pagination configuration
        /// </summary>
        public const string ResourceKey = "FloorballMatches";
    }
}
