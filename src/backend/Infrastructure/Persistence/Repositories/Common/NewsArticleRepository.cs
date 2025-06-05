using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories;
using System.Text.Json;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common
{
    /// <summary>
    /// Implementation of the news article repository
    /// </summary>
    public class NewsArticleRepository : RepositoryBase<NewsArticle, CommonDbContext>, INewsArticleRepository
    {
        /// <summary>
        /// Initializes a new instance of the NewsArticleRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public NewsArticleRepository(CommonDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a news by ID
        /// </summary>
        /// <param name="id">The news ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The news if found, null otherwise</returns>
        public async Task<NewsArticle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
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
        public async Task<IEnumerable<NewsArticle>> GetByAuthorAsync(string author, CancellationToken cancellationToken = default)
        {
            return await _entities
                .Where(n => n.Author == author)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets news by category
        /// </summary>
        /// <param name="category">The news category as string</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of news in the specified category</returns>
        public async Task<IEnumerable<NewsArticle>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return new List<NewsArticle>();
            }

            // Parse the category string to enum for comparison
            if (Enum.TryParse<NewsCategory>(category, true, out NewsCategory parsedCategory))
            {
                return await _entities
                    .Where(n => n.Category == parsedCategory)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync(cancellationToken);
            }

            return new List<NewsArticle>();
        }

        /// <summary>
        /// Gets news by sports category
        /// </summary>
        /// <param name="sportCategory">The sports category as string</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of news in the specified sports category</returns>
        public async Task<IEnumerable<NewsArticle>> GetBySportCategoryAsync(string sportCategory, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sportCategory))
            {
                return new List<NewsArticle>();
            }

            // Parse the sport category string to enum for comparison
            if (Enum.TryParse<SportsCategory>(sportCategory, true, out SportsCategory parsedSportCategory))
            {
                return await _entities
                    .Where(n => n.SportCategory == parsedSportCategory)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync(cancellationToken);
            }

            return new List<NewsArticle>();
        }

        /// <summary>
        /// Gets news by tag
        /// </summary>
        /// <param name="tag">The tag to search for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of news containing the specified tag</returns>
        public async Task<IEnumerable<NewsArticle>> GetByTagAsync(string tag, CancellationToken cancellationToken = default)
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
        public async Task<IEnumerable<NewsArticle>> GetRecentAsync(int count = 10, bool includeArchived = false, CancellationToken cancellationToken = default)
        {
            IQueryable<NewsArticle> query = _entities;

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
        public async Task<IEnumerable<NewsArticle>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<NewsArticle>();
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
        public async Task<IEnumerable<NewsArticle>> GetAllAsync(int page, int pageSize, string? category = null, string? sportCategory = null, string? author = null, bool includeArchived = false, CancellationToken cancellationToken = default)
        {
            IQueryable<NewsArticle> query = _entities;

            // Apply filters
            if (!includeArchived)
            {
                query = query.Where(n => !n.IsArchived);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                if (Enum.TryParse<NewsCategory>(category, true, out NewsCategory parsedCategory))
                {
                    query = query.Where(n => n.Category == parsedCategory);
                }
            }

            if (!string.IsNullOrWhiteSpace(sportCategory))
            {
                if (Enum.TryParse<SportsCategory>(sportCategory, true, out SportsCategory parsedSportCategory))
                {
                    query = query.Where(n => n.SportCategory == parsedSportCategory);
                }
            }

            if (!string.IsNullOrWhiteSpace(author))
            {
                query = query.Where(n => EF.Functions.ILike(n.Author ?? "", $"%{author}%"));
            }

            // Apply pagination
            int skip = (page - 1) * pageSize;

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets all unique tags used in news articles
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of unique tags</returns>
        public async Task<IEnumerable<string>> GetAllTagsAsync(CancellationToken cancellationToken = default)
        {
            // Get all news articles that have tags
            var newsWithTags = await _entities
                .Where(n => n.Tags.Count > 0)
                .Select(n => EF.Property<string>(n, "Tags"))
                .ToListAsync(cancellationToken);

            // Extract unique tags from JSON arrays
            var allTags = new HashSet<string>();

            foreach (var tagsJson in newsWithTags)
            {
                if (!string.IsNullOrWhiteSpace(tagsJson))
                {
                    try
                    {
                        var tags = System.Text.Json.JsonSerializer.Deserialize<List<string>>(tagsJson);
                        if (tags != null)
                        {
                            foreach (var tag in tags)
                            {
                                if (!string.IsNullOrWhiteSpace(tag))
                                {
                                    allTags.Add(tag.Trim());
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Skip invalid JSON
                    }
                }
            }

            return allTags.OrderBy(t => t).ToList();
        }

        /// <summary>
        /// Saves a news item (insert or update)
        /// </summary>
        /// <param name="news">The news to save</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        public async Task SaveAsync(NewsArticle news, CancellationToken cancellationToken = default)
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
        public async Task<IEnumerable<NewsArticle>> GetArchivedAsync(CancellationToken cancellationToken = default)
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
        public async Task<IEnumerable<NewsArticle>> GetActiveAsync(CancellationToken cancellationToken = default)
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
        public async Task CreateNews(NewsArticle news)
        {
            await _entities.AddAsync(news);
        }

        /// <summary>
        /// Updates an existing news item
        /// </summary>
        /// <param name="news">The news to update</param>
        /// <returns>Task representing the async operation</returns>
        public Task UpdateNews(NewsArticle news)
        {
            _entities.Update(news);
            return Task.CompletedTask;
        }
    }
} 