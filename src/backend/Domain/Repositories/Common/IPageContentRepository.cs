// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Domain.Entities.Common;

namespace Domain.Repositories.Common;

/// <summary>
/// Repository for managing static page content
/// </summary>
public interface IPageContentRepository
{
    /// <summary>
    /// Gets a page content by its ID.
    /// </summary>
    /// <param name="id">The page content ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The page content if found, null otherwise.</returns>
    Task<PageContent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a page content by its unique slug identifier.
    /// </summary>
    /// <param name="pageSlug">The page slug (e.g., "terms-of-service").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The page content if found, null otherwise.</returns>
    Task<PageContent?> GetBySlugAsync(string pageSlug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all page contents.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of all page contents.</returns>
    Task<IEnumerable<PageContent>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a page content exists by slug.
    /// </summary>
    /// <param name="pageSlug">The page slug to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the page content exists, false otherwise.</returns>
    Task<bool> ExistsBySlugAsync(string pageSlug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new page content.
    /// </summary>
    /// <param name="pageContent">The page content to add.</param>
    /// <returns>The added page content.</returns>
    PageContent Add(PageContent pageContent);

    /// <summary>
    /// Updates an existing page content.
    /// </summary>
    /// <param name="pageContent">The page content to update.</param>
    /// <returns>The updated page content.</returns>
    PageContent Update(PageContent pageContent);

    /// <summary>
    /// Deletes a page content by ID.
    /// </summary>
    /// <param name="id">The ID of the page content to delete.</param>
    /// <returns>True if the page content was deleted, false if not found.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
