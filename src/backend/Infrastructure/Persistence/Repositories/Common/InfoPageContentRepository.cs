// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common;

/// <summary>
/// Implementation of the info page content repository
/// </summary>
public class InfoPageContentRepository
    : RepositoryBase<InfoPageContent, CommonDbContext>, IInfoPageContentRepository
{
    /// <summary>
    /// Initializes a new instance of the InfoPageContentRepository class
    /// </summary>
    /// <param name="dbContext">The database context</param>
    public InfoPageContentRepository(CommonDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<InfoPageContent?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _entities.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<InfoPageContent?> GetBySlugAsync(
        string pageSlug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pageSlug))
        {
            return null;
        }

        return await _entities.FirstOrDefaultAsync(
            p => p.PageSlug == pageSlug,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<InfoPageContent>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _entities
            .OrderBy(p => p.PageSlug)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsBySlugAsync(
        string pageSlug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pageSlug))
        {
            return false;
        }

        return await _entities.AnyAsync(p => p.PageSlug == pageSlug, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(InfoPageContent infoPageContent, CancellationToken cancellationToken = default)
    {
        await _entities.AddAsync(infoPageContent, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(InfoPageContent infoPageContent, CancellationToken cancellationToken = default)
    {
        _entities.Update(infoPageContent);
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        InfoPageContent? infoPageContent = await GetByIdAsync(id, cancellationToken);

        if (infoPageContent == null)
        {
            return false;
        }

        _entities.Remove(infoPageContent);
        return true;
    }
}
