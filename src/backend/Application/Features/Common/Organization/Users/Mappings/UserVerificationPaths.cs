using Domain.Enums.Common;

namespace Application.Features.Common.Organization.Users.Mappings;

/// <summary>
/// Maps a user role to the frontend pages of that role's own area, so invitation and
/// welcome emails send each user to the matching verify and login pages.
/// </summary>
public static class UserVerificationPaths
{
    /// <summary>
    /// Returns the verify-email path (relative to the frontend base URL) for the given role.
    /// </summary>
    /// <param name="role">The invited user's role.</param>
    public static string ForRole(UserRole role) => $"{AreaFor(role)}/verify-email";

    /// <summary>
    /// Returns the login path (relative to the frontend base URL) for the given role.
    /// </summary>
    /// <param name="role">The user's role.</param>
    public static string LoginForRole(UserRole role) => $"{AreaFor(role)}/login";

    private static string AreaFor(UserRole role) => role switch
    {
        UserRole.ClubAdmin => "/club-admin",
        UserRole.MatchOfficial => "/official",
        _ => "/admin",
    };
}
