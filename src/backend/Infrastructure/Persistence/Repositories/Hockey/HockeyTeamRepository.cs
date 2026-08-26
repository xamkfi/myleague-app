using Domain.Common;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Common;
using Domain.Repositories.Hockey;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Hockey;

/// <summary>
/// EF Core repository for hockey teams.
/// </summary>
public class HockeyTeamRepository : IHockeyTeamRepository
{
    private readonly HockeyDbContext _dbContext;

    public HockeyTeamRepository(HockeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(HockeyTeam team)
    {
        await _dbContext.HockeyTeams.AddAsync(team);
    }

    public async Task<HockeyTeam?> GetByIdAsync(Guid id)
    {
        return await TeamQuery()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IReadOnlyList<HockeyTeam>> GetAllAsync()
    {
        List<HockeyTeam> teams = await TeamQuery()
            .OrderBy(t => t.Name)
            .ToListAsync();
        return DistinctById(teams);
    }

    public async Task<IReadOnlyList<HockeyTeam>> GetByClubIdAsync(Guid clubId)
    {
        List<HockeyTeam> teams = await TeamQuery()
            .Where(t => t.ClubId == clubId)
            .OrderBy(t => t.Name)
            .ToListAsync();
        return DistinctById(teams);
    }

    public async Task<IReadOnlyList<HockeyTeam>> GetByPlayerIdAsync(Guid playerId)
    {
        List<HockeyTeam> teams = await TeamQuery()
            .Where(t => t.Roster.Any(p => p.PlayerId == playerId && p.LeftAt == null))
            .OrderBy(t => t.Name)
            .ToListAsync();
        return DistinctById(teams);
    }

    public async Task<PagedResult<HockeyTeam>> GetPagedAsync(
        int page,
        int pageSize,
        string searchTerm = "",
        Guid? clubId = null,
        TeamCategory? teamCategory = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<HockeyTeam> query = _dbContext.HockeyTeams.AsQueryable();

        if (clubId.HasValue)
        {
            query = query.Where(t => t.ClubId == clubId.Value);
        }

        if (teamCategory.HasValue)
        {
            query = query.Where(t => t.TeamCategory == teamCategory.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string loweredSearchTerm = searchTerm.ToLower();
            query = query.Where(t =>
                t.Name.ToLower().Contains(loweredSearchTerm)
                || t.ShortName.ToLower().Contains(loweredSearchTerm));
        }

        query = query.OrderBy(t => t.Name);
        int totalCount = await query.CountAsync(cancellationToken);
        List<HockeyTeam> items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult.Create(items, totalCount, page, pageSize);
    }

    public async Task<bool> HasAnyForClubAsync(Guid clubId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.HockeyTeams.AnyAsync(t => t.ClubId == clubId, cancellationToken);
    }

    public async Task<bool> HasAnyForDivisionAsync(Guid divisionId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.HockeyTeams.AnyAsync(t => t.DivisionId == divisionId, cancellationToken);
    }

    private IQueryable<HockeyTeam> TeamQuery()
    {
        return _dbContext.HockeyTeams
            .AsSplitQuery()
            .Include(t => t.Roster)
            .Include(t => t.Lines)
                .ThenInclude(l => l.Players)
            .Include(t => t.StaffMembers);
    }

    private static IReadOnlyList<HockeyTeam> DistinctById(List<HockeyTeam> teams)
    {
        return teams.DistinctBy(team => team.Id).ToList();
    }
}
