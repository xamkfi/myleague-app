using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces.Common;

public interface ICommonDbContext
{
    DbSet<PageContent> PageContents { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

