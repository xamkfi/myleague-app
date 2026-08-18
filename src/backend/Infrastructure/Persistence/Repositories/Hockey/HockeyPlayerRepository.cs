using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Hockey;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Hockey;

/// <summary>
/// EF Core repository for hockey players.
/// </summary>
public class HockeyPlayerRepository : IHockeyPlayerRepository
{
    private readonly HockeyDbContext _dbContext;

    public HockeyPlayerRepository(HockeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(HockeyPlayer player)
    {
        await _dbContext.HockeyPlayers.AddAsync(player);
    }

    public async Task<HockeyPlayer?> GetByIdAsync(Guid id)
    {
        return await _dbContext.HockeyPlayers
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<HockeyPlayer?> GetByPersonIdAsync(Guid personId)
    {
        return await _dbContext.HockeyPlayers
            .FirstOrDefaultAsync(p => p.PersonId == personId);
    }
}
