using Domain.Repositories.Floorball;
using MyLeague.Infrastructure.Persistence.Contexts;
using System.Threading;
using System.Threading.Tasks;

namespace MyLeague.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// Unit of Work implementation for Floorball entities
/// </summary>
public class FloorballUnitOfWork : IFloorballUnitOfWork
{
    private readonly FloorballDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the FloorballUnitOfWork class
    /// </summary>
    /// <param name="dbContext">The database context</param>
    public FloorballUnitOfWork(FloorballDbContext dbContext)
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
        return await UniqueConstraint.SaveChangesAsync(_dbContext, cancellationToken);
    }
} 