using Domain.Entities.Common;

namespace Domain.Repositories.Common
{
    public interface IPageContentRepository
    {
        Task<PageContent?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<PageContent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task SaveAsync(PageContent pageContent, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);
    }
}
