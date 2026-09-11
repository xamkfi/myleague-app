using Domain.Entities;

namespace Domain.Entities.Common;

/// <summary>
/// Singleton site-wide settings. One row; auth timings apply to newly issued tokens and login codes.
/// </summary>
public class SiteSettings : BaseEntity
{
    public const int AccessTokenExpirationMinutesMin = 2;
    public const int AccessTokenExpirationMinutesMax = 180;
    public const int AccessTokenExpirationMinutesDefault = 15;

    public const int RefreshTokenExpirationDaysMin = 1;
    public const int RefreshTokenExpirationDaysMax = 90;
    public const int RefreshTokenExpirationDaysDefault = 7;

    public const int LoginCodeExpirationMinutesMin = 2;
    public const int LoginCodeExpirationMinutesMax = 60;
    public const int LoginCodeExpirationMinutesDefault = 10;

    public const int LoginCodeMaxAttemptsMin = 3;
    public const int LoginCodeMaxAttemptsMax = 20;
    public const int LoginCodeMaxAttemptsDefault = 5;

    public const int SessionExpiryWarningMinutesMin = 1;
    public const int SessionExpiryWarningMinutesMax = 30;
    public const int SessionExpiryWarningMinutesDefault = 5;

    public int AccessTokenExpirationMinutes { get; private set; }

    public int RefreshTokenExpirationDays { get; private set; }

    public int LoginCodeExpirationMinutes { get; private set; }

    public int LoginCodeMaxAttempts { get; private set; }

    public int SessionExpiryWarningMinutes { get; private set; }

    private SiteSettings()
    {
    }

    public SiteSettings(
        Guid id,
        int accessTokenExpirationMinutes,
        int refreshTokenExpirationDays,
        int loginCodeExpirationMinutes,
        int loginCodeMaxAttempts,
        int sessionExpiryWarningMinutes)
        : base(id)
    {
        Apply(
            accessTokenExpirationMinutes,
            refreshTokenExpirationDays,
            loginCodeExpirationMinutes,
            loginCodeMaxAttempts,
            sessionExpiryWarningMinutes);
    }

    public void Update(
        int accessTokenExpirationMinutes,
        int refreshTokenExpirationDays,
        int loginCodeExpirationMinutes,
        int loginCodeMaxAttempts,
        int sessionExpiryWarningMinutes)
    {
        Apply(
            accessTokenExpirationMinutes,
            refreshTokenExpirationDays,
            loginCodeExpirationMinutes,
            loginCodeMaxAttempts,
            sessionExpiryWarningMinutes);
        UpdatedAt = DateTime.UtcNow;
    }

    private void Apply(
        int accessTokenExpirationMinutes,
        int refreshTokenExpirationDays,
        int loginCodeExpirationMinutes,
        int loginCodeMaxAttempts,
        int sessionExpiryWarningMinutes)
    {
        AccessTokenExpirationMinutes = ValidateRange(
            accessTokenExpirationMinutes,
            AccessTokenExpirationMinutesMin,
            AccessTokenExpirationMinutesMax,
            nameof(accessTokenExpirationMinutes));
        RefreshTokenExpirationDays = ValidateRange(
            refreshTokenExpirationDays,
            RefreshTokenExpirationDaysMin,
            RefreshTokenExpirationDaysMax,
            nameof(refreshTokenExpirationDays));
        LoginCodeExpirationMinutes = ValidateRange(
            loginCodeExpirationMinutes,
            LoginCodeExpirationMinutesMin,
            LoginCodeExpirationMinutesMax,
            nameof(loginCodeExpirationMinutes));
        LoginCodeMaxAttempts = ValidateRange(
            loginCodeMaxAttempts,
            LoginCodeMaxAttemptsMin,
            LoginCodeMaxAttemptsMax,
            nameof(loginCodeMaxAttempts));
        SessionExpiryWarningMinutes = ValidateRange(
            sessionExpiryWarningMinutes,
            SessionExpiryWarningMinutesMin,
            SessionExpiryWarningMinutesMax,
            nameof(sessionExpiryWarningMinutes));
    }

    private static int ValidateRange(int value, int min, int max, string paramName)
    {
        if (value < min || value > max)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                $"{paramName} must be between {min} and {max}.");
        }

        return value;
    }
}
