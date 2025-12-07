using MediatR;
using Application.DTOs.Common;
using Application.Common;
using Domain.Common;

namespace Application.Queries.Clubs;

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