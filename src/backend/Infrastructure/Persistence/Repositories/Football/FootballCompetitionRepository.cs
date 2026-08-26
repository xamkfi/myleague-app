using Domain.Common;
using Domain.Entities.Football.Competitions;
using Domain.Repositories.Football;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Football
{
    /// <summary>
    /// Implementation of the football competition repository
    /// </summary>
    public class FootballCompetitionRepository : RepositoryBase<FootballCompetition, FootballDbContext>, IFootballCompetitionRepository
    {
        /// <summary>
        /// Initializes a new instance of the FootballCompetitionRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public FootballCompetitionRepository(FootballDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a football competition by ID
        /// </summary>
        /// <param name="id">The competition ID</param>
        /// <returns>The competition if found, null otherwise</returns>
        public async Task<FootballCompetition?> GetByIdAsync(Guid? id)
        {
            return await _entities
                .Include(s => s.Teams)
                .Include(s => s.Matches)
                    .ThenInclude(m => m.HomeTeam)
                .Include(s => s.Matches)
                    .ThenInclude(m => m.AwayTeam)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Gets all football competitions
        /// </summary>
        /// <returns>A collection of all football competitions</returns>
        public override async Task<IEnumerable<FootballCompetition>> GetAllAsync()
        {
            return await _entities
                .Include(s => s.Teams)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a football competition by name
        /// </summary>
        /// <param name="name">The competition name</param>
        /// <returns>The competition if found, null otherwise</returns>
        public async Task<FootballCompetition?> GetByNameAsync(string name)
        {
            return await _entities
                .Include(s => s.Teams)
                .Include(s => s.Matches)
                    .ThenInclude(m => m.HomeTeam)
                .Include(s => s.Matches)
                    .ThenInclude(m => m.AwayTeam)
                .FirstOrDefaultAsync(s => s.Name == name);
        }

        /// <summary>
        /// Gets active football competitions
        /// </summary>
        /// <returns>A collection of active football competitions</returns>
        public async Task<IEnumerable<FootballCompetition>> GetActiveAsync()
        {
            return await _entities
                .Include(s => s.Teams)
                .Where(s => s.IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// Gets completed football competitions
        /// </summary>
        /// <returns>A collection of completed football competitions</returns>
        public async Task<IEnumerable<FootballCompetition>> GetCompletedAsync()
        {
            return await _entities
                .Include(s => s.Teams)
                .Where(s => s.IsCompleted)
                .ToListAsync();
        }

        /// <summary>
        /// Gets football competitions by division
        /// </summary>
        /// <param name="divisionId">The division to filter by</param>
        /// <returns>A collection of football competitions for the specified division</returns>
        public async Task<IEnumerable<FootballCompetition>> GetByDivisionAsync(Guid divisionId)
        {
            HashSet<Guid> competitionIds = await _dbContext.Set<FootballCompetitionDivision>()
                .Where(sd => sd.DivisionId == divisionId)
                .Select(sd => sd.CompetitionId)
                .ToHashSetAsync();

            return await _entities
                .Include(s => s.Teams)
                .Where(s => competitionIds.Contains(s.Id))
                .ToListAsync();
        }

        /// <summary>
        /// Gets competitions containing a specific team
        /// </summary>
        /// <param name="teamId">The team ID</param>
        /// <returns>A collection of competitions with the team participating</returns>
        public async Task<IEnumerable<FootballCompetition>> GetByTeamIdAsync(Guid teamId)
        {
            return await _entities
                .Include(s => s.Teams)
                .Where(s => s.Teams.Any(t => t.Id == teamId))
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new football competition
        /// </summary>
        /// <param name="competition">The competition to add</param>
        public override async Task AddAsync(FootballCompetition competition)
        {
            await base.AddAsync(competition);
        }

        /// <summary>
        /// Updates an existing football competition
        /// </summary>
        /// <param name="competition">The competition to update</param>
        public override async Task UpdateAsync(FootballCompetition competition)
        {
            await base.UpdateAsync(competition);
        }

        /// <summary>
        /// Deletes a football competition by ID
        /// </summary>
        /// <param name="id">The ID of the competition to delete</param>
        public async Task DeleteAsync(Guid id)
        {
            FootballCompetition? competition = await _entities.FindAsync(id);
            if (competition != null)
            {
                await DeleteAsync(competition);
            }
        }

        /// <summary>
        /// Checks if a football competition exists
        /// </summary>
        /// <param name="id">The competition ID</param>
        /// <returns>True if the competition exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _entities.AnyAsync(s => s.Id == id);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<FootballSeasonDateSummary>> GetSeasonDateSummariesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _entities
                .OfType<FootballSeason>()
                .AsNoTracking()
                .Select(s => new FootballSeasonDateSummary(s.StartDate, s.EndDate, s.IsActive))
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<PagedResult<FootballSeason>> GetSeasonsPagedAsync(
            int page,
            int pageSize,
            int? startYear,
            int? endYear,
            Domain.Enums.Common.TeamCategory? teamCategory = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FootballSeason> query = _entities
                .OfType<FootballSeason>()
                .AsNoTracking();

            if (startYear.HasValue && endYear.HasValue)
            {
                int start = startYear.Value;
                int end = endYear.Value;
                query = query.Where(s => s.StartDate.Year == start && s.EndDate.Year == end);
            }

            if (teamCategory.HasValue)
            {
                query = query.Where(s => s.TeamCategory == teamCategory.Value);
            }

            int totalCount = await query.CountAsync(cancellationToken);

            List<FootballSeason> items = await query
                .OrderByDescending(s => s.IsActive)
                .ThenByDescending(s => s.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PagedResult.Create(items, totalCount, page, pageSize);
        }
    }
}
