// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common;

/// <summary>
/// Implementation of the page content repository.
/// </summary>
public class PageContentRepository : RepositoryBase<PageContent, CommonDbContext>, IPageContentRepository
{
    /// <summary>
    /// Initializes a new instance of the PageContentRepository class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public PageContentRepository(CommonDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Gets a page content by its ID.
    /// </summary>
    /// <param name="id">The page content ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The page content if found, null otherwise.</returns>
    public override async Task<PageContent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _entities
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <summary>
    /// Gets a page content by its unique slug identifier.
    /// </summary>
    /// <param name="pageSlug">The page slug (e.g., "terms-of-service").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The page content if found, null otherwise.</returns>
    public async Task<PageContent?> GetBySlugAsync(string pageSlug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pageSlug))
            return null;

        return await _entities
            .FirstOrDefaultAsync(p => p.PageSlug == pageSlug, cancellationToken);
    }

    /// <summary>
    /// Gets all page contents.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of all page contents.</returns>
    public async Task<IEnumerable<PageContent>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _entities
            .OrderBy(p => p.PageSlug)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a page content exists by slug.
    /// </summary>
    /// <param name="pageSlug">The page slug to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the page content exists, false otherwise.</returns>
    public async Task<bool> ExistsBySlugAsync(string pageSlug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pageSlug))
            return false;

        return await _entities
            .AnyAsync(p => p.PageSlug == pageSlug, cancellationToken);
    }

    /// <summary>
    /// Adds a new page content.
    /// </summary>
    /// <param name="pageContent">The page content to add.</param>
    /// <returns>The added page content.</returns>
    public PageContent Add(PageContent pageContent)
    {
        return _entities.Add(pageContent).Entity;
    }

    /// <summary>
    /// Updates an existing page content.
    /// </summary>
    /// <param name="pageContent">The page content to update.</param>
    /// <returns>The updated page content.</returns>
    public PageContent Update(PageContent pageContent)
    {
        return _entities.Update(pageContent).Entity;
    }

    /// <summary>
    /// Deletes a page content by ID.
    /// </summary>
    /// <param name="id">The ID of the page content to delete.</param>
    /// <returns>True if the page content was deleted, false if not found.</returns>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pageContent = await GetByIdAsync(id, cancellationToken);
        if (pageContent == null)
            return false;

        _entities.Remove(pageContent);
        return true;
    }
}
