using Application.Features.Common.Shared.DTOs;
using Domain.Common;
using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;

namespace Application.Features.Common.Statistics;

/// <summary>
/// Summed all-time totals for one player profile.
/// </summary>
public sealed record AllTimePlayerAggregate(
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

/// <summary>
/// Which sort columns each sport's all-time list accepts.
/// </summary>
public static class AllTimeStatSortRules
{
    /// <summary>
    /// Floorball and hockey sort by games, goals, assists, points, or penalty minutes.
    /// </summary>
    public static bool IsSkaterSort(AllTimeStatSort sort) =>
        sort is AllTimeStatSort.Games
            or AllTimeStatSort.Goals
            or AllTimeStatSort.Assists
            or AllTimeStatSort.Points
            or AllTimeStatSort.Penalties;

    /// <summary>
    /// Football sort by games, goals, assists, points, yellow cards, or red cards.
    /// </summary>
    public static bool IsFootballSort(AllTimeStatSort sort) =>
        sort is AllTimeStatSort.Games
            or AllTimeStatSort.Goals
            or AllTimeStatSort.Assists
            or AllTimeStatSort.Points
            or AllTimeStatSort.YellowCards
            or AllTimeStatSort.RedCards;
}

/// <summary>
/// Sums competition statistics rows into one all-time row per player.
/// </summary>
public static class AllTimePlayerStatistics
{
    /// <summary>
    /// Drops loan profiles and players with no games, then sums the remaining rows.
    /// The team name is the team from the latest competition, then the one with more games.
    /// </summary>
    public static List<AllTimePlayerAggregate> Aggregate(IEnumerable<AllTimePlayerStatRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        return rows
            .Where(row => !row.IsLoanProfile)
            .GroupBy(row => row.PlayerId)
            .Select(group =>
            {
                AllTimePlayerStatRow latest = group
                    .OrderByDescending(row => row.CompetitionStart)
                    .ThenByDescending(row => row.GamesPlayed)
                    .ThenBy(row => row.TeamName, StringComparer.Ordinal)
                    .First();

                return new AllTimePlayerAggregate(
                    group.Key,
                    latest.PersonId,
                    latest.TeamName,
                    group.Sum(row => row.GamesPlayed),
                    group.Sum(row => row.Goals),
                    group.Sum(row => row.Assists),
                    group.Sum(row => row.Points),
                    group.Sum(row => row.PenaltyMinutes),
                    group.Sum(row => row.YellowCards),
                    group.Sum(row => row.RedCards));
            })
            .Where(player => player.GamesPlayed > 0)
            .ToList();
    }

    /// <summary>
    /// Orders aggregated players by the requested column. Ties break by points, goals, assists, then player id.
    /// </summary>
    public static List<AllTimePlayerAggregate> Rank(
        IEnumerable<AllTimePlayerStatRow> rows,
        AllTimeStatSort sort,
        AllTimeSortDirection direction)
    {
        List<AllTimePlayerAggregate> players = Aggregate(rows);
        bool descending = direction == AllTimeSortDirection.Desc;

        IOrderedEnumerable<AllTimePlayerAggregate> ordered = descending
            ? players.OrderByDescending(player => PrimaryValue(player, sort))
            : players.OrderBy(player => PrimaryValue(player, sort));

        return ordered
            .ThenByDescending(player => player.Points)
            .ThenByDescending(player => player.Goals)
            .ThenByDescending(player => player.Assists)
            .ThenBy(player => player.PlayerId)
            .ToList();
    }

    /// <summary>
    /// Lists the teams that have at least one non-loan player with games, named by their latest competition row.
    /// </summary>
    public static List<AllTimeTeamOptionDto> Teams(IEnumerable<AllTimePlayerStatRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        return rows
            .Where(row => !row.IsLoanProfile && row.GamesPlayed > 0)
            .GroupBy(row => row.TeamId)
            .Select(group => new AllTimeTeamOptionDto(
                group.Key,
                group.OrderByDescending(row => row.CompetitionStart).First().TeamName))
            .OrderBy(team => team.TeamName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static int PrimaryValue(AllTimePlayerAggregate player, AllTimeStatSort sort) => sort switch
    {
        AllTimeStatSort.Games => player.GamesPlayed,
        AllTimeStatSort.Goals => player.Goals,
        AllTimeStatSort.Assists => player.Assists,
        AllTimeStatSort.Points => player.Points,
        AllTimeStatSort.Penalties => player.PenaltyMinutes,
        AllTimeStatSort.YellowCards => player.YellowCards,
        AllTimeStatSort.RedCards => player.RedCards,
        _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, "Unsupported all-time sort column.")
    };
}

/// <summary>
/// Paging, ordering, and filter options for one all-time list request.
/// When <paramref name="TeamId"/> is set, only that team's rows are summed and ranked.
/// <paramref name="Search"/> filters by player name after ranking, so matches keep their rank.
/// </summary>
public sealed record AllTimePageRequest(
    int Page,
    int PageSize,
    AllTimeStatSort Sort,
    AllTimeSortDirection Direction,
    string? Search,
    Guid? TeamId);

/// <summary>
/// Pages a ranked all-time list and fills player names from the person catalogue.
/// </summary>
public static class AllTimePlayerStatisticsPager
{
    /// <summary>
    /// Ranks the source rows, optionally filters by player name, takes one page, and maps each row
    /// with the resolved person name and its rank before the name filter.
    /// </summary>
    public static async Task<PagedResult<TDto>> PageAsync<TDto>(
        IReadOnlyList<AllTimePlayerStatRow> rows,
        AllTimePageRequest request,
        IPersonRepository personRepository,
        Func<AllTimePlayerAggregate, string, int, TDto> map)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(personRepository);
        ArgumentNullException.ThrowIfNull(map);

        int page = request.Page;
        int pageSize = request.PageSize;
        IEnumerable<AllTimePlayerStatRow> scoped = request.TeamId is Guid teamId
            ? rows.Where(row => row.TeamId == teamId)
            : rows;

        List<RankedPlayer> ranked = AllTimePlayerStatistics.Rank(scoped, request.Sort, request.Direction)
            .Select((player, index) => new RankedPlayer(player, index + 1))
            .ToList();
        string? term = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();

        List<RankedPlayer> matches;
        Dictionary<Guid, string> names;
        if (term is null)
        {
            matches = ranked;
            names = await ResolveNamesAsync(
                personRepository,
                ranked.Skip((page - 1) * pageSize).Take(pageSize));
        }
        else
        {
            names = await ResolveNamesAsync(personRepository, ranked);
            matches = ranked
                .Where(entry => names.TryGetValue(entry.Player.PersonId, out string? fullName)
                    && fullName.Contains(term, StringComparison.CurrentCultureIgnoreCase))
                .ToList();
        }

        List<TDto> dtos = matches
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(entry =>
            {
                string playerName = names.TryGetValue(entry.Player.PersonId, out string? fullName)
                    ? fullName
                    : string.Empty;
                return map(entry.Player, playerName, entry.Rank);
            })
            .ToList();

        return PagedResult.Create(dtos, matches.Count, page, pageSize);
    }

    private static async Task<Dictionary<Guid, string>> ResolveNamesAsync(
        IPersonRepository personRepository,
        IEnumerable<RankedPlayer> players)
    {
        IEnumerable<Person> persons = await personRepository.GetByIdsAsync(
            players.Select(entry => entry.Player.PersonId).Distinct());
        return persons.ToDictionary(person => person.Id, person => person.FullName);
    }

    private sealed record RankedPlayer(AllTimePlayerAggregate Player, int Rank);
}
