using Domain.Common;
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

    public async Task<PagedResult<HockeyOfficial>> GetPagedAsync(
        int page,
        int pageSize,
        bool? isActive = null,
        string? searchTerm = null,
        int? licenseExpiringWithinDays = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<HockeyOfficial> query = _dbContext.HockeyOfficials.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(o => o.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string loweredSearchTerm = searchTerm.ToLower();
            query = query.Where(o =>
                o.OfficialNumber != null && o.OfficialNumber.ToLower().Contains(loweredSearchTerm));
        }

        if (licenseExpiringWithinDays.HasValue)
        {
            DateTime cutoffDate = DateTime.UtcNow.AddDays(licenseExpiringWithinDays.Value);
            query = query.Where(o => o.LicenseExpiryDate <= cutoffDate && o.IsActive);
        }

        query = query
            .OrderBy(o => o.LicenseExpiryDate ?? DateTime.MaxValue)
            .ThenByDescending(o => o.MatchesOfficiated);

        int totalCount = await query.CountAsync(cancellationToken);
        List<HockeyOfficial> items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult.Create(items, totalCount, page, pageSize);
    }
}
