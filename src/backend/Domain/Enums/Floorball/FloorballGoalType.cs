namespace Domain.Enums.Floorball;

/// <summary>
/// Types of goals that can be scored in a floorball match
/// </summary>
public enum FloorballGoalType
{
    /// <summary>
    /// Regular goal scored during normal play
    /// </summary>
    Regular = 0,

    /// <summary>
    /// Goal scored during power play (opponent has penalty)
    /// </summary>
    PowerPlay = 1,

    /// <summary>
    /// Goal scored while short-handed (team has penalty)
    /// </summary>
    ShortHanded = 2,

    /// <summary>
    /// Empty net goal (opponent goalie not in goal)
    /// </summary>
    EmptyNet = 3,

    /// <summary>
    /// Penalty shot goal
    /// </summary>
    PenaltyShot = 4,

    /// <summary>
    /// Own goal (scored by defending team)
    /// </summary>
    OwnGoal = 5,

    /// <summary>
    /// Goal scored in overtime
    /// </summary>
    Overtime = 6,

    /// <summary>
    /// Goal scored in shootout
    /// </summary>
    Shootout = 7
} 