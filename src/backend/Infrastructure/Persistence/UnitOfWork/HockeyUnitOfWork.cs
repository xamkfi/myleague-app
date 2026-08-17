using Domain.Repositories.Hockey;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// Unit of Work implementation for Hockey entities.
/// </summary>
public class HockeyUnitOfWork : IHockeyUnitOfWork
{
    private readonly HockeyDbContext _dbContext;

    public HockeyUnitOfWork(HockeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
