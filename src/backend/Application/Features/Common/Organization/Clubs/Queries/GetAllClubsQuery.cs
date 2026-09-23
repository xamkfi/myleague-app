using MediatR;
using Application.Features.Common.Organization.Users.DTOs;
using Application.Features.Common.Organization.Persons.DTOs;
using Application.Features.Common.Organization.Clubs.DTOs;
using Application.Features.Common.Organization.Divisions.DTOs;
using Application.Features.Common.Content.News.DTOs;
using Application.Features.Common.CrossCutting.Search.DTOs;
using Application.Features.Common.CrossCutting.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Common;
using Domain.Common;

namespace Application.Features.Common.Organization.Clubs.Queries;

/// <summary>
/// Query for retrieving clubs with pagination support
/// </summary>
public record GetAllClubsQuery(
    int Page = 1,
    int PageSize = 0 // 0 means use default from configuration
) : IRequest<Result<PagedResult<ClubDto>>>
{
    /// <summary>
    /// Resource key for pagination configuration
    /// </summary>
    public const string ResourceKey = "Clubs";
} 
