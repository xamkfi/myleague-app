namespace Application.Features.Common.SiteSettings.DTOs;

public record SiteSettingsDto(
    int AccessTokenExpirationMinutes,
    int RefreshTokenExpirationDays,
    int LoginCodeExpirationMinutes,
    int LoginCodeMaxAttempts,
    int SessionExpiryWarningMinutes,
    bool IsPersisted);
