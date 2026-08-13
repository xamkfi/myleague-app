using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<NewsArticleRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the NewsArticleRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="logger">The logger</param>
        public NewsArticleRepository(CommonDbContext dbContext, ILogger<NewsArticleRepository> logger) : base(dbContext)
        {
            _logger = logger;
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
            try
            {
                if (string.IsNullOrWhiteSpace(author))
                {
                    _logger.LogWarning("GetByAuthorAsync called with null or empty author");
                    return new List<NewsArticle>();
                }

                return await _entities
                    .Where(n => n.Author == author)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving news articles by author: {Author}", author);
                throw;
            }
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
            try
            {
                if (string.IsNullOrWhiteSpace(tag))
                {
                    _logger.LogWarning("GetByTagAsync called with null or empty tag");
                    return new List<NewsArticle>();
                }

                return await _entities
                    .Where(n => EF.Functions.JsonContains(EF.Property<string>(n, "Tags"), $"\"{tag}\""))
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving news articles by tag: {Tag}", tag);
                throw;
            }
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
            try
            {
                if (count <= 0)
                {
                    _logger.LogWarning("GetRecentAsync called with invalid count: {Count}", count);
                    return new List<NewsArticle>();
                }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving recent news articles. Count: {Count}, IncludeArchived: {IncludeArchived}", count, includeArchived);
                throw;
            }
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
        public async Task<IEnumerable<NewsArticle>> GetAllAsync(int page, int pageSize, string? category = null, string? sportCategory = null, string? search = null, string? author = null, bool includeArchived = false, IReadOnlyCollection<string>? teamCategories = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (page <= 0)
                {
                    _logger.LogWarning("GetAllAsync called with invalid page number: {Page}", page);
                    page = 1;
                }

                if (pageSize <= 0)
                {
                    _logger.LogWarning("GetAllAsync called with invalid page size: {PageSize}", pageSize);
                    pageSize = 10;
                }

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

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(n =>
                        EF.Functions.Like(n.Title ?? "", $"%{search}%") ||
                        EF.Functions.Like(n.Summary ?? "", $"%{search}%"));
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

                query = ApplyTeamCategoryFilter(query, teamCategories);

                // Apply pagination
                int skip = (page - 1) * pageSize;

                return await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving paginated news articles. Page: {Page}, PageSize: {PageSize}", page, pageSize);
                throw;
            }
        }

        /// <summary>
        /// Gets the total count of news articles with filtering
        /// </summary>
        /// <param name="category">Optional category filter</param>
        /// <param name="sportCategory">Optional sport category filter</param>
        /// <param name="author">Optional author filter</param>
        /// <param name="includeArchived">Whether to include archived articles</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Total count of matching news articles</returns>
        public async Task<int> GetCountAsync(string? category = null, string? sportCategory = null, string? search = null, string? author = null, bool includeArchived = false, IReadOnlyCollection<string>? teamCategories = null, CancellationToken cancellationToken = default)
        {
            try
            {
                IQueryable<NewsArticle> query = _entities;

                // Apply same filters as GetAllAsync
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

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(n =>
                        EF.Functions.Like(n.Title ?? "", $"%{search}%") ||
                        EF.Functions.Like(n.Summary ?? "", $"%{search}%"));
                }

                if (!string.IsNullOrWhiteSpace(author))
                {
                    query = query.Where(n => EF.Functions.ILike(n.Author ?? "", $"%{author}%"));
                }

                query = ApplyTeamCategoryFilter(query, teamCategories);

                return await query.CountAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while counting news articles with filters. Category: {Category}, SportCategory: {SportCategory}, Author: {Author}", category, sportCategory, author);
                throw;
            }
        }

        /// <summary>
        /// Parses team category filter values into enum values, ignoring invalid input
        /// </summary>
        private static List<TeamCategory> ParseTeamCategories(IReadOnlyCollection<string>? teamCategories)
        {
            List<TeamCategory> parsed = new List<TeamCategory>();

            if (teamCategories is null)
            {
                return parsed;
            }

            foreach (string value in teamCategories)
            {
                if (!string.IsNullOrWhiteSpace(value)
                    && Enum.TryParse<TeamCategory>(value, true, out TeamCategory category)
                    && !parsed.Contains(category))
                {
                    parsed.Add(category);
                }
            }

            return parsed;
        }

        /// <summary>
        /// Filters articles for the selected audiences. Articles with no team category are shown to everyone.
        /// Uses equality predicates instead of Contains() so Npgsql can translate the nullable
        /// string-converted TeamCategory column.
        /// </summary>
        private static IQueryable<NewsArticle> ApplyTeamCategoryFilter(
            IQueryable<NewsArticle> query,
            IReadOnlyCollection<string>? teamCategories)
        {
            List<TeamCategory> parsed = ParseTeamCategories(teamCategories);
            if (parsed.Count == 0)
            {
                return query;
            }

            bool includeAdult = parsed.Contains(TeamCategory.Adult);
            bool includeYouth = parsed.Contains(TeamCategory.Youth);
            bool includeWomen = parsed.Contains(TeamCategory.Women);

            return query.Where(n =>
                n.TeamCategory == null
                || (includeAdult && n.TeamCategory == TeamCategory.Adult)
                || (includeYouth && n.TeamCategory == TeamCategory.Youth)
                || (includeWomen && n.TeamCategory == TeamCategory.Women));
        }

        /// <summary>
        /// Gets all unique tags used in news articles
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of unique tags</returns>
        public async Task<IEnumerable<string>> GetAllTagsAsync(CancellationToken cancellationToken = default)
        {
            // Get all news articles that have tags
            List<string> newsWithTags = await _entities
                .Where(n => n.Tags.Count > 0)
                .Select(n => EF.Property<string>(n, "Tags"))
                .ToListAsync(cancellationToken);

            // Extract unique tags from JSON arrays
            HashSet<string> allTags = new HashSet<string>();

            foreach (string tagsJson in newsWithTags)
            {
                if (!string.IsNullOrWhiteSpace(tagsJson))
                {
                    try
                    {
                        List<string>? tags = System.Text.Json.JsonSerializer.Deserialize<List<string>>(tagsJson);
                        if (tags != null)
                        {
                            foreach (string tag in tags)
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
            try
            {
                if (news == null)
                {
                    _logger.LogError("SaveAsync called with null news article");
                    throw new ArgumentNullException(nameof(news));
                }

                bool exists = await ExistsAsync(news.Id, cancellationToken);
                
                if (exists)
                {
                    _entities.Update(news);
                    _logger.LogDebug("Updated existing news article with ID: {NewsId}", news.Id);
                }
                else
                {
                    await _entities.AddAsync(news, cancellationToken);
                    _logger.LogDebug("Added new news article with ID: {NewsId}", news.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving news article with ID: {NewsId}", news?.Id);
                throw;
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
            try
            {
                if (news == null)
                {
                    _logger.LogError("CreateNews called with null news article");
                    throw new ArgumentNullException(nameof(news));
                }

                await _entities.AddAsync(news);
                _logger.LogDebug("Created news article with ID: {NewsId}", news.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating news article with ID: {NewsId}", news?.Id);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing news item
        /// </summary>
        /// <param name="news">The news to update</param>
        /// <returns>Task representing the async operation</returns>
        public Task UpdateNews(NewsArticle news)
        {
            try
            {
                if (news == null)
                {
                    _logger.LogError("UpdateNews called with null news article");
                    throw new ArgumentNullException(nameof(news));
                }

                _entities.Update(news);
                _logger.LogDebug("Updated news article with ID: {NewsId}", news.Id);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating news article with ID: {NewsId}", news?.Id);
                throw;
            }
        }

        public async Task<NewsArticle?> GetMainNews()
        {
            NewsArticle? news = await _entities.OrderByDescending(n => n.CreatedAt).FirstOrDefaultAsync();

            if(news == null)
            {
                _logger.LogWarning("No news articles found when trying to fetch the newest news.");
                return null;
            }

            return news;
        }

        /// <summary>
        /// Deletes news by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteNews(Guid id)
        {
            NewsArticle? news = await _entities.FindAsync(id);
            if(news == null)
            {
                _logger.LogWarning("No news articles found when trying to fetch the newest news.");
                return false;
            }
            _entities.Remove(news);
            return true;
        }
    }
} 
