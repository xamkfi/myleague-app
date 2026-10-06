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
/// Helpers over competition statistics rows for all-time lists.
/// </summary>
public static class AllTimePlayerStatistics
{
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
/// Loads one ranked all-time page from the database and fills player names from the person catalogue.
/// </summary>
public static class AllTimePlayerStatisticsPager
{
    /// <summary>
    /// Resolves the name search to person ids, loads the summed and ranked page, and maps each row
    /// with the resolved person name and its rank before the name filter.
    /// </summary>
    public static async Task<PagedResult<TDto>> PageAsync<TDto>(
        Func<AllTimePlayerPageRequest, CancellationToken, Task<PagedResult<AllTimePlayerTotals>>> loadPage,
        AllTimePageRequest request,
        IPersonRepository personRepository,
        Func<AllTimePlayerAggregate, string, int, TDto> map,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(loadPage);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(personRepository);
        ArgumentNullException.ThrowIfNull(map);

        string? term = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();
        IReadOnlyCollection<Guid>? personIds = term is null
            ? null
            : (await personRepository.GetIdsByNameContainsAsync(term, cancellationToken)).ToList();

        PagedResult<AllTimePlayerTotals> page = await loadPage(
            new AllTimePlayerPageRequest(
                request.Page,
                request.PageSize,
                request.Sort,
                request.Direction,
                request.TeamId,
                personIds),
            cancellationToken);

        Dictionary<Guid, string> names = await ResolveNamesAsync(personRepository, page.Items);

        List<TDto> dtos = page.Items
            .Select(player =>
            {
                string playerName = names.TryGetValue(player.PersonId, out string? fullName)
                    ? fullName
                    : string.Empty;
                AllTimePlayerAggregate aggregate = new(
                    player.PlayerId,
                    player.PersonId,
                    player.TeamName,
                    player.GamesPlayed,
                    player.Goals,
                    player.Assists,
                    player.Points,
                    player.PenaltyMinutes,
                    player.YellowCards,
                    player.RedCards);
                return map(aggregate, playerName, player.Rank);
            })
            .ToList();

        return PagedResult.Create(dtos, page.TotalCount, request.Page, request.PageSize);
    }

    private static async Task<Dictionary<Guid, string>> ResolveNamesAsync(
        IPersonRepository personRepository,
        IEnumerable<AllTimePlayerTotals> players)
    {
        List<Guid> personIds = players.Select(player => player.PersonId).Distinct().ToList();
        if (personIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        IEnumerable<Person> persons = await personRepository.GetByIdsAsync(personIds);
        return persons.ToDictionary(person => person.Id, person => person.FullName);
    }
}
