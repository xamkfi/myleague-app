using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories.Hockey;

/// <summary>
/// Unit of Work interface for managing Hockey database transactions.
/// </summary>
public interface IHockeyUnitOfWork
{
    /// <summary>
    /// Saves all changes to the Hockey database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
