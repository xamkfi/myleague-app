// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Domain.Entities.Common;

namespace Domain.Repositories.Common;

/// <summary>
/// Repository interface for InfoPageContent entities
/// </summary>
public interface IInfoPageContentRepository
{
    /// <summary>
    /// Gets info page content by ID
    /// </summary>
    /// <param name="id">The content ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The content if found, null otherwise</returns>
    Task<InfoPageContent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets info page content by slug
    /// </summary>
    /// <param name="pageSlug">The page slug</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The content if found, null otherwise</returns>
    Task<InfoPageContent?> GetBySlugAsync(string pageSlug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all info page contents
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All info page contents</returns>
    Task<IEnumerable<InfoPageContent>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether info page content exists for the given slug
    /// </summary>
    /// <param name="pageSlug">The page slug</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if content exists for the slug</returns>
    Task<bool> ExistsBySlugAsync(string pageSlug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new info page content entity
    /// </summary>
    /// <param name="infoPageContent">The entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(InfoPageContent infoPageContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing info page content entity
    /// </summary>
    /// <param name="infoPageContent">The entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateAsync(InfoPageContent infoPageContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes info page content by ID
    /// </summary>
    /// <param name="id">The content ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
