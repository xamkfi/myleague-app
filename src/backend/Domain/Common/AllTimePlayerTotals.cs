using Domain.Enums.Common;

namespace Domain.Common;

/// <summary>
/// Paging, ordering, and filter options for an all-time player page summed in the database.
/// When <paramref name="TeamId"/> is set, only that team's rows are summed and ranked.
/// When <paramref name="PersonIds"/> is set, only those people are returned, but each keeps
/// its rank from the unfiltered list.
/// </summary>
public sealed record AllTimePlayerPageRequest(
    int Page,
    int PageSize,
    AllTimeStatSort Sort,
    AllTimeSortDirection Direction,
    Guid? TeamId,
    IReadOnlyCollection<Guid>? PersonIds);

/// <summary>
/// Summed all-time totals for one player profile with its rank in the requested ordering.
/// The team name is the team from the latest competition, then the one with more games.
/// </summary>
public sealed record AllTimePlayerTotals(
    int Rank,
    Guid PlayerId,
    Guid PersonId,
    string TeamName,
    int GamesPlayed,
    int Goals,
    int Assists,
    int Points,
    int PenaltyMinutes,
    int YellowCards,
    int RedCards);
