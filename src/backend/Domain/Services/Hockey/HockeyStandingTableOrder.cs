using Domain.Enums.Hockey.Competitions;
using Domain.ValueObjects.Hockey.Rules;

namespace Domain.Services.Hockey;

/// <summary>
/// Values compared when ordering a hockey table.
/// </summary>
public readonly record struct HockeyStandingSortSnapshot(
    int Points,
    int RegulationWins,
    int Wins,
    int GoalDifference,
    int GoalsFor,
    int GoalsAgainst,
    int PenaltyMinutes,
    string TeamName);

/// <summary>
/// Orders hockey standings by the competition's tie-breaker list, then by team name.
/// Head-to-head and manual decisions do not change the order.
/// </summary>
public static class HockeyStandingTableOrder
{
    public static List<T> Sort<T>(
        IEnumerable<T> rows,
        IReadOnlyList<HockeyTieBreakerRule> criteria,
        Func<T, HockeyStandingSortSnapshot> snapshot)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(snapshot);

        IReadOnlyList<HockeyTieBreakerRule> effective = criteria is null || criteria.Count == 0
            ? HockeyStandingRules.Default().TieBreakers
            : criteria;

        return rows
            .OrderBy(snapshot, new HockeySnapshotComparer(effective))
            .ToList();
    }

    private sealed class HockeySnapshotComparer : IComparer<HockeyStandingSortSnapshot>
    {
        private readonly IReadOnlyList<HockeyTieBreakerRule> _criteria;

        public HockeySnapshotComparer(IReadOnlyList<HockeyTieBreakerRule> criteria)
        {
            _criteria = criteria;
        }

        public int Compare(HockeyStandingSortSnapshot x, HockeyStandingSortSnapshot y)
        {
            foreach (int comparison in _criteria.Select(criterion => criterion switch
            {
                HockeyTieBreakerRule.Points => y.Points.CompareTo(x.Points),
                HockeyTieBreakerRule.RegulationWins => y.RegulationWins.CompareTo(x.RegulationWins),
                HockeyTieBreakerRule.Wins => y.Wins.CompareTo(x.Wins),
                HockeyTieBreakerRule.GoalDifference => y.GoalDifference.CompareTo(x.GoalDifference),
                HockeyTieBreakerRule.GoalsFor => y.GoalsFor.CompareTo(x.GoalsFor),
                HockeyTieBreakerRule.GoalsAgainst => x.GoalsAgainst.CompareTo(y.GoalsAgainst),
                HockeyTieBreakerRule.FewestPenaltyMinutes => x.PenaltyMinutes.CompareTo(y.PenaltyMinutes),
                _ => 0
            }))
            {
                if (comparison != 0)
                    return comparison;
            }

            return string.Compare(x.TeamName, y.TeamName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
