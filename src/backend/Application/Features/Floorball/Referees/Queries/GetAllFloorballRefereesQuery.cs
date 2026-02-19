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
using MediatR;

namespace Application.Features.Floorball.Referees.Queries
{
    /// <summary>
    /// Query for retrieving floorball referees with pagination and filtering support
    /// </summary>
    public record GetAllFloorballRefereesQuery(
        int Page = 1,
        int PageSize = 0, // 0 means use default from configuration
        bool? IsActive = null, // null = all, true = active only, false = inactive only
        string? SearchTerm = null,
        int? LicenseExpiringWithinDays = null // Filter for referees with license expiring within specified days
    ) : IRequest<Result<PagedResult<FloorballRefereeDto>>>
    {
        /// <summary>
        /// Resource key for pagination configuration
        /// </summary>
        public const string ResourceKey = "FloorballReferees";
    }
} 
