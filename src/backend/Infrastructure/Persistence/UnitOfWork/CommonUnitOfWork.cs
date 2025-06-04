using Domain.Repositories.Common;
using MyLeague.Infrastructure.Persistence.Contexts;
using System.Threading;
using System.Threading.Tasks;

namespace MyLeague.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// Unit of Work implementation for Common entities
/// </summary>
public class CommonUnitOfWork : IUnitOfWork
{
    private readonly CommonDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the CommonUnitOfWork class
    /// </summary>
    /// <param name="dbContext">The database context</param>
    public CommonUnitOfWork(CommonDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Saves all changes to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of state entries written to the database</returns>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
} 