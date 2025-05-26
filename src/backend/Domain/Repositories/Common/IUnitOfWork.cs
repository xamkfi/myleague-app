using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories.Common;

/// <summary>
/// Unit of Work interface for managing database transactions
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
} 