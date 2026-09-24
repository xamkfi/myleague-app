using Domain.Common;
using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Competitions;
using Domain.Enums.Hockey.Matches;
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

    public async Task<IReadOnlyList<HockeyMatch>> GetByCompetitionIdAsync(Guid competitionId)
    {
        return await BuildListQuery()
            .Where(m => m.CompetitionId == competitionId)
            .OrderBy(m => m.ScheduledStartTime)
            .ToListAsync();
    }

    public Task<bool> HasMatchThatBlocksSeasonDeleteAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.HockeyMatches
            .AsNoTracking()
            .AnyAsync(
                match => match.CompetitionId == competitionId
                    && match.Status != HockeyMatchStatus.Scheduled
                    && match.Status != HockeyMatchStatus.Postponed,
                cancellationToken);
    }

    public async Task<int> DeleteAllByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        List<Guid> matchIds = await _dbContext.HockeyMatches
            .AsNoTracking()
            .Where(match => match.CompetitionId == competitionId)
            .Select(match => match.Id)
            .ToListAsync(cancellationToken);

        if (matchIds.Count == 0)
        {
            return 0;
        }

        // Match statistics restrict the match row. Drop them before the matches.
        await _dbContext.HockeyGoaliePeriodStatistics
            .Where(stat => matchIds.Contains(stat.MatchId))
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.HockeyGoalieMatchStatistics
            .Where(stat => matchIds.Contains(stat.MatchId))
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.HockeyMatchPlayerStatistics
            .Where(stat => matchIds.Contains(stat.MatchId))
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.HockeyMatchTeamStatistics
            .Where(stat => matchIds.Contains(stat.MatchId))
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.HockeyMatches
            .Where(match => match.CompetitionId == competitionId && match.NextMatchId != null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(match => match.NextMatchId, _ => (Guid?)null),
                cancellationToken);

        // Events restrict the match team and the active player. Remove them before those parents.
        await _dbContext.HockeyMatchEvents
            .Where(matchEvent => matchIds.Contains(matchEvent.MatchId))
            .ExecuteDeleteAsync(cancellationToken);

        List<Guid> matchTeamIds = await _dbContext.HockeyMatchTeams
            .Where(matchTeam => matchIds.Contains(matchTeam.MatchId))
            .Select(matchTeam => matchTeam.Id)
            .ToListAsync(cancellationToken);

        if (matchTeamIds.Count > 0)
        {
            List<Guid> onIceStateIds = await _dbContext.HockeyOnIceStates
                .Where(state => matchTeamIds.Contains(state.MatchTeamId))
                .Select(state => state.Id)
                .ToListAsync(cancellationToken);
            if (onIceStateIds.Count > 0)
            {
                await _dbContext.HockeyOnIceChanges
                    .Where(change => onIceStateIds.Contains(change.OnIceStateId))
                    .ExecuteDeleteAsync(cancellationToken);
                await _dbContext.HockeyOnIcePlayers
                    .Where(player => onIceStateIds.Contains(player.OnIceStateId))
                    .ExecuteDeleteAsync(cancellationToken);
            }

            List<Guid> lineIds = await _dbContext.HockeyMatchLines
                .Where(line => matchTeamIds.Contains(line.MatchTeamId))
                .Select(line => line.Id)
                .ToListAsync(cancellationToken);
            if (lineIds.Count > 0)
            {
                await _dbContext.HockeyMatchLinePlayers
                    .Where(player => lineIds.Contains(player.MatchLineId))
                    .ExecuteDeleteAsync(cancellationToken);
            }

            await _dbContext.HockeyMatchTeams
                .Where(matchTeam => matchTeamIds.Contains(matchTeam.Id) && matchTeam.ActiveGoalieMatchPlayerId != null)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(matchTeam => matchTeam.ActiveGoalieMatchPlayerId, _ => (Guid?)null),
                    cancellationToken);
        }

        return await _dbContext.HockeyMatches
            .Where(match => match.CompetitionId == competitionId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<HockeyMatch>> GetByTeamIdAsync(Guid teamId)
    {
        return await BuildDetailQuery()
            .Where(m => m.MatchTeams.Any(t => t.TeamId == teamId))
            .OrderBy(m => m.ScheduledStartTime)
            .ToListAsync();
    }

    public async Task<bool> HasAnyForTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.HockeyMatches
            .AnyAsync(m => m.MatchTeams.Any(t => t.TeamId == teamId), cancellationToken);
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

    public void MarkEventAsDeleted(HockeyMatchEvent matchEvent)
    {
        _dbContext.Entry(matchEvent).State = EntityState.Deleted;
    }

    public async Task<PagedResult<HockeyMatch>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? competitionId = null,
        Guid? teamId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        HockeyMatchStatus? status = null,
        string sortOrder = "desc",
        string? searchQuery = null,
        TeamCategory? teamCategory = null,
        bool excludeDraftCompetitions = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<HockeyMatch> query = BuildListQuery();

        if (competitionId is Guid competitionFilter)
        {
            query = query.Where(m => m.CompetitionId == competitionFilter);
        }

        if (teamId is Guid teamFilter)
        {
            query = query.Where(m => m.MatchTeams.Any(t => t.TeamId == teamFilter));
        }

        if (startDate is DateTime start)
        {
            DateTime startDateUtc = DateTime.SpecifyKind(start, DateTimeKind.Utc);
            query = query.Where(m => m.ScheduledStartTime >= startDateUtc);
        }

        if (endDate is DateTime end)
        {
            DateTime endDateUtc = DateTime.SpecifyKind(end, DateTimeKind.Utc);
            query = query.Where(m => m.ScheduledStartTime <= endDateUtc);
        }

        if (status is HockeyMatchStatus statusFilter)
        {
            query = query.Where(m => m.Status == statusFilter);
        }

        if (teamCategory is TeamCategory categoryFilter)
        {
            query = query.Where(m => m.Competition != null && m.Competition.TeamCategory == categoryFilter);
        }

        if (excludeDraftCompetitions)
        {
            query = query.Where(match =>
                match.CompetitionId == null
                || (match.Competition != null && match.Competition.Status != HockeyCompetitionStatus.Draft)
                || (match.Competition is HockeySeason && match.Competition.EndDate < DateTime.UtcNow));
        }

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            string loweredSearch = searchQuery.ToLower();
            query = query.Where(m => m.Venue != null && m.Venue.ToLower().Contains(loweredSearch));
        }

        query = string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderBy(m => m.ScheduledStartTime)
            : query.OrderByDescending(m => m.ScheduledStartTime);

        int totalCount = await query.CountAsync(cancellationToken);
        List<HockeyMatch> items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult.Create(items, totalCount, page, pageSize);
    }

    private IQueryable<HockeyMatch> BuildListQuery() =>
        _dbContext.HockeyMatches
            .AsNoTracking()
            .Include(m => m.MatchTeams)
            .Include(m => m.Officials)
            .Include(m => m.PeriodScores)
            .Include(m => m.Competition);

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
