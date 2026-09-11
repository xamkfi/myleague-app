namespace Domain.Enums.Hockey.Matches;

/// <summary>
/// Represents the lifecycle status of a hockey match.
/// </summary>
public enum HockeyMatchStatus
{
    /// <summary>
    /// Scheduled
    /// </summary>
    Scheduled = 0,
    /// <summary>
    /// Warmup
    /// </summary>
    Warmup = 1,
    /// <summary>
    /// InProgress
    /// </summary>
    InProgress = 2,
    /// <summary>
    /// Intermission
    /// </summary>
    Intermission = 3,
    /// <summary>
    /// Overtime
    /// </summary>
    Overtime = 4,
    /// <summary>
    /// Shootout
    /// </summary>
    Shootout = 5,
    /// <summary>
    /// Finished
    /// </summary>
    Finished = 6,
    /// <summary>
    /// Cancelled
    /// </summary>
    Cancelled = 7,
    /// <summary>
    /// Postponed
    /// </summary>
    Postponed = 8,
    /// <summary>
    /// Suspended
    /// </summary>
    Suspended = 9,
    /// <summary>
    /// Forfeit
    /// </summary>
    Forfeit = 10
}

