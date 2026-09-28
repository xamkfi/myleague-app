using Domain.Enums.Common;

namespace Domain.Services.Common;

/// <summary>
/// Default and validation for a season's standings order.
/// Stored default is the ints 0,1,2,4,5.
/// </summary>
public static class StandingSortCriteria
{
    /// <summary>
    /// Points, overall goal difference, overall goals scored, head-to-head points, then head-to-head goal difference.
    /// </summary>
    public static IReadOnlyList<StandingSortCriterion> Default { get; } =
    [
        StandingSortCriterion.Points,
        StandingSortCriterion.GoalDifference,
        StandingSortCriterion.GoalsFor,
        StandingSortCriterion.HeadToHeadPoints,
        StandingSortCriterion.HeadToHeadGoalDifference
    ];

    /// <summary>
    /// Order used before seasons stored their own criteria. Kept for tournaments.
    /// </summary>
    public static IReadOnlyList<StandingSortCriterion> LegacyWithoutGoalsAgainst { get; } =
    [
        StandingSortCriterion.Points,
        StandingSortCriterion.GoalDifference,
        StandingSortCriterion.GoalsFor
    ];

    /// <summary>
    /// Returns the default list when <paramref name="criteria"/> is omitted.
    /// </summary>
    public static List<StandingSortCriterion> Resolve(IEnumerable<StandingSortCriterion>? criteria)
    {
        if (criteria is null)
            return Default.ToList();

        List<StandingSortCriterion> list = criteria.ToList();
        Validate(list);
        return list;
    }

    /// <summary>
    /// True when the list is omitted or a non-empty set of known values without duplicates.
    /// </summary>
    public static bool IsValid(IReadOnlyCollection<StandingSortCriterion>? criteria)
    {
        if (criteria is null)
            return true;
        if (criteria.Count == 0)
            return false;
        if (criteria.Distinct().Count() != criteria.Count)
            return false;

        foreach (StandingSortCriterion criterion in criteria)
        {
            if (!Enum.IsDefined(criterion))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Rejects an empty list, duplicates, and unknown values.
    /// </summary>
    public static void Validate(IReadOnlyList<StandingSortCriterion> criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        if (!IsValid(criteria))
            throw new ArgumentException("Ranking criteria must be a non-empty list of unique values.", nameof(criteria));
    }

    /// <summary>
    /// True when ordering needs the matches played among the tied teams.
    /// </summary>
    public static bool UsesHeadToHead(IReadOnlyList<StandingSortCriterion> criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        foreach (StandingSortCriterion criterion in criteria)
        {
            if (criterion is StandingSortCriterion.HeadToHeadPoints
                or StandingSortCriterion.HeadToHeadGoalDifference
                or StandingSortCriterion.HeadToHeadGoalsFor)
            {
                return true;
            }
        }

        return false;
    }
}
