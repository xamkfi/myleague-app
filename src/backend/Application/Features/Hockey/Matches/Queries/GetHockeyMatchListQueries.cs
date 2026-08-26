using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Common;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Queries;

/// <summary>
/// Gets hockey matches for a competition (season or tournament).
/// </summary>
public record GetHockeyMatchesByCompetitionQuery(Guid CompetitionId)
    : IRequest<Result<IEnumerable<HockeyMatchDto>>>;

/// <summary>
/// Gets hockey matches involving a career team (home or away).
/// </summary>
public record GetHockeyMatchesByTeamQuery(Guid TeamId)
    : IRequest<Result<IEnumerable<HockeyMatchDto>>>;

/// <summary>
/// Paginated hockey match list for admin screens. Does not load events, lines, or on-ice state.
/// </summary>
public record GetPagedHockeyMatchesQuery(
    int Page = 1,
    int PageSize = 0,
    Guid? CompetitionId = null,
    Guid? TeamId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    HockeyMatchStatus? Status = null,
    string SortOrder = "desc",
    string? SearchQuery = null) : IRequest<Result<PagedResult<HockeyMatchDto>>>
{
    public const string ResourceKey = "HockeyMatches";
}
