namespace Domain.Enums.Common;

/// <summary>
/// Defines the role of a system user
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Club administrator - can manage their club's data
    /// </summary>
    ClubAdmin = 0,

    /// <summary>
    /// System administrator - full access to all features
    /// </summary>
    SystemAdmin = 1,

    /// <summary>
    /// Team leader - can manage jersey numbers and announce match rosters for their own teams
    /// </summary>
    TeamLeader = 2
}
