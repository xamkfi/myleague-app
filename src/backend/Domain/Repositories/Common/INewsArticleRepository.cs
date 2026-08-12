using Domain.Entities.Common;
using Domain.Enums.Common;

namespace Domain.Repositories.Common
{
    public interface INewsArticleRepository
    {
        /// <summary>
        /// Gets a news by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<NewsArticle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets a news by Author
        /// </summary>
        /// <param name="author"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<NewsArticle>> GetByAuthorAsync(string author, CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets news articles by Category
        /// </summary>
        /// <param name="category"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<NewsArticle>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets news by sports category
        /// </summary>
        Task<IEnumerable<NewsArticle>> GetBySportCategoryAsync(string sportCategory, CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets a news by Tags
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<NewsArticle>> GetByTagAsync(string tag, CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets the newest news
        /// </summary>
        /// <param name="count"></param>
        /// <param name="includeArchived"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<NewsArticle>> GetRecentAsync(int count = 10, bool includeArchived = false, CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets a news by search
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<NewsArticle>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all news articles with pagination and filtering
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="category">Optional category filter</param>
        /// <param name="sportCategory">Optional sport category filter</param>
        /// <param name="author">Optional author filter</param>
        /// <param name="includeArchived">Whether to include archived articles</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of news articles</returns>
        Task<IEnumerable<NewsArticle>> GetAllAsync(int page, int pageSize, string? category = null, string? sportCategory = null, string? search = null, string? author = null, bool includeArchived = false, string? teamCategory = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the total count of news articles with filtering
        /// </summary>
        /// <param name="category">Optional category filter</param>
        /// <param name="sportCategory">Optional sport category filter</param>
        /// <param name="author">Optional author filter</param>
        /// <param name="includeArchived">Whether to include archived articles</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Total count of matching news articles</returns>
        Task<int> GetCountAsync(string? category = null, string? sportCategory = null, string? search = null, string? author = null, bool includeArchived = false, string? teamCategory = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all unique tags used in news articles
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of unique tags</returns>
        Task<IEnumerable<string>> GetAllTagsAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Saves the news
        /// </summary>
        /// <param name="news"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SaveAsync(NewsArticle news, CancellationToken cancellationToken = default);
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
        Task<IEnumerable<NewsArticle>> GetArchivedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets active (non-archived) news
        /// </summary>
        Task<IEnumerable<NewsArticle>> GetActiveAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Creates a news
        /// </summary>
        /// <param name="news"></param>
        /// <returns></returns>
        Task CreateNews(NewsArticle news);
        /// <summary>
        /// Updates an existing news
        /// </summary>
        /// <param name="news"></param>
        /// <returns></returns>
        Task UpdateNews(NewsArticle news);

        /// <summary>
        /// Gets the newest news for the main news
        /// </summary>
        /// <returns></returns>
        Task<NewsArticle?> GetMainNews();

        /// <summary>
        /// Deletes the news by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteNews(Guid id);
    }
}
