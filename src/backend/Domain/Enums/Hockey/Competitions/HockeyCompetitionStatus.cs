namespace Domain.Enums.Hockey.Competitions;

/// <summary>
/// Represents the lifecycle status of a hockey competition.
/// </summary>
public enum HockeyCompetitionStatus
{
    /// <summary>
    /// Draft
    /// </summary>
    Draft = 0,
    /// <summary>
    /// Published
    /// </summary>
    Published = 1,
    /// <summary>
    /// RegistrationOpen
    /// </summary>
    RegistrationOpen = 2,
    /// <summary>
    /// Active
    /// </summary>
    Active = 3,
    /// <summary>
    /// Completed
    /// </summary>
    Completed = 4,
    /// <summary>
    /// Cancelled
    /// </summary>
    Cancelled = 5
}

