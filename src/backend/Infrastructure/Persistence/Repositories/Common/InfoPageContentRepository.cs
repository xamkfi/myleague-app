// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common;

public class InfoPageContentRepository
    : RepositoryBase<InfoPageContent, CommonDbContext>, IInfoPageContentRepository
{
    public InfoPageContentRepository(CommonDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<InfoPageContent?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _entities.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

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

    public async Task<IEnumerable<InfoPageContent>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _entities
            .OrderBy(p => p.PageSlug)
            .ToListAsync(cancellationToken);
    }

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

    public InfoPageContent Add(InfoPageContent infoPageContent)
    {
        return _entities.Add(infoPageContent).Entity;
    }

    public InfoPageContent Update(InfoPageContent infoPageContent)
    {
        return _entities.Update(infoPageContent).Entity;
    }

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
