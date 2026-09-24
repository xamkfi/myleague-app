using Domain.Entities.Hockey.Competitions;
using Domain.Enums.Hockey.Competitions;
using Domain.Repositories.Hockey;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Hockey;

/// <summary>
/// EF Core repository for hockey competitions.
/// </summary>
public class HockeyCompetitionRepository : IHockeyCompetitionRepository
{
    private readonly HockeyDbContext _dbContext;

    public HockeyCompetitionRepository(HockeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(HockeyCompetition competition)
    {
        await _dbContext.HockeyCompetitions.AddAsync(competition);
    }

    public async Task<HockeyCompetition?> GetByIdAsync(Guid id)
    {
        HockeyCompetition? competition = await _dbContext.HockeyCompetitions
            .AsSplitQuery()
            .Include(c => c.Teams)
            .Include(c => c.Divisions)
                .ThenInclude(d => d.Teams)
            .Include(c => c.PlayoffSeries)
            .Include(c => c.Matches)
                .ThenInclude(m => m.MatchTeams)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (competition is HockeyTournament tournament)
        {
            await _dbContext.Entry(tournament)
                .Collection(t => t.Groups)
                .Query()
                .Include(g => g.Teams)
                .LoadAsync();
        }

        return competition;
    }

    public async Task<HockeySeason?> GetSeasonByIdAsync(Guid id)
    {
        return await _dbContext.HockeySeasons
            .AsSplitQuery()
            .Include(c => c.Teams)
            .Include(c => c.Divisions)
                .ThenInclude(d => d.Teams)
            .Include(c => c.PlayoffSeries)
            .Include(c => c.Matches)
                .ThenInclude(m => m.MatchTeams)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<HockeySeason?> GetSeasonByNameAsync(string name)
    {
        return await _dbContext.HockeySeasons
            .FirstOrDefaultAsync(season => season.Name == name);
    }

    public async Task<HockeyTournament?> GetTournamentByIdAsync(Guid id)
    {
        return await _dbContext.HockeyTournaments
            .AsSplitQuery()
            .Include(c => c.Teams)
            .Include(c => c.Groups)
                .ThenInclude(g => g.Teams)
            .Include(c => c.PlayoffSeries)
            .Include(c => c.Matches)
                .ThenInclude(m => m.MatchTeams)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IReadOnlyList<HockeySeason>> GetAllSeasonsAsync()
    {
        List<HockeySeason> seasons = await _dbContext.HockeySeasons
            .AsSplitQuery()
            .Include(c => c.Teams)
            .Include(c => c.Divisions)
                .ThenInclude(d => d.Teams)
            .Include(c => c.PlayoffSeries)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();
        return seasons.DistinctBy(season => season.Id).ToList();
    }

    public async Task<IReadOnlyList<HockeyTournament>> GetAllTournamentsAsync()
    {
        List<HockeyTournament> tournaments = await _dbContext.HockeyTournaments
            .AsSplitQuery()
            .Include(c => c.Teams)
            .Include(c => c.Groups)
                .ThenInclude(g => g.Teams)
            .Include(c => c.PlayoffSeries)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();
        return tournaments.DistinctBy(tournament => tournament.Id).ToList();
    }

    public async Task<HockeySeason?> GetSeasonWithContentBlocksAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HockeySeasons
            .Include(season => season.ContentBlocks)
            .FirstOrDefaultAsync(season => season.Id == id, cancellationToken);
    }

    public async Task<HockeySeason?> GetFeaturedSeasonWithContentBlocksAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HockeySeasons
            .Where(season => season.Status != HockeyCompetitionStatus.Draft || season.EndDate < DateTime.UtcNow)
            .Include(season => season.ContentBlocks)
            .OrderByDescending(season => season.Status == HockeyCompetitionStatus.Active)
            .ThenByDescending(season => season.StartDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.HockeySeasons.AnyAsync(season => season.Id == id, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Division teams restrict the competition team, while both the division and the
        // competition team cascade from the competition. Remove the division-team rows first
        // so the season delete can cascade the rest.
        List<Guid> divisionIds = await _dbContext.HockeyCompetitionDivisions
            .Where(division => division.CompetitionId == id)
            .Select(division => division.Id)
            .ToListAsync(cancellationToken);

        if (divisionIds.Count > 0)
        {
            List<HockeyCompetitionDivisionTeam> divisionTeams = await _dbContext.HockeyCompetitionDivisionTeams
                .Where(team => divisionIds.Contains(team.CompetitionDivisionId))
                .ToListAsync(cancellationToken);
            _dbContext.HockeyCompetitionDivisionTeams.RemoveRange(divisionTeams);
        }

        HockeySeason? season = await _dbContext.HockeySeasons
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (season != null)
        {
            _dbContext.HockeySeasons.Remove(season);
        }
    }

    public void MarkNewContentBlocksAdded(HockeySeason season, IReadOnlyCollection<Guid> existingBlockIds)
    {
        foreach (HockeySeasonContentBlock block in season.ContentBlocks.Where(block => !existingBlockIds.Contains(block.Id)))
        {
            EntityEntry<HockeySeasonContentBlock> entry = _dbContext.Entry(block);
            if (entry.State == EntityState.Detached)
            {
                entry = _dbContext.Add(block);
            }
            else if (entry.State != EntityState.Added)
            {
                entry.State = EntityState.Added;
            }

            entry.Property(added => added.Id).IsTemporary = false;
        }
    }
}
