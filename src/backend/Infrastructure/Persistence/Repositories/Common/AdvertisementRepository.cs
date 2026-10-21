using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common;

public class AdvertisementRepository
    : RepositoryBase<Advertisement, CommonDbContext>, IAdvertisementRepository
{
    public AdvertisementRepository(CommonDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Advertisement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _entities.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Advertisement>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _entities
            .AsNoTracking()
            .OrderBy(x => x.Slot)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Advertisement>> GetVisibleAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        return await _entities
            .AsNoTracking()
            .Where(x => x.IsActive
                && (x.StartsAt == null || x.StartsAt <= utcNow)
                && (x.EndsAt == null || x.EndsAt > utcNow))
            .OrderBy(x => x.Slot)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Advertisement advertisement, CancellationToken cancellationToken = default)
    {
        await _entities.AddAsync(advertisement, cancellationToken);
    }

    public Task UpdateAsync(Advertisement advertisement, CancellationToken cancellationToken = default)
    {
        _entities.Update(advertisement);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Advertisement advertisement, CancellationToken cancellationToken = default)
    {
        _entities.Remove(advertisement);
        return Task.CompletedTask;
    }
}
