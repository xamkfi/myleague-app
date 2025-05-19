namespace Domain.Enums.Floorball;

/// <summary>
/// Types of events that can occur in a floorball match
/// </summary>
public enum FloorballEventType
{
    /// <summary>
    /// Goal scored
    /// </summary>
    Goal = 0,

    /// <summary>
    /// Assist
    /// </summary>
    Assist = 1,

    /// <summary>
    /// Penalty
    /// </summary>
    Penalty = 2,

    /// <summary>
    /// Timeout taken
    /// </summary>
    Timeout = 3,

    /// <summary>
    /// Period started
    /// </summary>
    PeriodStart = 4,

    /// <summary>
    /// Period ended
    /// </summary>
    PeriodEnd = 5,

    /// <summary>
    /// Match started
    /// </summary>
    MatchStart = 6,

    /// <summary>
    /// Match ended
    /// </summary>
    MatchEnd = 7,

    /// <summary>
    /// Substitution
    /// </summary>
    Substitution = 8,

    /// <summary>
    /// Injury
    /// </summary>
    Injury = 9,

    /// <summary>
    /// Save by goalie
    /// </summary>
    Save = 10,

    /// <summary>
    /// Shot on goal
    /// </summary>
    Shot = 11,

    /// <summary>
    /// Other event
    /// </summary>
    Other = 99
} 
