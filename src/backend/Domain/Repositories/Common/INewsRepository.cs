using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.ValueObjects.Common;

namespace Domain.Repositories.Common
{
    public interface INewsRepository
    {
        Task<News?> GetByIdAsync(NewsId id, CancellationToken cancellationToken = default);
        Task<IEnumerable<News>> GetByAuthorAsync(string author, CancellationToken cancellationToken = default);
        Task<IEnumerable<News>> GetByCategoryAsync(NewsCategory category, CancellationToken cancellationToken = default);
        Task<IEnumerable<News>> GetByTagAsync(string tag, CancellationToken cancellationToken = default);
        Task<IEnumerable<News>> GetRecentAsync(int count = 10, bool includeArchived = false, CancellationToken cancellationToken = default);
        Task<IEnumerable<News>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
        Task SaveAsync(News news, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(NewsId id, CancellationToken cancellationToken = default);
    }
}
