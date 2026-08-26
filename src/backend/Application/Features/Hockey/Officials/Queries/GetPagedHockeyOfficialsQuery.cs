using Application.Common;
using Application.Features.Hockey.Officials.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Hockey.Officials.Queries;

/// <summary>
/// Paginated hockey official list for admin screens.
/// </summary>
public record GetPagedHockeyOfficialsQuery(
    int Page = 1,
    int PageSize = 0,
    bool? IsActive = null,
    string? SearchTerm = null,
    int? LicenseExpiringWithinDays = null) : IRequest<Result<PagedResult<HockeyOfficialDto>>>
{
    public const string ResourceKey = "HockeyOfficials";
}
