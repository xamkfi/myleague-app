namespace Domain.Enums.Hockey.Matches;

/// <summary>
/// Represents the result of a shootout attempt.
/// </summary>
public enum HockeyShootoutAttemptResult
{
    /// <summary>
    /// Goal
    /// </summary>
    Goal = 0,
    /// <summary>
    /// Saved
    /// </summary>
    Saved = 1,
    /// <summary>
    /// Missed
    /// </summary>
    Missed = 2,
    /// <summary>
    /// Post
    /// </summary>
    Post = 3,
    /// <summary>
    /// NoShot
    /// </summary>
    NoShot = 4
}

