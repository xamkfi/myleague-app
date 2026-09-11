namespace Domain.Enums.Hockey.Matches;

/// <summary>
/// Represents special circumstances for a goal.
/// </summary>
public enum HockeyGoalSpecialType
{
    /// <summary>
    /// None
    /// </summary>
    None = 0,
    /// <summary>
    /// EmptyNet
    /// </summary>
    EmptyNet = 1,
    /// <summary>
    /// PenaltyShot
    /// </summary>
    PenaltyShot = 2,
    /// <summary>
    /// Shootout
    /// </summary>
    Shootout = 3,
    /// <summary>
    /// OwnGoal
    /// </summary>
    OwnGoal = 4,
    /// <summary>
    /// AwardedGoal
    /// </summary>
    AwardedGoal = 5
}

