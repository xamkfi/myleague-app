namespace Domain.Constants;

/// <summary>
/// Role name constants for use in authorization attributes.
/// Values must match the <see cref="Domain.Enums.Common.UserRole"/> member names,
/// which are emitted as JWT role claims.
/// </summary>
public static class AuthRoles
{
    /// <summary>
    /// Club administrator role name
    /// </summary>
    public const string ClubAdmin = "ClubAdmin";

    /// <summary>
    /// System administrator role name
    /// </summary>
    public const string SystemAdmin = "SystemAdmin";

    /// <summary>
    /// Team leader role name
    /// </summary>
    public const string TeamLeader = "TeamLeader";

    /// <summary>
    /// Comma-separated list for endpoints restricted to administrators
    /// </summary>
    public const string AdminOnly = ClubAdmin + "," + SystemAdmin;

    /// <summary>
    /// Comma-separated list for endpoints available to team leaders and administrators
    /// </summary>
    public const string TeamLeaderOrAdmin = TeamLeader + "," + AdminOnly;
}
