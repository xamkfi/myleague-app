using Domain.Entities.Hockey.Teams;
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
        return await TeamQuery()
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<HockeyTeam>> GetByClubIdAsync(Guid clubId)
    {
        return await TeamQuery()
            .Where(t => t.ClubId == clubId)
            .OrderBy(t => t.Name)
            .ToListAsync();
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
            .Include(t => t.Roster)
            .Include(t => t.Lines)
                .ThenInclude(l => l.Players)
            .Include(t => t.StaffMembers);
    }
}
