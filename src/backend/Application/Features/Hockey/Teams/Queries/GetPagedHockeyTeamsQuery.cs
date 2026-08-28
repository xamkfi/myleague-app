using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Common;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Hockey.Teams.Queries;

/// <summary>
/// Paginated hockey team list for admin screens. Does not load roster, lines, or staff.
/// </summary>
public record GetPagedHockeyTeamsQuery(
    int Page = 1,
    int PageSize = 0,
    string? SearchTerm = null,
    Guid? ClubId = null,
    TeamCategory? TeamCategory = null) : IRequest<Result<PagedResult<HockeyTeamDto>>>
{
    public const string ResourceKey = "HockeyTeams";
}
