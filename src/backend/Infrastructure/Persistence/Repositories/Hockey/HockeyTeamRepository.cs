using Domain.Common;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Competitions;
using Domain.Enums.Hockey.Teams;
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

    public async Task<IReadOnlyDictionary<Guid, string>> GetNamesByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        return await _dbContext.HockeyTeams
            .AsNoTracking()
            .Where(t => ids.Contains(t.Id))
            .Select(t => new { t.Id, t.Name })
            .ToDictionaryAsync(t => t.Id, t => t.Name, cancellationToken);
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
        IReadOnlyCollection<TeamCategory>? teamCategories = null,
        Guid? competitionId = null,
        Guid? competitionDivisionId = null,
        Guid? divisionId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<HockeyTeam> query = _dbContext.HockeyTeams.AsQueryable();

        if (clubId is Guid clubFilter)
        {
            query = query.Where(t => t.ClubId == clubFilter);
        }

        if (divisionId is Guid homeDivisionId)
        {
            query = query.Where(team => team.DivisionId == homeDivisionId);
        }

        if (teamCategories is { Count: > 0 })
        {
            query = query.Where(t => teamCategories.Contains(t.TeamCategory));
        }

        if (competitionId is Guid competitionFilter)
        {
            query = query.Where(team =>
                _dbContext.HockeyCompetitionTeams.Any(membership =>
                    membership.TeamId == team.Id
                    && membership.CompetitionId == competitionFilter
                    && membership.LeftAt == null));
        }

        if (competitionId is Guid competitionForDivision && competitionDivisionId is Guid seasonDivisionId)
        {
            query = query.Where(team =>
                _dbContext.HockeyCompetitionDivisionTeams.Any(membership =>
                    membership.IsActive
                    && membership.CompetitionTeam.TeamId == team.Id
                    && membership.CompetitionTeam.LeftAt == null
                    && membership.CompetitionTeam.CompetitionId == competitionForDivision
                    && membership.CompetitionDivision.DivisionId == seasonDivisionId
                    && membership.CompetitionDivision.CompetitionId == competitionForDivision));
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

    public async Task<IReadOnlyDictionary<Guid, int>> CountCompetitionRostersAsync(
        IReadOnlyCollection<Guid> teamIds,
        Guid competitionId,
        CancellationToken cancellationToken = default)
    {
        if (teamIds.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        List<Guid> ids = teamIds.ToList();
        var rows = await _dbContext.HockeyTeamPlayers
            .Where(player =>
                player.CompetitionId == competitionId
                && player.LeftAt == null
                && ids.Contains(player.TeamId))
            .GroupBy(player => player.TeamId)
            .Select(group => new { TeamId = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(row => row.TeamId, row => row.Count);
    }

    public async Task<IReadOnlyCollection<Guid>> GetTeamIdsWithOpenRosterAsync(
        IReadOnlyCollection<Guid> teamIds,
        CancellationToken cancellationToken = default)
    {
        if (teamIds.Count == 0)
        {
            return Array.Empty<Guid>();
        }

        List<Guid> ids = teamIds.ToList();
        return await _dbContext.HockeyTeamPlayers
            .Where(player => player.LeftAt == null && ids.Contains(player.TeamId))
            .Select(player => player.TeamId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasAnyForClubAsync(Guid clubId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.HockeyTeams.AnyAsync(t => t.ClubId == clubId, cancellationToken);
    }

    public async Task<bool> HasAnyForDivisionAsync(Guid divisionId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.HockeyTeams.AnyAsync(t => t.DivisionId == divisionId, cancellationToken);
    }

    public async Task<int> DeactivateOpenPlayerLicencesAsync(CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;
        return await _dbContext.HockeyTeamPlayers
            .Where(row => row.LeftAt == null && row.RosterStatus == HockeyRosterStatus.Active)
            .Where(row =>
                row.CompetitionId == null
                || !_dbContext.HockeyCompetitions.Any(competition =>
                    competition.Id == row.CompetitionId
                    && competition.Status == HockeyCompetitionStatus.Completed))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(row => row.RosterStatus, HockeyRosterStatus.Inactive)
                    .SetProperty(row => row.UpdatedAt, now),
                cancellationToken);
    }

    public async Task<IReadOnlyList<PlayerLicenceRow>> GetOpenPlayerLicencesAsync(
        Guid playerId,
        CancellationToken cancellationToken = default)
    {
        List<PlayerLicenceRow> rows = await (
            from membership in _dbContext.HockeyTeamPlayers
            join team in _dbContext.HockeyTeams on membership.TeamId equals team.Id
            join competition in _dbContext.HockeyCompetitions on membership.CompetitionId equals competition.Id into competitions
            from competition in competitions.DefaultIfEmpty()
            where membership.PlayerId == playerId
                && membership.LeftAt == null
                && membership.RosterStatus == HockeyRosterStatus.Active
                && competition != null
                && competition.Status == HockeyCompetitionStatus.Active
            orderby team.Name, competition != null ? competition.Name : null
            select new PlayerLicenceRow(
                team.Id,
                team.Name,
                membership.CompetitionId,
                competition != null ? competition.Name : null,
                membership.RosterStatus == HockeyRosterStatus.Active)
        ).ToListAsync(cancellationToken);

        return rows;
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
