using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common;

/// <summary>
/// Implementation of the rules section repository
/// </summary>
public class RulesSectionRepository
    : RepositoryBase<RulesSection, CommonDbContext>, IRulesSectionRepository
{
    /// <summary>
    /// Initializes a new instance of the RulesSectionRepository class
    /// </summary>
    /// <param name="dbContext">The database context</param>
    public RulesSectionRepository(CommonDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<RulesSection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _entities.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RulesSection>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _entities
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsBySectionTypeAsync(
        RulesSectionType sectionType,
        CancellationToken cancellationToken = default)
    {
        return await _entities.AnyAsync(x => x.SectionType == sectionType, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasChildSectionsAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _entities.AnyAsync(x => x.ParentSectionId == parentId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(RulesSection section, CancellationToken cancellationToken = default)
    {
        await _entities.AddAsync(section, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(RulesSection section, CancellationToken cancellationToken = default)
    {
        _entities.Update(section);
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task RemoveAsync(RulesSection section, CancellationToken cancellationToken = default)
    {
        _entities.Remove(section);
        await Task.CompletedTask;
    }
}
