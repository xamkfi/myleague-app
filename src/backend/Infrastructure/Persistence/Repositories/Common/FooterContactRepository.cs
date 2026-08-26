using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common;

public class FooterContactRepository
    : RepositoryBase<FooterContact, CommonDbContext>, IFooterContactRepository
{
    public FooterContactRepository(CommonDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<FooterContact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _entities.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<FooterContact>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _entities
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(FooterContact contact, CancellationToken cancellationToken = default)
    {
        await _entities.AddAsync(contact, cancellationToken);
    }

    public Task UpdateAsync(FooterContact contact, CancellationToken cancellationToken = default)
    {
        _entities.Update(contact);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(FooterContact contact, CancellationToken cancellationToken = default)
    {
        _entities.Remove(contact);
        return Task.CompletedTask;
    }
}
