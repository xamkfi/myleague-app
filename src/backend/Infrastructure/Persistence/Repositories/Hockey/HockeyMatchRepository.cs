using Domain.Common;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
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
        return await BuildDetailQuery()
            .Where(m => m.CompetitionId == competitionId)
            .OrderBy(m => m.ScheduledStartTime)
            .ToListAsync();
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
        CancellationToken cancellationToken = default)
    {
        DateTime? startDateUtc = startDate.HasValue
            ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
            : null;
        DateTime? endDateUtc = endDate.HasValue
            ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc)
            : null;

        IQueryable<HockeyMatch> query = _dbContext.HockeyMatches
            .Include(m => m.MatchTeams)
            .Include(m => m.Officials)
            .Include(m => m.PeriodScores)
            .AsQueryable();

        if (competitionId.HasValue)
        {
            query = query.Where(m => m.CompetitionId == competitionId.Value);
        }

        if (teamId.HasValue)
        {
            query = query.Where(m => m.MatchTeams.Any(t => t.TeamId == teamId.Value));
        }

        if (startDateUtc.HasValue)
        {
            query = query.Where(m => m.ScheduledStartTime >= startDateUtc.Value);
        }

        if (endDateUtc.HasValue)
        {
            query = query.Where(m => m.ScheduledStartTime <= endDateUtc.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(m => m.Status == status.Value);
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
