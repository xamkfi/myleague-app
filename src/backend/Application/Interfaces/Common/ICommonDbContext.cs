using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces.Common;

public interface ICommonDbContext
{
    DbSet<InfoPageContent> InfoPageContents { get; }
    DbSet<RulesSection> RulesSections { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

