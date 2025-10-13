using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories.Floorball;

/// <summary>
/// Unit of Work interface for managing Floorball database transactions
/// </summary>
public interface IFloorballUnitOfWork
{
    /// <summary>
    /// Saves all changes to the Floorball database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
} 