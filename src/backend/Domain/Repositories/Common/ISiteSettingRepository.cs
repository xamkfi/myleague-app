using Domain.Entities.Common;

namespace Domain.Repositories.Common;

/// <summary>
/// Repository contract for general-purpose site settings.
/// </summary>
public interface ISiteSettingRepository
{
    /// <summary>
    /// Gets a site setting by key.
    /// </summary>
    Task<SiteSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a site setting.
    /// </summary>
    Task SaveAsync(SiteSetting setting, CancellationToken cancellationToken = default);
}
