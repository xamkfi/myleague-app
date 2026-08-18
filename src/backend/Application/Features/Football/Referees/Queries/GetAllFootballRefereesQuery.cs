using System;
using Application.Common;
using Domain.Common;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using MediatR;

namespace Application.Features.Football.Referees.Queries
{
    /// <summary>
    /// Query for retrieving football referees with pagination and filtering support
    /// </summary>
    public record GetAllFootballRefereesQuery(
        int Page = 1,
        int PageSize = 0, // 0 means use default from configuration
        bool? IsActive = null, // null = all, true = active only, false = inactive only
        string? SearchTerm = null,
        int? LicenseExpiringWithinDays = null // Filter for referees with license expiring within specified days
    ) : IRequest<Result<PagedResult<FootballRefereeDto>>>
    {
        /// <summary>
        /// Resource key for pagination configuration
        /// </summary>
        public const string ResourceKey = "FootballReferees";
    }
} 
