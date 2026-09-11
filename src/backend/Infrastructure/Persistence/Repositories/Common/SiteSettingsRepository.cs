using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common;

public class SiteSettingsRepository
    : RepositoryBase<SiteSettings, CommonDbContext>, ISiteSettingsRepository
{
    public SiteSettingsRepository(CommonDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<SiteSettings?> GetAsync(CancellationToken cancellationToken = default)
    {
        return await _entities.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(SiteSettings settings, CancellationToken cancellationToken = default)
    {
        await _entities.AddAsync(settings, cancellationToken);
    }

    public Task UpdateAsync(SiteSettings settings, CancellationToken cancellationToken = default)
    {
        _entities.Update(settings);
        return Task.CompletedTask;
    }
}
