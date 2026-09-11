using Domain.Repositories.Football;
using MyLeague.Infrastructure.Persistence.Contexts;
using System.Threading;
using System.Threading.Tasks;

namespace MyLeague.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// Unit of Work implementation for Football entities
/// </summary>
public class FootballUnitOfWork : IFootballUnitOfWork
{
    private readonly FootballDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the FootballUnitOfWork class
    /// </summary>
    /// <param name="dbContext">The database context</param>
    public FootballUnitOfWork(FootballDbContext dbContext)
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
