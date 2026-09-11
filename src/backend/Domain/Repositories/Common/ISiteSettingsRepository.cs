using Domain.Entities.Common;

namespace Domain.Repositories.Common;

/// <summary>
/// Repository contract for the singleton site settings row.
/// </summary>
public interface ISiteSettingsRepository
{
    Task<SiteSettings?> GetAsync(CancellationToken cancellationToken = default);

    Task AddAsync(SiteSettings settings, CancellationToken cancellationToken = default);

    Task UpdateAsync(SiteSettings settings, CancellationToken cancellationToken = default);
}
