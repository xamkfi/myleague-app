// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Domain.Entities.Common;

namespace Domain.Repositories.Common;

public interface IInfoPageContentRepository
{
    Task<InfoPageContent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InfoPageContent?> GetBySlugAsync(string pageSlug, CancellationToken cancellationToken = default);
    Task<IEnumerable<InfoPageContent>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlugAsync(string pageSlug, CancellationToken cancellationToken = default);
    InfoPageContent Add(InfoPageContent infoPageContent);
    InfoPageContent Update(InfoPageContent infoPageContent);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
