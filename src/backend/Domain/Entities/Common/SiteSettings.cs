using Domain.Entities;

namespace Domain.Entities.Common;

/// <summary>
/// Singleton site-wide settings. One row; auth timings apply to newly issued tokens and login codes.
/// Player-licence cutoff is a recurring month/day in Europe/Helsinki.
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

    public const int PlayerLicenceResetMonthMin = 1;
    public const int PlayerLicenceResetMonthMax = 12;
    public const int PlayerLicenceResetMonthDefault = 5;

    public const int PlayerLicenceResetDayMin = 1;
    public const int PlayerLicenceResetDayMax = 31;
    public const int PlayerLicenceResetDayDefault = 1;

    public int AccessTokenExpirationMinutes { get; private set; }

    public int RefreshTokenExpirationDays { get; private set; }

    public int LoginCodeExpirationMinutes { get; private set; }

    public int LoginCodeMaxAttempts { get; private set; }

    public int SessionExpiryWarningMinutes { get; private set; }

    public int PlayerLicenceResetMonth { get; private set; }

    public int PlayerLicenceResetDay { get; private set; }

    public int? LastPlayerLicenceResetYear { get; private set; }

    private SiteSettings()
    {
        PlayerLicenceResetMonth = PlayerLicenceResetMonthDefault;
        PlayerLicenceResetDay = PlayerLicenceResetDayDefault;
    }

    public SiteSettings(
        Guid id,
        int accessTokenExpirationMinutes,
        int refreshTokenExpirationDays,
        int loginCodeExpirationMinutes,
        int loginCodeMaxAttempts,
        int sessionExpiryWarningMinutes,
        int playerLicenceResetMonth = PlayerLicenceResetMonthDefault,
        int playerLicenceResetDay = PlayerLicenceResetDayDefault)
        : base(id)
    {
        Apply(
            accessTokenExpirationMinutes,
            refreshTokenExpirationDays,
            loginCodeExpirationMinutes,
            loginCodeMaxAttempts,
            sessionExpiryWarningMinutes,
            playerLicenceResetMonth,
            playerLicenceResetDay);
    }

    public static SiteSettings CreateDefault(Guid id)
    {
        return new SiteSettings(
            id,
            AccessTokenExpirationMinutesDefault,
            RefreshTokenExpirationDaysDefault,
            LoginCodeExpirationMinutesDefault,
            LoginCodeMaxAttemptsDefault,
            SessionExpiryWarningMinutesDefault,
            PlayerLicenceResetMonthDefault,
            PlayerLicenceResetDayDefault);
    }

    public void Update(
        int accessTokenExpirationMinutes,
        int refreshTokenExpirationDays,
        int loginCodeExpirationMinutes,
        int loginCodeMaxAttempts,
        int sessionExpiryWarningMinutes,
        int playerLicenceResetMonth = PlayerLicenceResetMonthDefault,
        int playerLicenceResetDay = PlayerLicenceResetDayDefault)
    {
        Apply(
            accessTokenExpirationMinutes,
            refreshTokenExpirationDays,
            loginCodeExpirationMinutes,
            loginCodeMaxAttempts,
            sessionExpiryWarningMinutes,
            playerLicenceResetMonth,
            playerLicenceResetDay);
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkPlayerLicenceReset(int year)
    {
        if (year < 2000 || year > 2100)
        {
            throw new ArgumentOutOfRangeException(nameof(year), year, "year must be between 2000 and 2100.");
        }

        LastPlayerLicenceResetYear = year;
        UpdatedAt = DateTime.UtcNow;
    }

    public static bool IsValidLicenceResetDate(int month, int day)
    {
        if (month < PlayerLicenceResetMonthMin || month > PlayerLicenceResetMonthMax)
        {
            return false;
        }

        int maxDay = month == 2 ? 29 : DateTime.DaysInMonth(2024, month);
        return day >= PlayerLicenceResetDayMin && day <= maxDay;
    }

    private void Apply(
        int accessTokenExpirationMinutes,
        int refreshTokenExpirationDays,
        int loginCodeExpirationMinutes,
        int loginCodeMaxAttempts,
        int sessionExpiryWarningMinutes,
        int playerLicenceResetMonth,
        int playerLicenceResetDay)
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

        if (!IsValidLicenceResetDate(playerLicenceResetMonth, playerLicenceResetDay))
        {
            throw new ArgumentOutOfRangeException(
                nameof(playerLicenceResetDay),
                playerLicenceResetDay,
                "playerLicenceResetMonth and playerLicenceResetDay must form a real calendar day (29 February is allowed).");
        }

        PlayerLicenceResetMonth = playerLicenceResetMonth;
        PlayerLicenceResetDay = playerLicenceResetDay;
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
