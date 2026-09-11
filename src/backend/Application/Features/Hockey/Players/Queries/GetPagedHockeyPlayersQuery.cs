using Application.Common;
using Application.Features.Hockey.Players.DTOs;
using Domain.Common;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Teams;
using MediatR;

namespace Application.Features.Hockey.Players.Queries;

/// <summary>
/// Paginated hockey player list for admin screens.
/// </summary>
public record GetPagedHockeyPlayersQuery(
    int Page = 1,
    int PageSize = 0,
    string? SearchTerm = null,
    bool? IsActive = null,
    HockeyPosition? Position = null,
    Guid? ClubId = null,
    Guid? TeamId = null,
    TeamCategory? TeamCategory = null) : IRequest<Result<PagedResult<HockeyPlayerDto>>>
{
    public const string ResourceKey = "HockeyPlayers";
}
