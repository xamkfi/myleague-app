using Application.Configuration;
using Application.Interfaces.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Options;

namespace MyLeague.Infrastructure.Services.Common;

public class SiteSettingsProvider : ISiteSettingsProvider
{
    private readonly ISiteSettingsRepository _repository;
    private readonly SiteSettingsCache _cache;
    private readonly JwtConfiguration _jwtConfig;
    private readonly LoginCodeConfiguration _loginCodeConfig;

    public SiteSettingsProvider(
        ISiteSettingsRepository repository,
        SiteSettingsCache cache,
        IOptions<JwtConfiguration> jwtConfig,
        IOptions<LoginCodeConfiguration> loginCodeConfig)
    {
        _repository = repository;
        _cache = cache;
        _jwtConfig = jwtConfig.Value;
        _loginCodeConfig = loginCodeConfig.Value;
    }

    public async Task<EffectiveAuthSettings> GetEffectiveAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGet(out EffectiveAuthSettings cached))
        {
            return cached;
        }

        SiteSettings? row = await _repository.GetAsync(cancellationToken);
        EffectiveAuthSettings settings = row is null
            ? FromConfiguration()
            : FromEntity(row);

        _cache.Set(settings);
        return settings;
    }

    public void Invalidate()
    {
        _cache.Invalidate();
    }

    private EffectiveAuthSettings FromConfiguration()
    {
        return new EffectiveAuthSettings(
            _jwtConfig.AccessTokenExpirationMinutes,
            _jwtConfig.RefreshTokenExpirationDays,
            _loginCodeConfig.ExpirationMinutes,
            _loginCodeConfig.MaxAttempts,
            SiteSettings.SessionExpiryWarningMinutesDefault,
            IsPersisted: false);
    }

    private static EffectiveAuthSettings FromEntity(SiteSettings row)
    {
        return new EffectiveAuthSettings(
            row.AccessTokenExpirationMinutes,
            row.RefreshTokenExpirationDays,
            row.LoginCodeExpirationMinutes,
            row.LoginCodeMaxAttempts,
            row.SessionExpiryWarningMinutes,
            IsPersisted: true);
    }
}
