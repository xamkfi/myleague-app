using System.Linq.Expressions;
using Domain.Common;
using Domain.Enums.Common;
using Microsoft.EntityFrameworkCore;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common;

/// <summary>
/// Summed totals for one player, produced by a SQL GROUP BY in each sport's statistics repository.
/// </summary>
internal sealed class AllTimeTotalsRow
{
    public Guid PlayerId { get; init; }
    public Guid PersonId { get; init; }
    public int GamesPlayed { get; init; }
    public int Goals { get; init; }
    public int Assists { get; init; }
    public int Points { get; init; }
    public int PenaltyMinutes { get; init; }
    public int YellowCards { get; init; }
    public int RedCards { get; init; }
}

/// <summary>
/// One competition row used to pick a player's latest team name.
/// </summary>
internal sealed class AllTimeTeamRow
{
    public Guid PlayerId { get; init; }
    public string TeamName { get; init; } = string.Empty;
    public DateTime CompetitionStart { get; init; }
    public int GamesPlayed { get; init; }
}

/// <summary>
/// Ranks summed all-time player totals in SQL and returns one page.
/// Ties break by points, goals, assists, then player id.
/// </summary>
internal static class AllTimePlayerStatisticsQuery
{
    public static async Task<PagedResult<AllTimePlayerTotals>> PageAsync(
        IQueryable<AllTimeTotalsRow> totals,
        IQueryable<AllTimeTeamRow> teamRows,
        AllTimePlayerPageRequest request,
        CancellationToken cancellationToken)
    {
        IQueryable<AllTimeTotalsRow> withGames = totals.Where(row => row.GamesPlayed > 0);
        IQueryable<AllTimeTotalsRow> ordered = Order(withGames, request.Sort, request.Direction);
        int skip = (request.Page - 1) * request.PageSize;

        List<(AllTimeTotalsRow Row, int Rank)> pageRows;
        int totalCount;
        if (request.PersonIds is { } personIds)
        {
            HashSet<Guid> wanted = personIds.ToHashSet();
            List<(AllTimeTotalsRow Row, int Rank)> matches = (await ordered.ToListAsync(cancellationToken))
                .Select((row, index) => (row, index + 1))
                .Where(entry => wanted.Contains(entry.row.PersonId))
                .ToList();
            totalCount = matches.Count;
            pageRows = matches.Skip(skip).Take(request.PageSize).ToList();
        }
        else
        {
            totalCount = await withGames.CountAsync(cancellationToken);
            pageRows = (await ordered.Skip(skip).Take(request.PageSize).ToListAsync(cancellationToken))
                .Select((row, index) => (row, skip + index + 1))
                .ToList();
        }

        Dictionary<Guid, string> teamNames = await LatestTeamNamesAsync(
            teamRows,
            pageRows.Select(entry => entry.Row.PlayerId).ToList(),
            cancellationToken);

        List<AllTimePlayerTotals> items = pageRows
            .Select(entry => new AllTimePlayerTotals(
                entry.Rank,
                entry.Row.PlayerId,
                entry.Row.PersonId,
                teamNames.GetValueOrDefault(entry.Row.PlayerId, string.Empty),
                entry.Row.GamesPlayed,
                entry.Row.Goals,
                entry.Row.Assists,
                entry.Row.Points,
                entry.Row.PenaltyMinutes,
                entry.Row.YellowCards,
                entry.Row.RedCards))
            .ToList();

        return PagedResult.Create(items, totalCount, request.Page, request.PageSize);
    }

    private static IQueryable<AllTimeTotalsRow> Order(
        IQueryable<AllTimeTotalsRow> totals,
        AllTimeStatSort sort,
        AllTimeSortDirection direction)
    {
        Expression<Func<AllTimeTotalsRow, int>> primary = sort switch
        {
            AllTimeStatSort.Games => row => row.GamesPlayed,
            AllTimeStatSort.Goals => row => row.Goals,
            AllTimeStatSort.Assists => row => row.Assists,
            AllTimeStatSort.Points => row => row.Points,
            AllTimeStatSort.Penalties => row => row.PenaltyMinutes,
            AllTimeStatSort.YellowCards => row => row.YellowCards,
            AllTimeStatSort.RedCards => row => row.RedCards,
            _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, "Unsupported all-time sort column."),
        };

        IOrderedQueryable<AllTimeTotalsRow> ordered = direction == AllTimeSortDirection.Desc
            ? totals.OrderByDescending(primary)
            : totals.OrderBy(primary);

        return ordered
            .ThenByDescending(row => row.Points)
            .ThenByDescending(row => row.Goals)
            .ThenByDescending(row => row.Assists)
            .ThenBy(row => row.PlayerId);
    }

    private static async Task<Dictionary<Guid, string>> LatestTeamNamesAsync(
        IQueryable<AllTimeTeamRow> teamRows,
        IReadOnlyCollection<Guid> playerIds,
        CancellationToken cancellationToken)
    {
        if (playerIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        List<AllTimeTeamRow> rows = await teamRows
            .Where(row => playerIds.Contains(row.PlayerId))
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.PlayerId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(row => row.CompetitionStart)
                    .ThenByDescending(row => row.GamesPlayed)
                    .ThenBy(row => row.TeamName, StringComparer.Ordinal)
                    .First()
                    .TeamName);
    }
}
