namespace Domain.Enums.Common;

/// <summary>
/// Criterion used to order a league table.
/// </summary>
public enum StandingSortCriterion
{
    /// <summary>
    /// Points, highest first.
    /// </summary>
    Points = 0,

    /// <summary>
    /// Goal difference in the whole competition, highest first.
    /// </summary>
    GoalDifference = 1,

    /// <summary>
    /// Goals scored in the whole competition, highest first.
    /// </summary>
    GoalsFor = 2,

    /// <summary>
    /// Goals conceded, lowest first. Kept so previously stored lists still sort.
    /// </summary>
    GoalsAgainst = 3,

    /// <summary>
    /// Points from matches among the teams that are still tied, highest first.
    /// </summary>
    HeadToHeadPoints = 4,

    /// <summary>
    /// Goal difference from matches among the teams that are still tied, highest first.
    /// </summary>
    HeadToHeadGoalDifference = 5,

    /// <summary>
    /// Goals scored in matches among the teams that are still tied, highest first.
    /// </summary>
    HeadToHeadGoalsFor = 6,

    /// <summary>
    /// Penalty minutes, fewest first.
    /// </summary>
    PenaltyMinutes = 7,

    /// <summary>
    /// Stable drawing of lots. The same teams keep the same order between requests.
    /// </summary>
    Draw = 8
}
