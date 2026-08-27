using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common;

/// <summary>
/// Implementation of the season content block repository
/// </summary>
public class SeasonContentBlockRepository
    : RepositoryBase<SeasonContentBlock, CommonDbContext>, ISeasonContentBlockRepository
{
    /// <summary>
    /// Initializes a new instance of the SeasonContentBlockRepository class
    /// </summary>
    public SeasonContentBlockRepository(CommonDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<SeasonContentBlock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _entities.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SeasonContentBlock>> GetByCompetitionIdAsync(
        Guid competitionId,
        CancellationToken cancellationToken = default)
    {
        return await _entities
            .AsNoTracking()
            .Where(x => x.CompetitionId == competitionId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SeasonContentBlock>> GetBySportAndSeasonYearAsync(
        SportsCategory sport,
        string seasonYear,
        CancellationToken cancellationToken = default)
    {
        return await _entities
            .AsNoTracking()
            .Where(x => x.Sport == sport && x.SeasonYear == seasonYear)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(SeasonContentBlock block, CancellationToken cancellationToken = default)
    {
        await _entities.AddAsync(block, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(SeasonContentBlock block, CancellationToken cancellationToken = default)
    {
        _entities.Update(block);
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task RemoveAsync(SeasonContentBlock block, CancellationToken cancellationToken = default)
    {
        _entities.Remove(block);
        await Task.CompletedTask;
    }
}
