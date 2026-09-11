namespace Domain.Enums.Hockey.Teams;

/// <summary>
/// Represents a player's roster availability status.
/// </summary>
public enum HockeyRosterStatus
{
    /// <summary>
    /// Active
    /// </summary>
    Active = 0,
    /// <summary>
    /// Inactive
    /// </summary>
    Inactive = 1,
    /// <summary>
    /// Injured
    /// </summary>
    Injured = 2,
    /// <summary>
    /// DayToDay
    /// </summary>
    DayToDay = 3,
    /// <summary>
    /// LongTermInjured
    /// </summary>
    LongTermInjured = 4,
    /// <summary>
    /// Suspended
    /// </summary>
    Suspended = 5,
    /// <summary>
    /// Affiliate
    /// </summary>
    Affiliate = 6,
    /// <summary>
    /// Tryout
    /// </summary>
    Tryout = 7,
    /// <summary>
    /// Guest
    /// </summary>
    Guest = 8,
    /// <summary>
    /// Loaned
    /// </summary>
    Loaned = 9
}

