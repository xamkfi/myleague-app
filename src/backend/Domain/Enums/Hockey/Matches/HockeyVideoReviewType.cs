namespace Domain.Enums.Hockey.Matches;

/// <summary>
/// Represents the type of video review being conducted.
/// </summary>
public enum HockeyVideoReviewType
{
    /// <summary>
    /// PuckOverGoalLine
    /// </summary>
    PuckOverGoalLine = 0,
    /// <summary>
    /// GoalBeforeTimeExpired
    /// </summary>
    GoalBeforeTimeExpired = 1,
    /// <summary>
    /// HighStickGoal
    /// </summary>
    HighStickGoal = 2,
    /// <summary>
    /// KickingMotion
    /// </summary>
    KickingMotion = 3,
    /// <summary>
    /// GoalieInterference
    /// </summary>
    GoalieInterference = 4,
    /// <summary>
    /// OffsideBeforeGoal
    /// </summary>
    OffsideBeforeGoal = 5,
    /// <summary>
    /// PuckOutBeforeGoal
    /// </summary>
    PuckOutBeforeGoal = 6,
    /// <summary>
    /// PenaltyShotReview
    /// </summary>
    PenaltyShotReview = 7
}

