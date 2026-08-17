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
    public async Task<FloorballTeamSeasonStatistics?> GetTeamSeasonStatisticsAsync(Guid teamId, Guid competitionId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballTeamSeasonStatistics.Include(x => x.Team).Include(x => x.Competition)
            .FirstOrDefaultAsync(s => s.TeamId == teamId && s.CompetitionId == competitionId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FloorballTeamSeasonStatistics>> GetTeamStatisticsByCompetitionAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballTeamSeasonStatistics.Include(x => x.Competition).Include(x => x.Team)
            .Where(s => s.CompetitionId == competitionId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FloorballTeamSeasonStatistics>> GetTeamStandingsAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballTeamSeasonStatistics
            .Include(x => x.Team)
            .Include(x => x.Competition)
            .Where(s => s.CompetitionId == competitionId)
            .OrderByDescending(s => s.Points)
            .ThenByDescending(s => s.GoalDifference)
            .ThenByDescending(s => s.GoalsFor)
            .ThenBy(s => s.GoalsAgainst)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FloorballTeamSeasonStatistics>> GetTeamSeasonStatisticsForTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballTeamSeasonStatistics
            .Include(x => x.Team)
            .Include(x => x.Competition)
            .Where(s => s.TeamId == teamId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveTeamSeasonStatisticsAsync(FloorballTeamSeasonStatistics statistics, CancellationToken cancellationToken = default)
    {
        FloorballTeamSeasonStatistics? existing = await GetTeamSeasonStatisticsAsync(statistics.TeamId, statistics.CompetitionId, cancellationToken);

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
    public async Task<FloorballPlayerSeasonStatistics?> GetPlayerSeasonStatisticsAsync(Guid playerId, Guid teamId, Guid competitionId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballPlayerSeasonStatistics
            .FirstOrDefaultAsync(s => s.PlayerId == playerId && s.TeamId == teamId && s.CompetitionId == competitionId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FloorballPlayerSeasonStatistics>> GetPlayerStatisticsByCompetitionAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballPlayerSeasonStatistics.Include(x => x.Player).Include(x => x.Team).Include(x => x.Competition)
            .Where(s => s.CompetitionId == competitionId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FloorballPlayerSeasonStatistics>> GetPlayerStatisticsByTeamAndCompetitionAsync(Guid teamId, Guid competitionId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballPlayerSeasonStatistics
            .Include(x => x.Player)
            .Include(x => x.Team)
            .Include(x => x.Competition)
            .Where(s => s.TeamId == teamId && s.CompetitionId == competitionId)
            .OrderByDescending(s => s.Points)
            .ThenByDescending(s => s.Goals)
            .ThenByDescending(s => s.Assists)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FloorballPlayerSeasonStatistics>> GetPlayerStatisticsForTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballPlayerSeasonStatistics
            .Include(x => x.Player)
            .Include(x => x.Team)
            .Include(x => x.Competition)
            .Where(s => s.TeamId == teamId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FloorballPlayerSeasonStatistics>> GetTopScorersAsync(Guid competitionId, int topN, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballPlayerSeasonStatistics.Include(x => x.Player).Include(x => x.Team).Include(x => x.Competition)
            .Where(s => s.CompetitionId == competitionId)
            .OrderByDescending(s => s.Goals)
            .ThenByDescending(s => s.Points)
            .ThenByDescending(s => s.Assists)
            .ThenBy(s => s.GamesPlayed)
            .Take(topN)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FloorballPlayerSeasonStatistics>> GetTopAssistsAsync(Guid competitionId, int topN, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballPlayerSeasonStatistics.Include(x => x.Player).Include(x => x.Team).Include(x => x.Competition)
            .Where(s => s.CompetitionId == competitionId)
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
        return await _context.FloorballPlayerSeasonStatistics.Include(x => x.Player).Include(x => x.Competition).Include(x => x.Team)
            .Where(s => s.PlayerId == playerId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SavePlayerSeasonStatisticsAsync(FloorballPlayerSeasonStatistics statistics, CancellationToken cancellationToken = default)
    {
        FloorballPlayerSeasonStatistics? existing = await GetPlayerSeasonStatisticsAsync(statistics.PlayerId, statistics.TeamId, statistics.CompetitionId, cancellationToken);

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
    public async Task<FloorballGoalieSeasonStatistics?> GetGoalieSeasonStatisticsAsync(Guid playerId, Guid teamId, Guid competitionId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballGoalieSeasonStatistics
            .FirstOrDefaultAsync(s => s.PlayerId == playerId && s.TeamId == teamId && s.CompetitionId == competitionId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FloorballGoalieSeasonStatistics>> GetGoalieStatisticsByCompetitionAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballGoalieSeasonStatistics.Include(x => x.Player).Include(x => x.Team).Include(x => x.Competition)
            .Where(s => s.CompetitionId == competitionId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FloorballGoalieSeasonStatistics>> GetTopGoaliesAsync(Guid competitionId, int topN, int minimumGames, CancellationToken cancellationToken = default)
    {
        return await _context.FloorballGoalieSeasonStatistics
            .Include(x => x.Player)
            .Include(x => x.Team)
            .Include(x => x.Competition)
            .Where(s => s.CompetitionId == competitionId && s.GamesPlayed >= minimumGames)
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
        return await _context.FloorballGoalieSeasonStatistics.Include(x => x.Team).Include(x => x.Competition).Include(x => x.Player)
            .Where(s => s.PlayerId == playerId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveGoalieSeasonStatisticsAsync(FloorballGoalieSeasonStatistics statistics, CancellationToken cancellationToken = default)
    {
        FloorballGoalieSeasonStatistics? existing = await GetGoalieSeasonStatisticsAsync(statistics.PlayerId, statistics.TeamId, statistics.CompetitionId, cancellationToken);

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
    public async Task RemoveCompetitionCacheAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        List<FloorballStatisticsCache> competitionCacheEntries = await _context.FloorballStatisticsCache
            .Where(c => c.CompetitionId == competitionId)
            .ToListAsync(cancellationToken);
        
        if (competitionCacheEntries.Any())
        {
            _context.FloorballStatisticsCache.RemoveRange(competitionCacheEntries);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    #endregion

    #region Bulk Operations

    /// <inheritdoc />
    public async Task SaveTeamSeasonStatisticsBatchAsync(IEnumerable<FloorballTeamSeasonStatistics> statistics, CancellationToken cancellationToken = default)
    {
        List<FloorballTeamSeasonStatistics> statsToProcess = statistics.ToList();
        
        foreach (FloorballTeamSeasonStatistics stat in statsToProcess)
        {
            FloorballTeamSeasonStatistics? existing = await GetTeamSeasonStatisticsAsync(stat.TeamId, stat.CompetitionId, cancellationToken);
            
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
        List<FloorballPlayerSeasonStatistics> statsToProcess = statistics.ToList();
        
        // Track keys added within this batch so duplicate roster entries don't cause duplicate inserts.
        HashSet<(Guid, Guid, Guid)> addedInBatch = [];

        foreach (FloorballPlayerSeasonStatistics stat in statsToProcess)
        {
            if (!addedInBatch.Add((stat.PlayerId, stat.TeamId, stat.CompetitionId)))
            {
                continue;
            }

            FloorballPlayerSeasonStatistics? existing = await GetPlayerSeasonStatisticsAsync(stat.PlayerId, stat.TeamId, stat.CompetitionId, cancellationToken);
            
            if (existing == null)
            {
                await _context.FloorballPlayerSeasonStatistics.AddAsync(stat, cancellationToken);
            }
            else if (!ReferenceEquals(existing, stat))
            {
                // The incoming row is a freshly initialized (zeroed) statistics entry created by
                // AddTeamToSeason; overwriting the tracked row would both clear accumulated stats
                // and attempt to modify the primary key. Keep the existing row untouched.
                continue;
            }
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveGoalieSeasonStatisticsBatchAsync(IEnumerable<FloorballGoalieSeasonStatistics> statistics, CancellationToken cancellationToken = default)
    {
        List<FloorballGoalieSeasonStatistics> statsToProcess = statistics.ToList();
        
        HashSet<(Guid, Guid, Guid)> addedInBatch = [];

        foreach (FloorballGoalieSeasonStatistics stat in statsToProcess)
        {
            if (!addedInBatch.Add((stat.PlayerId, stat.TeamId, stat.CompetitionId)))
            {
                continue;
            }

            FloorballGoalieSeasonStatistics? existing = await GetGoalieSeasonStatisticsAsync(stat.PlayerId, stat.TeamId, stat.CompetitionId, cancellationToken);
            
            if (existing == null)
            {
                await _context.FloorballGoalieSeasonStatistics.AddAsync(stat, cancellationToken);
            }
            else if (!ReferenceEquals(existing, stat))
            {
                // Keep the existing tracked row; overwriting it would modify the primary key.
                continue;
            }
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task ResetCompetitionStatisticsAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        List<FloorballTeamSeasonStatistics> teamStats = await _context.FloorballTeamSeasonStatistics
            .Where(s => s.CompetitionId == competitionId)
            .ToListAsync(cancellationToken);
        _context.FloorballTeamSeasonStatistics.RemoveRange(teamStats);

        List<FloorballPlayerSeasonStatistics> playerStats = await _context.FloorballPlayerSeasonStatistics
            .Where(s => s.CompetitionId == competitionId)
            .ToListAsync(cancellationToken);
        _context.FloorballPlayerSeasonStatistics.RemoveRange(playerStats);

        List<FloorballGoalieSeasonStatistics> goalieStats = await _context.FloorballGoalieSeasonStatistics
            .Where(s => s.CompetitionId == competitionId)
            .ToListAsync(cancellationToken);
        _context.FloorballGoalieSeasonStatistics.RemoveRange(goalieStats);
        
        await RemoveCompetitionCacheAsync(competitionId, cancellationToken);
        
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion
}
