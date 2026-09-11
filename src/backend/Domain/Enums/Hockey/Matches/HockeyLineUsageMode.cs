namespace Domain.Enums.Hockey.Matches;

/// <summary>
/// Represents how line suggestions are used in a match.
/// </summary>
public enum HockeyLineUsageMode
{
    /// <summary>
    /// Optional
    /// </summary>
    Optional = 0,
    /// <summary>
    /// StartingSuggestion
    /// </summary>
    StartingSuggestion = 1,
    /// <summary>
    /// RotationSuggestion
    /// </summary>
    RotationSuggestion = 2,
    /// <summary>
    /// SpecialTeams
    /// </summary>
    SpecialTeams = 3,
    /// <summary>
    /// ShootoutOrder
    /// </summary>
    ShootoutOrder = 4,
    /// <summary>
    /// Custom
    /// </summary>
    Custom = 5
}

