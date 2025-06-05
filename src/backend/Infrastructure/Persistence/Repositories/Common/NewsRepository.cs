using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common
{
    /// <summary>
    /// Implementation of the news repository
    /// </summary>
    public class NewsRepository : RepositoryBase<News, CommonDbContext>, INewsRepository
    {
        /// <summary>
        /// Initializes a new instance of the NewsRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public NewsRepository(CommonDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a news by ID
        /// </summary>
        /// <param name="id">The news ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The news if found, null otherwise</returns>
        public async Task<News?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _entities
                .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        }

        /// <summary>
        /// Gets news by author
        /// </summary>
        /// <param name="author">The author name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of news by the specified author</returns>
        public async Task<IEnumerable<News>> GetByAuthorAsync(string author, CancellationToken cancellationToken = default)
        {
            return await _entities
                .Where(n => n.Author == author)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets news by category
        /// </summary>
        /// <param name="category">The news category</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of news in the specified category</returns>
        public async Task<IEnumerable<News>> GetByCategoryAsync(NewsCategory category, CancellationToken cancellationToken = default)
        {
            return await _entities
                .Where(n => n.Category == category)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets news by sports category
        /// </summary>
        /// <param name="sportCategory">The sports category</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of news in the specified sports category</returns>
        public async Task<IEnumerable<News>> GetBySportCategoryAsync(SportsCategory sportCategory, CancellationToken cancellationToken = default)
        {
            return await _entities
                .Where(n => n.SportCategory == sportCategory)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets news by tag
        /// </summary>
        /// <param name="tag">The tag to search for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of news containing the specified tag</returns>
        public async Task<IEnumerable<News>> GetByTagAsync(string tag, CancellationToken cancellationToken = default)
        {
            return await _entities
                .Where(n => EF.Functions.JsonContains(EF.Property<string>(n, "Tags"), $"\"{tag}\""))
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets the most recent news
        /// </summary>
        /// <param name="count">Number of news items to retrieve</param>
        /// <param name="includeArchived">Whether to include archived news</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of recent news</returns>
        public async Task<IEnumerable<News>> GetRecentAsync(int count = 10, bool includeArchived = false, CancellationToken cancellationToken = default)
        {
            IQueryable<News> query = _entities;

            if (!includeArchived)
            {
                query = query.Where(n => !n.IsArchived);
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Searches for news by search term
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of news matching the search term</returns>
        public async Task<IEnumerable<News>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<News>();
            }

            string lowerSearchTerm = searchTerm.ToLower();

            return await _entities
                .Where(n => EF.Functions.ILike(n.Title, $"%{lowerSearchTerm}%") ||
                           EF.Functions.ILike(n.Summary ?? "", $"%{lowerSearchTerm}%") ||
                           EF.Functions.ILike(n.ContentHtml, $"%{lowerSearchTerm}%") ||
                           EF.Functions.ILike(n.Author ?? "", $"%{lowerSearchTerm}%"))
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Saves a news item (insert or update)
        /// </summary>
        /// <param name="news">The news to save</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        public async Task SaveAsync(News news, CancellationToken cancellationToken = default)
        {
            bool exists = await ExistsAsync(news.Id, cancellationToken);
            
            if (exists)
            {
                _entities.Update(news);
            }
            else
            {
                await _entities.AddAsync(news, cancellationToken);
            }
        }

        /// <summary>
        /// Checks if a news item exists
        /// </summary>
        /// <param name="id">The news ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if the news exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _entities.AnyAsync(n => n.Id == id, cancellationToken);
        }

        /// <summary>
        /// Gets archived news
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of archived news</returns>
        public async Task<IEnumerable<News>> GetArchivedAsync(CancellationToken cancellationToken = default)
        {
            return await _entities
                .Where(n => n.IsArchived)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets active (non-archived) news
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of active news</returns>
        public async Task<IEnumerable<News>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return await _entities
                .Where(n => !n.IsArchived)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Creates a new news item
        /// </summary>
        /// <param name="news">The news to create</param>
        /// <returns>Task representing the async operation</returns>
        public async Task CreateNews(News news)
        {
            await _entities.AddAsync(news);
        }

        /// <summary>
        /// Updates an existing news item
        /// </summary>
        /// <param name="news">The news to update</param>
        /// <returns>Task representing the async operation</returns>
        public Task UpdateNews(News news)
        {
            _entities.Update(news);
            return Task.CompletedTask;
        }
    }
} 