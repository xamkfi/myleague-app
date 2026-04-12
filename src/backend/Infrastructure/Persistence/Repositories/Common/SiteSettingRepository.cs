using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common;

/// <summary>
/// Repository implementation for <see cref="SiteSetting"/>.
/// </summary>
public class SiteSettingRepository : ISiteSettingRepository
{
    private readonly CommonDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="SiteSettingRepository"/> class.
    /// </summary>
    public SiteSettingRepository(CommonDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<SiteSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        return await _dbContext.SiteSettings.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveAsync(SiteSetting setting, CancellationToken cancellationToken = default)
    {
        bool exists = await _dbContext.SiteSettings
            .AsNoTracking()
            .AnyAsync(x => x.Id == setting.Id, cancellationToken);

        if (!exists)
        {
            await _dbContext.SiteSettings.AddAsync(setting, cancellationToken);
        }
    }
}
