using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Repositories.Hockey;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Hockey;

/// <summary>
/// EF Core repository for hockey matches.
/// </summary>
public class HockeyMatchRepository : IHockeyMatchRepository
{
    private readonly HockeyDbContext _dbContext;

    public HockeyMatchRepository(HockeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(HockeyMatch match)
    {
        await _dbContext.HockeyMatches.AddAsync(match);
    }

    public async Task<HockeyMatch?> GetByIdAsync(Guid id)
    {
        return await BuildDetailQuery().FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<HockeyMatch?> GetByIdForStatisticsAsync(Guid id)
    {
        return await BuildStatisticsQuery().FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IReadOnlyList<HockeyMatch>> GetByCompetitionIdForStatisticsAsync(Guid competitionId)
    {
        return await BuildStatisticsQuery()
            .Where(m => m.CompetitionId == competitionId)
            .ToListAsync();
    }

    public void MarkEventAsAdded(HockeyMatchEvent matchEvent)
    {
        _dbContext.Entry(matchEvent).State = EntityState.Added;
    }

    private IQueryable<HockeyMatch> BuildDetailQuery() =>
        _dbContext.HockeyMatches
            .Include(m => m.MatchTeams)
                .ThenInclude(t => t.PlayerSelection!)
                    .ThenInclude(s => s.ActivePlayers)
            .Include(m => m.MatchTeams)
                .ThenInclude(t => t.Lines)
                    .ThenInclude(l => l.Players)
            .Include(m => m.MatchTeams)
                .ThenInclude(t => t.OnIceState!)
                    .ThenInclude(s => s.PlayersOnIce)
            .Include(m => m.Events)
            .Include(m => m.Officials)
            .Include(m => m.PeriodScores);

    private IQueryable<HockeyMatch> BuildStatisticsQuery() =>
        _dbContext.HockeyMatches
            .Include(m => m.MatchTeams)
                .ThenInclude(t => t.PlayerSelection!)
                    .ThenInclude(s => s.ActivePlayers)
            .Include(m => m.MatchTeams)
                .ThenInclude(t => t.OnIceState!)
                    .ThenInclude(s => s.ChangeLog)
            .Include(m => m.Events);
}
