using Domain.Entities.Common;
using Domain.Enums.Common;

namespace Domain.Repositories.Common
{
    public interface INewsRepository
    {
        /// <summary>
        /// Gets a news by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<News?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets a news by Author
        /// </summary>
        /// <param name="author"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<News>> GetByAuthorAsync(string author, CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets a news by Category
        /// </summary>
        /// <param name="category"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<News>> GetByCategoryAsync(NewsCategory category, CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets news by sports category
        /// </summary>
        Task<IEnumerable<News>> GetBySportCategoryAsync(SportsCategory sportCategory, CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets a news by Tags
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<News>> GetByTagAsync(string tag, CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets the newest news
        /// </summary>
        /// <param name="count"></param>
        /// <param name="includeArchived"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<News>> GetRecentAsync(int count = 10, bool includeArchived = false, CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets a news by search
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<News>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
        /// <summary>
        /// Saves the news
        /// </summary>
        /// <param name="news"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SaveAsync(News news, CancellationToken cancellationToken = default);
        /// <summary>
        /// Check if the news exists
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets archived news
        /// </summary>
        Task<IEnumerable<News>> GetArchivedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets active (non-archived) news
        /// </summary>
        Task<IEnumerable<News>> GetActiveAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Creates a news
        /// </summary>
        /// <param name="news"></param>
        /// <returns></returns>
        Task CreateNews(News news);
        /// <summary>
        /// Updates an existing news
        /// </summary>
        /// <param name="news"></param>
        /// <returns></returns>
        Task UpdateNews(News news);
    }
}
