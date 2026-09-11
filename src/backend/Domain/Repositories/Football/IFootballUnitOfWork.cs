using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories.Football;

/// <summary>
/// Unit of Work for football database transactions.
/// </summary>
public interface IFootballUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
