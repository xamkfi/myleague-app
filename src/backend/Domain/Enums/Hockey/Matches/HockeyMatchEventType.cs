namespace Domain.Enums.Hockey.Matches;

/// <summary>
/// Represents the top-level category of a match event.
/// </summary>
public enum HockeyMatchEventType
{
    /// <summary>
    /// Period
    /// </summary>
    Period = 0,
    /// <summary>
    /// Goal
    /// </summary>
    Goal = 1,
    /// <summary>
    /// Penalty
    /// </summary>
    Penalty = 2,
    /// <summary>
    /// Shot
    /// </summary>
    Shot = 3,
    /// <summary>
    /// Faceoff
    /// </summary>
    Faceoff = 4,
    /// <summary>
    /// Stoppage
    /// </summary>
    Stoppage = 5,
    /// <summary>
    /// Timeout
    /// </summary>
    Timeout = 6,
    /// <summary>
    /// GoalieChange
    /// </summary>
    GoalieChange = 7,
    /// <summary>
    /// VideoReview
    /// </summary>
    VideoReview = 8,
    /// <summary>
    /// ShootoutAttempt
    /// </summary>
    ShootoutAttempt = 9
}

