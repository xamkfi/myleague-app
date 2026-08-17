namespace Domain.Constants;

/// <summary>
/// Role name constants for use in authorization attributes.
/// Values must match the <see cref="Domain.Enums.Common.UserRole"/> member names,
/// which are emitted as JWT role claims.
/// </summary>
public static class AuthRoles
{
    /// <summary>
    /// Club administrator role name (club-scoped: manages one or more clubs and their teams)
    /// </summary>
    public const string ClubAdmin = "ClubAdmin";

    /// <summary>
    /// System administrator role name (site admin)
    /// </summary>
    public const string SystemAdmin = "SystemAdmin";

    /// <summary>
    /// Roles allowed on endpoints restricted to site administrators
    /// </summary>
    public const string AdminOnly = SystemAdmin;

    /// <summary>
    /// Comma-separated list for endpoints available to club admins and site administrators
    /// </summary>
    public const string ClubAdminOrAdmin = ClubAdmin + "," + SystemAdmin;
}
