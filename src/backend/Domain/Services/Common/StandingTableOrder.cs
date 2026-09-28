using Domain.Enums.Common;

namespace Domain.Services.Common;

/// <summary>
/// Values compared when ordering a league table.
/// </summary>
public readonly record struct StandingSortSnapshot(
    Guid TeamId,
    int Points,
    int GoalDifference,
    int GoalsFor,
    int GoalsAgainst,
    int PenaltyMinutes,
    string TeamName);

/// <summary>
/// Orders standings rows by a season's ranking criteria, then by team name.
/// Head-to-head criteria use only the matches among the teams that are still tied.
/// </summary>
public static class StandingTableOrder
{
    public static List<T> Sort<T>(
        IEnumerable<T> rows,
        IReadOnlyList<StandingSortCriterion> criteria,
        Func<T, StandingSortSnapshot> snapshot,
        IReadOnlyList<StandingMatchResult>? matches = null)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(snapshot);

        IReadOnlyList<StandingSortCriterion> effective = criteria is null || criteria.Count == 0
            ? StandingSortCriteria.Default
            : criteria;

        return OrderGroup(rows.ToList(), effective, 0, matches ?? [], snapshot);
    }

    private static List<T> OrderGroup<T>(
        List<T> rows,
        IReadOnlyList<StandingSortCriterion> criteria,
        int index,
        IReadOnlyList<StandingMatchResult> matches,
        Func<T, StandingSortSnapshot> snapshot)
    {
        if (rows.Count <= 1)
            return rows;

        if (index >= criteria.Count)
        {
            return rows
                .OrderBy(row => snapshot(row).TeamName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        StandingSortCriterion criterion = criteria[index];
        if (criterion == StandingSortCriterion.Draw)
        {
            return rows
                .OrderBy(row => snapshot(row).TeamId)
                .ToList();
        }

        Dictionary<Guid, int> keys = KeysFor(rows, criterion, matches, snapshot);
        bool fewerIsBetter = criterion is StandingSortCriterion.GoalsAgainst or StandingSortCriterion.PenaltyMinutes;
        List<T> ordered = fewerIsBetter
            ? rows.OrderBy(row => keys[snapshot(row).TeamId])
                .ThenBy(row => snapshot(row).TeamName, StringComparer.OrdinalIgnoreCase)
                .ToList()
            : rows.OrderByDescending(row => keys[snapshot(row).TeamId])
                .ThenBy(row => snapshot(row).TeamName, StringComparer.OrdinalIgnoreCase)
                .ToList();

        List<T> result = new(ordered.Count);
        int start = 0;
        while (start < ordered.Count)
        {
            int key = keys[snapshot(ordered[start]).TeamId];
            int end = start + 1;
            while (end < ordered.Count && keys[snapshot(ordered[end]).TeamId] == key)
                end++;

            List<T> tied = ordered.GetRange(start, end - start);
            if (tied.Count == 1)
                result.Add(tied[0]);
            else
                result.AddRange(OrderGroup(tied, criteria, index + 1, matches, snapshot));

            start = end;
        }

        return result;
    }

    private static Dictionary<Guid, int> KeysFor<T>(
        List<T> rows,
        StandingSortCriterion criterion,
        IReadOnlyList<StandingMatchResult> matches,
        Func<T, StandingSortSnapshot> snapshot)
    {
        if (criterion is StandingSortCriterion.HeadToHeadPoints
            or StandingSortCriterion.HeadToHeadGoalDifference
            or StandingSortCriterion.HeadToHeadGoalsFor)
        {
            return HeadToHeadKeys(rows, criterion, matches, snapshot);
        }

        Dictionary<Guid, int> keys = new();
        foreach (StandingSortSnapshot item in rows.Select(snapshot))
        {
            keys[item.TeamId] = criterion switch
            {
                StandingSortCriterion.Points => item.Points,
                StandingSortCriterion.GoalDifference => item.GoalDifference,
                StandingSortCriterion.GoalsFor => item.GoalsFor,
                StandingSortCriterion.GoalsAgainst => item.GoalsAgainst,
                StandingSortCriterion.PenaltyMinutes => item.PenaltyMinutes,
                _ => 0
            };
        }

        return keys;
    }

    private static Dictionary<Guid, int> HeadToHeadKeys<T>(
        List<T> rows,
        StandingSortCriterion criterion,
        IReadOnlyList<StandingMatchResult> matches,
        Func<T, StandingSortSnapshot> snapshot)
    {
        HashSet<Guid> tied = rows.Select(row => snapshot(row).TeamId).ToHashSet();
        Dictionary<Guid, (int Points, int GoalsFor, int GoalsAgainst)> table = tied.ToDictionary(
            id => id,
            _ => (0, 0, 0));

        foreach (StandingMatchResult match in matches.Where(match =>
            match.HomeTeamId != match.AwayTeamId
            && tied.Contains(match.HomeTeamId)
            && tied.Contains(match.AwayTeamId)))
        {
            (int Points, int GoalsFor, int GoalsAgainst) home = table[match.HomeTeamId];
            table[match.HomeTeamId] = (
                home.Points + match.HomePoints,
                home.GoalsFor + match.HomeGoals,
                home.GoalsAgainst + match.AwayGoals);

            (int Points, int GoalsFor, int GoalsAgainst) away = table[match.AwayTeamId];
            table[match.AwayTeamId] = (
                away.Points + match.AwayPoints,
                away.GoalsFor + match.AwayGoals,
                away.GoalsAgainst + match.HomeGoals);
        }

        Dictionary<Guid, int> keys = new();
        foreach ((Guid id, (int Points, int GoalsFor, int GoalsAgainst) stats) in table)
        {
            keys[id] = criterion switch
            {
                StandingSortCriterion.HeadToHeadPoints => stats.Points,
                StandingSortCriterion.HeadToHeadGoalDifference => stats.GoalsFor - stats.GoalsAgainst,
                StandingSortCriterion.HeadToHeadGoalsFor => stats.GoalsFor,
                _ => 0
            };
        }

        return keys;
    }
}
