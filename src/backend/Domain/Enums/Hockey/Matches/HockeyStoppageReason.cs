namespace Domain.Enums.Hockey.Matches;

/// <summary>
/// Represents why play was stopped.
/// </summary>
public enum HockeyStoppageReason
{
    /// <summary>
    /// Goal
    /// </summary>
    Goal = 0,
    /// <summary>
    /// Offside
    /// </summary>
    Offside = 1,
    /// <summary>
    /// Icing
    /// </summary>
    Icing = 2,
    /// <summary>
    /// PuckOutOfPlay
    /// </summary>
    PuckOutOfPlay = 3,
    /// <summary>
    /// HandPass
    /// </summary>
    HandPass = 4,
    /// <summary>
    /// HighStick
    /// </summary>
    HighStick = 5,
    /// <summary>
    /// GoalieFreeze
    /// </summary>
    GoalieFreeze = 6,
    /// <summary>
    /// NetDislodged
    /// </summary>
    NetDislodged = 7,
    /// <summary>
    /// PenaltyCalled
    /// </summary>
    PenaltyCalled = 8,
    /// <summary>
    /// Injury
    /// </summary>
    Injury = 9,
    /// <summary>
    /// Timeout
    /// </summary>
    Timeout = 10,
    /// <summary>
    /// VideoReview
    /// </summary>
    VideoReview = 11,
    /// <summary>
    /// PeriodEnded
    /// </summary>
    PeriodEnded = 12,
    /// <summary>
    /// RefereeWhistle
    /// </summary>
    RefereeWhistle = 13
}

