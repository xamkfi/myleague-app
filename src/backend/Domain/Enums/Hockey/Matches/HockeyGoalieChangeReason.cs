namespace Domain.Enums.Hockey.Matches;

/// <summary>
/// Represents the reason for a goalie change.
/// </summary>
public enum HockeyGoalieChangeReason
{
    /// <summary>
    /// RegularChange
    /// </summary>
    RegularChange = 0,
    /// <summary>
    /// PulledForExtraAttacker
    /// </summary>
    PulledForExtraAttacker = 1,
    /// <summary>
    /// ReturnedToNet
    /// </summary>
    ReturnedToNet = 2,
    /// <summary>
    /// Injury
    /// </summary>
    Injury = 3,
    /// <summary>
    /// Performance
    /// </summary>
    Performance = 4,
    /// <summary>
    /// DelayedPenalty
    /// </summary>
    DelayedPenalty = 5,
    /// <summary>
    /// Shootout
    /// </summary>
    Shootout = 6
}

