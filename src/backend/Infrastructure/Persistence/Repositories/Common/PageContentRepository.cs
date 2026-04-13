using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common
{

    /// <summary>
    /// Implementation of the page content repository
    /// </summary>
    public class PageContentRepository : RepositoryBase<PageContent, CommonDbContext>, IPageContentRepository
    {
        private readonly ILogger<PageContentRepository> _logger;

        public PageContentRepository(CommonDbContext dbContext, ILogger<PageContentRepository> logger) : base(dbContext)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets a page content by its slug. Returns null if not found or if the slug is null/empty.
        /// </summary>
        /// <param name="slug"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<PageContent?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                _logger.LogWarning("GetBySlugAsync called with null or empty slug");
                return null;
            }

            return await _entities.FirstOrDefaultAsync(p => p.PageSlug == slug, cancellationToken);
        }

        /// <summary>
        /// Gets a page content by its ID. Returns null if not found.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<PageContent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _entities.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        /// <summary>
        /// Saves a new or existing page content. If the page content has an ID that already exists, it will be updated. Otherwise, it will be added as a new entry.
        /// </summary>
        /// <param name="pageContent"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task SaveAsync(PageContent pageContent, CancellationToken cancellationToken = default)
        {
            if (pageContent == null)
            {
                _logger.LogError("SaveAsync called with null pageContent");
                throw new ArgumentNullException(nameof(pageContent));
            }

            bool exists = await ExistsAsync(pageContent.Id, cancellationToken);
            if (exists)
            {
                _entities.Update(pageContent);
                _logger.LogDebug("Updated PageContent with ID: {Id}", pageContent.Id);
            }
            else
            {
                await _entities.AddAsync(pageContent, cancellationToken);
                _logger.LogDebug("Added PageContent with ID: {Id}", pageContent.Id);
            }
        }

        /// <summary>
        ///  Checks if a page content exists by its ID. Returns false if not found.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _entities.AnyAsync(p => p.Id == id, cancellationToken);
        }


        /// <summary>
        /// Checks if a page content exists by its slug. Returns false if not found or if the slug is null/empty.
        /// </summary>
        /// <param name="slug"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;
            return await _entities.AnyAsync(p => p.PageSlug == slug, cancellationToken);
        }
    }
}
