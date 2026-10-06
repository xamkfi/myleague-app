using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Common;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Queries;

/// <summary>
/// Gets hockey matches for a competition (season or tournament).
/// </summary>
public record GetHockeyMatchesByCompetitionQuery(Guid CompetitionId, bool IncludeDrafts = false)
    : IRequest<Result<IEnumerable<HockeyMatchDto>>>;

/// <summary>
/// Gets hockey matches involving a career team (home or away).
/// </summary>
public record GetHockeyMatchesByTeamQuery(Guid TeamId)
    : IRequest<Result<IEnumerable<HockeyMatchDto>>>;

/// <summary>
/// Gets scheduled matches from <paramref name="From"/> onwards for any of the given teams, earliest first.
/// Draft competitions are excluded.
/// </summary>
public record GetHockeyUpcomingMatchesForTeamsQuery(IReadOnlyCollection<Guid> TeamIds, DateTime From)
    : IRequest<Result<IEnumerable<HockeyMatchDto>>>;

/// <summary>
/// Gets matches that are live, start within the next two hours, or finished in the last
/// ten minutes, optionally for one competition. Meant for short-interval polling.
/// </summary>
public record GetHockeyLiveMatchesQuery(Guid? CompetitionId = null, bool IncludeDrafts = false)
    : IRequest<Result<IEnumerable<HockeyLiveMatchDto>>>;

/// <summary>
/// Gets the latest matches a career player was dressed for, newest first.
/// </summary>
public record GetHockeyPlayerRecentMatchesQuery(Guid PlayerId, int Limit = 50)
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
    string? SearchQuery = null,
    bool IncludeDrafts = false) : IRequest<Result<PagedResult<HockeyMatchDto>>>
{
    public const string ResourceKey = "HockeyMatches";
}

/// <summary>
/// Anonymous paginated hockey match list for public calendar and schedule views.
/// </summary>
public record GetHockeyMatchesQuery(
    int Page = 1,
    int PageSize = 0,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    TeamCategory? TeamCategory = null,
    string SortOrder = "desc",
    bool IncludeDrafts = false,
    IReadOnlyList<HockeyMatchStatus>? Statuses = null,
    Guid? CompetitionId = null,
    bool ActiveSeasonsOnly = false) : IRequest<Result<PagedResult<HockeyMatchListDto>>>
{
    public const string ResourceKey = "HockeyMatches";
}
