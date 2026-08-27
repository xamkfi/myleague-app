using Domain.Entities.Hockey.Competitions;
using Domain.Enums.Hockey.Competitions;
using Domain.Repositories.Hockey;
using Microsoft.EntityFrameworkCore;
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
            .Include(season => season.ContentBlocks)
            .OrderByDescending(season => season.Status == HockeyCompetitionStatus.Active)
            .ThenByDescending(season => season.StartDate)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
