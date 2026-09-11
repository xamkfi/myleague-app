namespace Domain.Enums.Hockey.Matches;

/// <summary>
/// Represents a period-related action during a match.
/// </summary>
public enum HockeyPeriodAction
{
    /// <summary>
    /// PeriodStarted
    /// </summary>
    PeriodStarted = 0,
    /// <summary>
    /// PeriodEnded
    /// </summary>
    PeriodEnded = 1,
    /// <summary>
    /// IntermissionStarted
    /// </summary>
    IntermissionStarted = 2,
    /// <summary>
    /// OvertimeStarted
    /// </summary>
    OvertimeStarted = 3,
    /// <summary>
    /// ShootoutStarted
    /// </summary>
    ShootoutStarted = 4
}

