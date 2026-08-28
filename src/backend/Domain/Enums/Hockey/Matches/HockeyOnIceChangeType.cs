namespace Domain.Enums.Hockey.Matches;

/// <summary>
/// Represents a type of on-ice personnel change.
/// </summary>
public enum HockeyOnIceChangeType
{
    /// <summary>
    /// PlayerAdded
    /// </summary>
    PlayerAdded = 0,
    /// <summary>
    /// PlayerRemoved
    /// </summary>
    PlayerRemoved = 1,
    /// <summary>
    /// PlayerSwapped
    /// </summary>
    PlayerSwapped = 2,
    /// <summary>
    /// PlayerMoved
    /// </summary>
    PlayerMoved = 3,
    /// <summary>
    /// LineApplied
    /// </summary>
    LineApplied = 4,
    /// <summary>
    /// IceCleared
    /// </summary>
    IceCleared = 5,
    /// <summary>
    /// GoaliePulled
    /// </summary>
    GoaliePulled = 6,
    /// <summary>
    /// GoalieReturned
    /// </summary>
    GoalieReturned = 7,
    /// <summary>
    /// ExtraAttackerAdded
    /// </summary>
    ExtraAttackerAdded = 8,
    /// <summary>
    /// ExtraAttackerRemoved
    /// </summary>
    ExtraAttackerRemoved = 9
}

