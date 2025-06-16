using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Domain.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Queries.Floorball.Player
{
    /// <summary>
    /// Query for retrieving floorball players with pagination and comprehensive filtering support
    /// </summary>
    public record GetAllFloorballPlayersQuery(
        int Page = 1,
        int PageSize = 0, // 0 means use default from configuration
        bool? IsActive = null, // null = all, true = active only, false = inactive only
        string? Position = null,
        Guid? TeamId = null,
        string? SearchTerm = null
    ) : IRequest<Result<PagedResult<FloorballPlayerDto>>>
    {
        /// <summary>
        /// Resource key for pagination configuration
        /// </summary>
        public const string ResourceKey = "FloorballPlayers";
    }
}
