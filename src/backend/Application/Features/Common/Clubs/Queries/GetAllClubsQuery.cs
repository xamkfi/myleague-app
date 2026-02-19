using MediatR;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Common;
using Domain.Common;

namespace Application.Features.Common.Clubs.Queries;

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
