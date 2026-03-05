using Domain.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MyLeague.Infrastructure.Persistence.Repositories.Floorball;

/// <summary>
/// Implementation of the floorball statistics repository
/// </summary>
public class FloorballStatisticsRepository : IFloorballStatisticsRepository
{
    private readonly FloorballDbContext _context;

    /// <summary>
    /// Initializes a new instance of the FloorballStatisticsRepository class
    /// </summary>
    /// <param name="context">The database context</param>
    public FloorballStatisticsRepository(FloorballDbContext context)
    {
        _context = context;
    }

    #region Team Season Statistics

    /// <inheritdoc />
    public async Task<FloorballTeamSeasonStatistics?> GetTeamSeasonStatisticsAsync(Guid teamId, Guid seasonId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballTeamSeasonStatistics.Include(x => x.Team).Include(x => x.Season)
            .FirstOrDefaultAsync(s => s.TeamId == teamId && s.SeasonId == seasonId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FloorballTeamSeasonStatistics>> GetTeamStatisticsBySeasonAsync(Guid seasonId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballTeamSeasonStatistics.Include(x => x.Season).Include(x => x.Team)
            .Where(s => s.SeasonId == seasonId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FloorballTeamSeasonStatistics>> GetTeamStandingsAsync(Guid seasonId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballTeamSeasonStatistics
            .Include(x => x.Team)
            .Include(x => x.Season)
            .Where(s => s.SeasonId == seasonId)
            .OrderByDescending(s => s.Points)
            .ThenByDescending(s => s.GoalDifference)
            .ThenByDescending(s => s.GoalsFor)
            .ThenBy(s => s.GoalsAgainst)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveTeamSeasonStatisticsAsync(FloorballTeamSeasonStatistics statistics, CancellationToken cancellationToken = default)
    {
        FloorballTeamSeasonStatistics? existing = await GetTeamSeasonStatisticsAsync(statistics.TeamId, statistics.SeasonId, cancellationToken);

        if (existing == null)
        {
            await _context.FloorballTeamSeasonStatistics.AddAsync(statistics, cancellationToken);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(statistics);
            _context.Entry(existing).Property(e => e.UpdatedAt).CurrentValue = DateTime.UtcNow;
        }
        // SaveChanges is deferred to the UnitOfWork
    }

    #endregion

    #region Player Season Statistics

    /// <inheritdoc />
    public async Task<FloorballPlayerSeasonStatistics?> GetPlayerSeasonStatisticsAsync(Guid playerId, Guid teamId, Guid seasonId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballPlayerSeasonStatistics
            .FirstOrDefaultAsync(s => s.PlayerId == playerId && s.TeamId == teamId && s.SeasonId == seasonId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FloorballPlayerSeasonStatistics>> GetPlayerStatisticsBySeasonAsync(Guid seasonId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballPlayerSeasonStatistics.Include(x => x.Player).Include(x => x.Team).Include(x => x.Season)
            .Where(s => s.SeasonId == seasonId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FloorballPlayerSeasonStatistics>> GetPlayerStatisticsByTeamAndSeasonAsync(Guid teamId, Guid seasonId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballPlayerSeasonStatistics
            .Include(x => x.Player)
            .Include(x => x.Team)
            .Include(x => x.Season)
            .Where(s => s.TeamId == teamId && s.SeasonId == seasonId)
            .OrderByDescending(s => s.Points)
            .ThenByDescending(s => s.Goals)
            .ThenByDescending(s => s.Assists)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FloorballPlayerSeasonStatistics>> GetTopScorersAsync(Guid seasonId, int topN, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballPlayerSeasonStatistics.Include(x => x.Player).Include(x => x.Team).Include(x => x.Season)
            .Where(s => s.SeasonId == seasonId)
            .OrderByDescending(s => s.Goals)
            .ThenByDescending(s => s.Points)
            .ThenByDescending(s => s.Assists)
            .ThenBy(s => s.GamesPlayed)
            .Take(topN)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FloorballPlayerSeasonStatistics>> GetTopAssistsAsync(Guid seasonId, int topN, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballPlayerSeasonStatistics.Include(x => x.Player).Include(x => x.Team).Include(x => x.Season)
            .Where(s => s.SeasonId == seasonId)
            .OrderByDescending(s => s.Assists)
            .ThenByDescending(s => s.Points)
            .ThenByDescending(s => s.Goals)
            .ThenBy(s => s.GamesPlayed)
            .Take(topN)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FloorballPlayerSeasonStatistics>> GetPlayerCareerStatisticsAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballPlayerSeasonStatistics.Include(x => x.Player).Include(x => x.Season).Include(x => x.Team)
            .Where(s => s.PlayerId == playerId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SavePlayerSeasonStatisticsAsync(FloorballPlayerSeasonStatistics statistics, CancellationToken cancellationToken = default)
    {
        FloorballPlayerSeasonStatistics? existing = await GetPlayerSeasonStatisticsAsync(statistics.PlayerId, statistics.TeamId, statistics.SeasonId, cancellationToken);

        if (existing == null)
        {
            await _context.FloorballPlayerSeasonStatistics.AddAsync(statistics, cancellationToken);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(statistics);
            _context.Entry(existing).Property(e => e.UpdatedAt).CurrentValue = DateTime.UtcNow;
        }
        // SaveChanges is deferred to the UnitOfWork
    }

    #endregion

    #region Goalie Season Statistics

    /// <inheritdoc />
    public async Task<FloorballGoalieSeasonStatistics?> GetGoalieSeasonStatisticsAsync(Guid playerId, Guid teamId, Guid seasonId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballGoalieSeasonStatistics
            .FirstOrDefaultAsync(s => s.PlayerId == playerId && s.TeamId == teamId && s.SeasonId == seasonId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FloorballGoalieSeasonStatistics>> GetGoalieStatisticsBySeasonAsync(Guid seasonId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballGoalieSeasonStatistics.Include(x => x.Player).Include(x => x.Team).Include(x => x.Season)
            .Where(s => s.SeasonId == seasonId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FloorballGoalieSeasonStatistics>> GetTopGoaliesAsync(Guid seasonId, int topN, int minimumGames, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballGoalieSeasonStatistics
            .Include(x => x.Player)
            .Include(x => x.Team)
            .Include(x => x.Season)
            .Where(s => s.SeasonId == seasonId && s.GamesPlayed >= minimumGames)
            .OrderByDescending(s => s.SavePercentage)
            .ThenBy(s => s.GoalsAgainstAverage)
            .ThenByDescending(s => s.Wins)
            .ThenByDescending(s => s.Shutouts)
            .Take(topN)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FloorballGoalieSeasonStatistics>> GetGoalieCareerStatisticsAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballGoalieSeasonStatistics.Include(x => x.Team).Include(x => x.Season).Include(x => x.Player)
            .Where(s => s.PlayerId == playerId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveGoalieSeasonStatisticsAsync(FloorballGoalieSeasonStatistics statistics, CancellationToken cancellationToken = default)
    {
        FloorballGoalieSeasonStatistics? existing = await GetGoalieSeasonStatisticsAsync(statistics.PlayerId, statistics.TeamId, statistics.SeasonId, cancellationToken);

        if (existing == null)
        {
            await _context.FloorballGoalieSeasonStatistics.AddAsync(statistics, cancellationToken);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(statistics);
            _context.Entry(existing).Property(e => e.UpdatedAt).CurrentValue = DateTime.UtcNow;
        }
        // SaveChanges is deferred to the UnitOfWork
    }

    #endregion

    #region Match Team Statistics

    /// <inheritdoc />
    public async Task<FloorballMatchTeamStatistics?> GetMatchTeamStatisticsAsync(Guid matchId, Guid teamId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballMatchTeamStatistics
            .FirstOrDefaultAsync(s => s.MatchId == matchId && s.TeamId == teamId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FloorballMatchTeamStatistics>> GetMatchStatisticsAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballMatchTeamStatistics
            .Where(s => s.MatchId == matchId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveMatchTeamStatisticsAsync(FloorballMatchTeamStatistics statistics, CancellationToken cancellationToken = default)
    {
        FloorballMatchTeamStatistics? existing = await GetMatchTeamStatisticsAsync(statistics.MatchId, statistics.TeamId, cancellationToken);
        
        if (existing == null)
        {
            await _context.FloorballMatchTeamStatistics.AddAsync(statistics, cancellationToken);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(statistics);
        }
        // SaveChanges is deferred to the UnitOfWork
    }

    #endregion

    #region Statistics Cache

    /// <inheritdoc />
    public async Task<FloorballStatisticsCache?> GetCachedStatisticsAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballStatisticsCache
            .FirstOrDefaultAsync(c => c.CacheKey == cacheKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveCachedStatisticsAsync(FloorballStatisticsCache cache, CancellationToken cancellationToken = default)
    {
        FloorballStatisticsCache? existing = await GetCachedStatisticsAsync(cache.CacheKey, cancellationToken);
        
        if (existing == null)
        {
            await _context.FloorballStatisticsCache.AddAsync(cache, cancellationToken);
        }
        else
        {
            existing.UpdateData(cache.JsonData, 
                (int)(cache.ExpiresAt - DateTime.UtcNow).TotalMinutes);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> RemoveExpiredCacheAsync(CancellationToken cancellationToken = default)
    {
        List<FloorballStatisticsCache> expiredEntries = await _context.FloorballStatisticsCache
            .Where(c => c.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(cancellationToken);
        
        if (expiredEntries.Any())
        {
            _context.FloorballStatisticsCache.RemoveRange(expiredEntries);
            await _context.SaveChangesAsync(cancellationToken);
        }
        
        return expiredEntries.Count;
    }

    /// <inheritdoc />
    public async Task RemoveSeasonCacheAsync(Guid seasonId, CancellationToken cancellationToken = default)
    {
        List<FloorballStatisticsCache> seasonCacheEntries = await _context.FloorballStatisticsCache
            .Where(c => c.SeasonId == seasonId)
            .ToListAsync(cancellationToken);
        
        if (seasonCacheEntries.Any())
        {
            _context.FloorballStatisticsCache.RemoveRange(seasonCacheEntries);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    #endregion

    #region Bulk Operations

    /// <inheritdoc />
    public async Task SaveTeamSeasonStatisticsBatchAsync(IEnumerable<FloorballTeamSeasonStatistics> statistics, CancellationToken cancellationToken = default)
    {
        var statsToProcess = statistics.ToList();
        
        foreach (FloorballTeamSeasonStatistics? stat in statsToProcess)
        {
            FloorballTeamSeasonStatistics? existing = await GetTeamSeasonStatisticsAsync(stat.TeamId, stat.SeasonId, cancellationToken);
            
            if (existing == null)
            {
                await _context.FloorballTeamSeasonStatistics.AddAsync(stat, cancellationToken);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(stat);
            }
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SavePlayerSeasonStatisticsBatchAsync(IEnumerable<FloorballPlayerSeasonStatistics> statistics, CancellationToken cancellationToken = default)
    {
        var statsToProcess = statistics.ToList();
        
        foreach (FloorballPlayerSeasonStatistics? stat in statsToProcess)
        {
            FloorballPlayerSeasonStatistics? existing = await GetPlayerSeasonStatisticsAsync(stat.PlayerId, stat.TeamId, stat.SeasonId, cancellationToken);
            
            if (existing == null)
            {
                await _context.FloorballPlayerSeasonStatistics.AddAsync(stat, cancellationToken);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(stat);
            }
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveGoalieSeasonStatisticsBatchAsync(IEnumerable<FloorballGoalieSeasonStatistics> statistics, CancellationToken cancellationToken = default)
    {
        var statsToProcess = statistics.ToList();
        
        foreach (FloorballGoalieSeasonStatistics? stat in statsToProcess)
        {
            FloorballGoalieSeasonStatistics? existing = await GetGoalieSeasonStatisticsAsync(stat.PlayerId, stat.TeamId, stat.SeasonId, cancellationToken);
            
            if (existing == null)
            {
                await _context.FloorballGoalieSeasonStatistics.AddAsync(stat, cancellationToken);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(stat);
            }
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task ResetSeasonStatisticsAsync(Guid seasonId, CancellationToken cancellationToken = default)
    {
        // Remove existing team statistics
        List<FloorballTeamSeasonStatistics> teamStats = await _context.FloorballTeamSeasonStatistics
            .Where(s => s.SeasonId == seasonId)
            .ToListAsync(cancellationToken);
        _context.FloorballTeamSeasonStatistics.RemoveRange(teamStats);

        // Remove existing player statistics
        List<FloorballPlayerSeasonStatistics> playerStats = await _context.FloorballPlayerSeasonStatistics
            .Where(s => s.SeasonId == seasonId)
            .ToListAsync(cancellationToken);
        _context.FloorballPlayerSeasonStatistics.RemoveRange(playerStats);

        // Remove existing goalie statistics
        List<FloorballGoalieSeasonStatistics> goalieStats = await _context.FloorballGoalieSeasonStatistics
            .Where(s => s.SeasonId == seasonId)
            .ToListAsync(cancellationToken);
        _context.FloorballGoalieSeasonStatistics.RemoveRange(goalieStats);
        
        // Remove season cache
        await RemoveSeasonCacheAsync(seasonId, cancellationToken);
        
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion
}
