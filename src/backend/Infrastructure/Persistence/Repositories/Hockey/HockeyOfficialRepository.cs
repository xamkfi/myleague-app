using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Hockey;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Hockey;

/// <summary>
/// EF Core repository for hockey officials.
/// </summary>
public class HockeyOfficialRepository : IHockeyOfficialRepository
{
    private readonly HockeyDbContext _dbContext;

    public HockeyOfficialRepository(HockeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(HockeyOfficial official)
    {
        await _dbContext.HockeyOfficials.AddAsync(official);
    }

    public async Task<HockeyOfficial?> GetByIdAsync(Guid id)
    {
        return await _dbContext.HockeyOfficials
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<HockeyOfficial?> GetByPersonIdAsync(Guid personId)
    {
        return await _dbContext.HockeyOfficials
            .FirstOrDefaultAsync(o => o.PersonId == personId);
    }

    public async Task<IReadOnlyList<HockeyOfficial>> GetAllAsync(bool? isActive = null)
    {
        IQueryable<HockeyOfficial> query = _dbContext.HockeyOfficials.AsQueryable();
        if (isActive.HasValue)
        {
            query = query.Where(o => o.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(o => o.OfficialRole)
            .ThenBy(o => o.OfficialNumber)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbContext.HockeyOfficials.AnyAsync(o => o.Id == id);
    }
}
