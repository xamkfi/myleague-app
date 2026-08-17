using Domain.Entities.Football.Statistics;
using Domain.Repositories.Football;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Football;

/// <summary>
/// Implementation of the football statistics repository
/// </summary>
public class FootballStatisticsRepository : IFootballStatisticsRepository
{
    private readonly FootballDbContext _context;

    /// <summary>
    /// Initializes a new instance of the FootballStatisticsRepository class
    /// </summary>
    /// <param name="context">The database context</param>
    public FootballStatisticsRepository(FootballDbContext context)
    {
        _context = context;
    }

    #region Team Season Statistics

    /// <inheritdoc />
    public async Task<FootballTeamSeasonStatistics?> GetTeamSeasonStatisticsAsync(Guid teamId, Guid competitionId, CancellationToken cancellationToken = default)
    {
        return await _context.FootballTeamSeasonStatistics.Include(x => x.Team).Include(x => x.Competition)
            .FirstOrDefaultAsync(s => s.TeamId == teamId && s.CompetitionId == competitionId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FootballTeamSeasonStatistics>> GetTeamStatisticsByCompetitionAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        return await _context.FootballTeamSeasonStatistics.Include(x => x.Competition).Include(x => x.Team)
            .Where(s => s.CompetitionId == competitionId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FootballTeamSeasonStatistics>> GetTeamStandingsAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        return await _context.FootballTeamSeasonStatistics
            .Include(x => x.Team)
            .Include(x => x.Competition)
            .Where(s => s.CompetitionId == competitionId)
            .OrderByDescending(s => s.Points)
            .ThenByDescending(s => s.GoalDifference)
            .ThenByDescending(s => s.GoalsFor)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FootballTeamSeasonStatistics>> GetTeamSeasonStatisticsForTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        return await _context.FootballTeamSeasonStatistics
            .Include(x => x.Team)
            .Include(x => x.Competition)
            .Where(s => s.TeamId == teamId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveTeamSeasonStatisticsAsync(FootballTeamSeasonStatistics statistics, CancellationToken cancellationToken = default)
    {
        FootballTeamSeasonStatistics? existing = await GetTeamSeasonStatisticsAsync(statistics.TeamId, statistics.CompetitionId, cancellationToken);

        if (existing == null)
        {
            await _context.FootballTeamSeasonStatistics.AddAsync(statistics, cancellationToken);
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
    public async Task<FootballPlayerSeasonStatistics?> GetPlayerSeasonStatisticsAsync(Guid playerId, Guid teamId, Guid competitionId, CancellationToken cancellationToken = default)
    {
        return await _context.FootballPlayerSeasonStatistics
            .FirstOrDefaultAsync(s => s.PlayerId == playerId && s.TeamId == teamId && s.CompetitionId == competitionId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FootballPlayerSeasonStatistics>> GetPlayerStatisticsByCompetitionAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        return await _context.FootballPlayerSeasonStatistics.Include(x => x.Player).Include(x => x.Team).Include(x => x.Competition)
            .Where(s => s.CompetitionId == competitionId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FootballPlayerSeasonStatistics>> GetPlayerStatisticsByTeamAndCompetitionAsync(Guid teamId, Guid competitionId, CancellationToken cancellationToken = default)
    {
        return await _context.FootballPlayerSeasonStatistics
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
    public async Task<List<FootballPlayerSeasonStatistics>> GetPlayerStatisticsForTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        return await _context.FootballPlayerSeasonStatistics
            .Include(x => x.Player)
            .Include(x => x.Team)
            .Include(x => x.Competition)
            .Where(s => s.TeamId == teamId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FootballPlayerSeasonStatistics>> GetTopScorersAsync(Guid competitionId, int topN, CancellationToken cancellationToken = default)
    {
        return await _context.FootballPlayerSeasonStatistics.Include(x => x.Player).Include(x => x.Team).Include(x => x.Competition)
            .Where(s => s.CompetitionId == competitionId)
            .OrderByDescending(s => s.Goals)
            .ThenByDescending(s => s.Points)
            .ThenByDescending(s => s.Assists)
            .ThenBy(s => s.GamesPlayed)
            .Take(topN)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FootballPlayerSeasonStatistics>> GetTopAssistsAsync(Guid competitionId, int topN, CancellationToken cancellationToken = default)
    {
        return await _context.FootballPlayerSeasonStatistics.Include(x => x.Player).Include(x => x.Team).Include(x => x.Competition)
            .Where(s => s.CompetitionId == competitionId)
            .OrderByDescending(s => s.Assists)
            .ThenByDescending(s => s.Points)
            .ThenByDescending(s => s.Goals)
            .ThenBy(s => s.GamesPlayed)
            .Take(topN)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FootballPlayerSeasonStatistics>> GetPlayerCareerStatisticsAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        return await _context.FootballPlayerSeasonStatistics.Include(x => x.Player).Include(x => x.Competition).Include(x => x.Team)
            .Where(s => s.PlayerId == playerId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SavePlayerSeasonStatisticsAsync(FootballPlayerSeasonStatistics statistics, CancellationToken cancellationToken = default)
    {
        FootballPlayerSeasonStatistics? existing = await GetPlayerSeasonStatisticsAsync(statistics.PlayerId, statistics.TeamId, statistics.CompetitionId, cancellationToken);

        if (existing == null)
        {
            await _context.FootballPlayerSeasonStatistics.AddAsync(statistics, cancellationToken);
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
    public async Task<FootballMatchTeamStatistics?> GetMatchTeamStatisticsAsync(Guid matchId, Guid teamId, CancellationToken cancellationToken = default)
    {
        return await _context.FootballMatchTeamStatistics
            .FirstOrDefaultAsync(s => s.MatchId == matchId && s.TeamId == teamId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FootballMatchTeamStatistics>> GetMatchStatisticsAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        return await _context.FootballMatchTeamStatistics
            .Where(s => s.MatchId == matchId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveMatchTeamStatisticsAsync(FootballMatchTeamStatistics statistics, CancellationToken cancellationToken = default)
    {
        FootballMatchTeamStatistics? existing = await GetMatchTeamStatisticsAsync(statistics.MatchId, statistics.TeamId, cancellationToken);

        if (existing == null)
        {
            await _context.FootballMatchTeamStatistics.AddAsync(statistics, cancellationToken);
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
    public async Task<FootballStatisticsCache?> GetCachedStatisticsAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        return await _context.FootballStatisticsCache
            .FirstOrDefaultAsync(c => c.CacheKey == cacheKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveCachedStatisticsAsync(FootballStatisticsCache cache, CancellationToken cancellationToken = default)
    {
        FootballStatisticsCache? existing = await GetCachedStatisticsAsync(cache.CacheKey, cancellationToken);

        if (existing == null)
        {
            await _context.FootballStatisticsCache.AddAsync(cache, cancellationToken);
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
        List<FootballStatisticsCache> expiredEntries = await _context.FootballStatisticsCache
            .Where(c => c.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        if (expiredEntries.Any())
        {
            _context.FootballStatisticsCache.RemoveRange(expiredEntries);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return expiredEntries.Count;
    }

    /// <inheritdoc />
    public async Task RemoveCompetitionCacheAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        List<FootballStatisticsCache> competitionCacheEntries = await _context.FootballStatisticsCache
            .Where(c => c.CompetitionId == competitionId)
            .ToListAsync(cancellationToken);

        if (competitionCacheEntries.Any())
        {
            _context.FootballStatisticsCache.RemoveRange(competitionCacheEntries);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    #endregion

    #region Bulk Operations

    /// <inheritdoc />
    public async Task ResetCompetitionStatisticsAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        List<FootballTeamSeasonStatistics> teamStats = await _context.FootballTeamSeasonStatistics
            .Where(s => s.CompetitionId == competitionId)
            .ToListAsync(cancellationToken);
        _context.FootballTeamSeasonStatistics.RemoveRange(teamStats);

        List<FootballPlayerSeasonStatistics> playerStats = await _context.FootballPlayerSeasonStatistics
            .Where(s => s.CompetitionId == competitionId)
            .ToListAsync(cancellationToken);
        _context.FootballPlayerSeasonStatistics.RemoveRange(playerStats);

        await RemoveCompetitionCacheAsync(competitionId, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion
}
