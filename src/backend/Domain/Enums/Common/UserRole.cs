namespace Domain.Enums.Common;

/// <summary>
/// Defines the role of a system user
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Club administrator - can edit their own club's information (not delete it) and manage
    /// the teams under the club: jersey numbers and match roster/lineup announcements
    /// </summary>
    ClubAdmin = 0,

    /// <summary>
    /// System administrator - full access to all features
    /// </summary>
    SystemAdmin = 1
}
