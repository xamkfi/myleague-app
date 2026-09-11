namespace Application.Interfaces.Common;

/// <summary>
/// Effective auth timings: persisted site settings when present, otherwise appsettings.
/// </summary>
public record EffectiveAuthSettings(
    int AccessTokenExpirationMinutes,
    int RefreshTokenExpirationDays,
    int LoginCodeExpirationMinutes,
    int LoginCodeMaxAttempts,
    int SessionExpiryWarningMinutes,
    bool IsPersisted);

/// <summary>
/// Reads effective site settings at token/login-code issuance time.
/// </summary>
public interface ISiteSettingsProvider
{
    Task<EffectiveAuthSettings> GetEffectiveAsync(CancellationToken cancellationToken = default);

    void Invalidate();
}
