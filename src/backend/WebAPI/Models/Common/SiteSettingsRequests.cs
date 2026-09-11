using System.ComponentModel.DataAnnotations;
using Domain.Entities.Common;

namespace WebAPI.Models.Common;

/// <summary>
/// Request model for updating site settings.
/// </summary>
public class UpdateSiteSettingsRequest
{
    /// <summary>Access token lifetime in minutes.</summary>
    [Range(SiteSettings.AccessTokenExpirationMinutesMin, SiteSettings.AccessTokenExpirationMinutesMax)]
    public int AccessTokenExpirationMinutes { get; set; }

    /// <summary>Refresh token lifetime in days.</summary>
    [Range(SiteSettings.RefreshTokenExpirationDaysMin, SiteSettings.RefreshTokenExpirationDaysMax)]
    public int RefreshTokenExpirationDays { get; set; }

    /// <summary>Login code lifetime in minutes.</summary>
    [Range(SiteSettings.LoginCodeExpirationMinutesMin, SiteSettings.LoginCodeExpirationMinutesMax)]
    public int LoginCodeExpirationMinutes { get; set; }

    /// <summary>Maximum failed login-code attempts before the code is locked.</summary>
    [Range(SiteSettings.LoginCodeMaxAttemptsMin, SiteSettings.LoginCodeMaxAttemptsMax)]
    public int LoginCodeMaxAttempts { get; set; }

    /// <summary>Minutes before access-token expiry to show the stay-logged-in reminder.</summary>
    [Range(SiteSettings.SessionExpiryWarningMinutesMin, SiteSettings.SessionExpiryWarningMinutesMax)]
    public int SessionExpiryWarningMinutes { get; set; }
}
