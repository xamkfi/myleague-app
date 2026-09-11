using Domain.Entities.Hockey.Statistics;
using Domain.Enums.Hockey.Statistics;
using Domain.Repositories.Hockey;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Hockey;

/// <summary>
/// EF Core repository for hockey match and competition statistics.
/// </summary>
public class HockeyStatisticsRepository : IHockeyStatisticsRepository
{
    private readonly HockeyDbContext _dbContext;

    public HockeyStatisticsRepository(HockeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<HockeyMatchTeamStatistics>> GetMatchTeamStatisticsAsync(Guid matchId)
    {
        return await _dbContext.HockeyMatchTeamStatistics
            .Where(s => s.MatchId == matchId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<HockeyMatchPlayerStatistics>> GetMatchPlayerStatisticsAsync(Guid matchId)
    {
        return await _dbContext.HockeyMatchPlayerStatistics
            .Where(s => s.MatchId == matchId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<HockeyGoalieMatchStatistics>> GetGoalieMatchStatisticsAsync(Guid matchId)
    {
        return await _dbContext.HockeyGoalieMatchStatistics
            .Include(s => s.PeriodStatistics)
            .Where(s => s.MatchId == matchId)
            .ToListAsync();
    }

    public async Task ReplaceMatchStatisticsAsync(
        Guid matchId,
        IReadOnlyList<HockeyMatchTeamStatistics> teams,
        IReadOnlyList<HockeyMatchPlayerStatistics> players,
        IReadOnlyList<HockeyGoalieMatchStatistics> goalies)
    {
        List<HockeyGoalieMatchStatistics> existingGoalies = await _dbContext.HockeyGoalieMatchStatistics
            .Include(s => s.PeriodStatistics)
            .Where(s => s.MatchId == matchId)
            .ToListAsync();
        _dbContext.HockeyGoalieMatchStatistics.RemoveRange(existingGoalies);

        List<HockeyMatchPlayerStatistics> existingPlayers = await _dbContext.HockeyMatchPlayerStatistics
            .Where(s => s.MatchId == matchId)
            .ToListAsync();
        _dbContext.HockeyMatchPlayerStatistics.RemoveRange(existingPlayers);

        List<HockeyMatchTeamStatistics> existingTeams = await _dbContext.HockeyMatchTeamStatistics
            .Where(s => s.MatchId == matchId)
            .ToListAsync();
        _dbContext.HockeyMatchTeamStatistics.RemoveRange(existingTeams);

        await _dbContext.HockeyMatchTeamStatistics.AddRangeAsync(teams);
        await _dbContext.HockeyMatchPlayerStatistics.AddRangeAsync(players);
        await _dbContext.HockeyGoalieMatchStatistics.AddRangeAsync(goalies);
    }

    public async Task<IReadOnlyList<HockeyTeamCompetitionStatistics>> GetTeamCompetitionStatisticsAsync(
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null)
    {
        return await FilterTeamScope(
                _dbContext.HockeyTeamCompetitionStatistics.Where(s => s.CompetitionId == competitionId),
                scope,
                competitionDivisionId,
                tournamentGroupId,
                playoffSeriesId)
            .OrderBy(s => s.StandingRank)
            .ThenByDescending(s => s.Points)
            .ToListAsync();
    }

    public async Task<HockeyTeamCompetitionStatistics?> GetTeamCompetitionStatisticsAsync(
        Guid teamId,
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null)
    {
        return await FilterTeamScope(
                _dbContext.HockeyTeamCompetitionStatistics
                    .Where(s => s.CompetitionId == competitionId && s.TeamId == teamId),
                scope,
                competitionDivisionId,
                tournamentGroupId,
                playoffSeriesId)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<HockeyPlayerCompetitionStatistics>> GetPlayerCompetitionStatisticsAsync(
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null)
    {
        return await FilterPlayerScope(
                _dbContext.HockeyPlayerCompetitionStatistics.Where(s => s.CompetitionId == competitionId),
                scope,
                competitionDivisionId,
                tournamentGroupId,
                playoffSeriesId)
            .OrderByDescending(s => s.Points)
            .ThenByDescending(s => s.Goals)
            .ToListAsync();
    }

    public async Task<HockeyPlayerCompetitionStatistics?> GetPlayerCompetitionStatisticsAsync(
        Guid playerId,
        Guid teamId,
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null)
    {
        return await FilterPlayerScope(
                _dbContext.HockeyPlayerCompetitionStatistics
                    .Where(s => s.CompetitionId == competitionId && s.PlayerId == playerId && s.TeamId == teamId),
                scope,
                competitionDivisionId,
                tournamentGroupId,
                playoffSeriesId)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<HockeyGoalieCompetitionStatistics>> GetGoalieCompetitionStatisticsAsync(
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null)
    {
        return await FilterGoalieScope(
                _dbContext.HockeyGoalieCompetitionStatistics.Where(s => s.CompetitionId == competitionId),
                scope,
                competitionDivisionId,
                tournamentGroupId,
                playoffSeriesId)
            .OrderByDescending(s => s.SavePercentage)
            .ThenBy(s => s.GoalsAgainstAverage)
            .ToListAsync();
    }

    public async Task<HockeyGoalieCompetitionStatistics?> GetGoalieCompetitionStatisticsAsync(
        Guid playerId,
        Guid teamId,
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null)
    {
        return await FilterGoalieScope(
                _dbContext.HockeyGoalieCompetitionStatistics
                    .Where(s => s.CompetitionId == competitionId && s.PlayerId == playerId && s.TeamId == teamId),
                scope,
                competitionDivisionId,
                tournamentGroupId,
                playoffSeriesId)
            .FirstOrDefaultAsync();
    }

    public async Task ReplaceCompetitionStatisticsAsync(
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId,
        Guid? tournamentGroupId,
        Guid? playoffSeriesId,
        IReadOnlyList<HockeyTeamCompetitionStatistics> teams,
        IReadOnlyList<HockeyPlayerCompetitionStatistics> players,
        IReadOnlyList<HockeyGoalieCompetitionStatistics> goalies)
    {
        await ResetCompetitionStatisticsAsync(
            competitionId,
            scope,
            competitionDivisionId,
            tournamentGroupId,
            playoffSeriesId);

        await _dbContext.HockeyTeamCompetitionStatistics.AddRangeAsync(teams);
        await _dbContext.HockeyPlayerCompetitionStatistics.AddRangeAsync(players);
        await _dbContext.HockeyGoalieCompetitionStatistics.AddRangeAsync(goalies);
    }

    public async Task ResetCompetitionStatisticsAsync(
        Guid competitionId,
        HockeyStatisticsScope? scope = null,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null)
    {
        IQueryable<HockeyTeamCompetitionStatistics> teams =
            _dbContext.HockeyTeamCompetitionStatistics.Where(s => s.CompetitionId == competitionId);
        IQueryable<HockeyPlayerCompetitionStatistics> players =
            _dbContext.HockeyPlayerCompetitionStatistics.Where(s => s.CompetitionId == competitionId);
        IQueryable<HockeyGoalieCompetitionStatistics> goalies =
            _dbContext.HockeyGoalieCompetitionStatistics.Where(s => s.CompetitionId == competitionId);

        if (scope is HockeyStatisticsScope concreteScope)
        {
            teams = FilterTeamScope(teams, concreteScope, competitionDivisionId, tournamentGroupId, playoffSeriesId);
            players = FilterPlayerScope(players, concreteScope, competitionDivisionId, tournamentGroupId, playoffSeriesId);
            goalies = FilterGoalieScope(goalies, concreteScope, competitionDivisionId, tournamentGroupId, playoffSeriesId);
        }

        _dbContext.HockeyTeamCompetitionStatistics.RemoveRange(await teams.ToListAsync());
        _dbContext.HockeyPlayerCompetitionStatistics.RemoveRange(await players.ToListAsync());
        _dbContext.HockeyGoalieCompetitionStatistics.RemoveRange(await goalies.ToListAsync());

        await RemoveCompetitionCacheAsync(competitionId);
    }

    public async Task<HockeyStatisticsCache?> GetCachedStatisticsAsync(
        string cacheKey,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HockeyStatisticsCache
            .FirstOrDefaultAsync(c => c.CacheKey == cacheKey, cancellationToken);
    }

    public async Task SaveCachedStatisticsAsync(
        HockeyStatisticsCache cache,
        CancellationToken cancellationToken = default)
    {
        HockeyStatisticsCache? existing = await GetCachedStatisticsAsync(cache.CacheKey, cancellationToken);
        if (existing is null)
        {
            await _dbContext.HockeyStatisticsCache.AddAsync(cache, cancellationToken);
        }
        else
        {
            existing.UpdateData(
                cache.JsonData,
                (int)(cache.ExpiresAt - DateTime.UtcNow).TotalMinutes);
        }
    }

    public async Task<int> RemoveExpiredCacheAsync(CancellationToken cancellationToken = default)
    {
        List<HockeyStatisticsCache> expiredEntries = await _dbContext.HockeyStatisticsCache
            .Where(c => c.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        if (expiredEntries.Count > 0)
        {
            _dbContext.HockeyStatisticsCache.RemoveRange(expiredEntries);
        }

        return expiredEntries.Count;
    }

    public async Task RemoveCompetitionCacheAsync(
        Guid competitionId,
        CancellationToken cancellationToken = default)
    {
        List<HockeyStatisticsCache> competitionCacheEntries = await _dbContext.HockeyStatisticsCache
            .Where(c => c.CompetitionId == competitionId)
            .ToListAsync(cancellationToken);

        if (competitionCacheEntries.Count > 0)
        {
            _dbContext.HockeyStatisticsCache.RemoveRange(competitionCacheEntries);
        }
    }

    public async Task<IReadOnlyList<HockeyPlayerCompetitionStatistics>> GetTopScorersAsync(
        Guid competitionId,
        HockeyStatisticsScope scope,
        int topN,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null)
    {
        return await FilterPlayerScope(
                _dbContext.HockeyPlayerCompetitionStatistics.Where(s => s.CompetitionId == competitionId),
                scope,
                competitionDivisionId,
                tournamentGroupId,
                playoffSeriesId)
            .OrderByDescending(s => s.Points)
            .ThenByDescending(s => s.Goals)
            .ThenByDescending(s => s.Assists)
            .ThenBy(s => s.GamesPlayed)
            .Take(topN)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<HockeyGoalieCompetitionStatistics>> GetTopGoaliesAsync(
        Guid competitionId,
        HockeyStatisticsScope scope,
        int topN,
        int minimumGamesPlayed = 1,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null)
    {
        return await FilterGoalieScope(
                _dbContext.HockeyGoalieCompetitionStatistics
                    .Where(s => s.CompetitionId == competitionId && s.GamesPlayed >= minimumGamesPlayed),
                scope,
                competitionDivisionId,
                tournamentGroupId,
                playoffSeriesId)
            .OrderByDescending(s => s.SavePercentage)
            .ThenBy(s => s.GoalsAgainstAverage)
            .ThenByDescending(s => s.Wins)
            .Take(topN)
            .ToListAsync();
    }

    private static IQueryable<HockeyTeamCompetitionStatistics> FilterTeamScope(
        IQueryable<HockeyTeamCompetitionStatistics> query,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId,
        Guid? tournamentGroupId,
        Guid? playoffSeriesId) =>
        query.Where(s =>
            s.Scope == scope &&
            s.CompetitionDivisionId == competitionDivisionId &&
            s.TournamentGroupId == tournamentGroupId &&
            s.PlayoffSeriesId == playoffSeriesId);

    private static IQueryable<HockeyPlayerCompetitionStatistics> FilterPlayerScope(
        IQueryable<HockeyPlayerCompetitionStatistics> query,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId,
        Guid? tournamentGroupId,
        Guid? playoffSeriesId) =>
        query.Where(s =>
            s.Scope == scope &&
            s.CompetitionDivisionId == competitionDivisionId &&
            s.TournamentGroupId == tournamentGroupId &&
            s.PlayoffSeriesId == playoffSeriesId);

    private static IQueryable<HockeyGoalieCompetitionStatistics> FilterGoalieScope(
        IQueryable<HockeyGoalieCompetitionStatistics> query,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId,
        Guid? tournamentGroupId,
        Guid? playoffSeriesId) =>
        query.Where(s =>
            s.Scope == scope &&
            s.CompetitionDivisionId == competitionDivisionId &&
            s.TournamentGroupId == tournamentGroupId &&
            s.PlayoffSeriesId == playoffSeriesId);
}
