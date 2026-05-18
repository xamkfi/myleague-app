using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Floorball
{
    /// <summary>
    /// Implementation of the floorball competition repository
    /// </summary>
    public class FloorballCompetitionRepository : RepositoryBase<FloorballCompetition, FloorballDbContext>, IFloorballCompetitionRepository
    {
        /// <summary>
        /// Initializes a new instance of the FloorballCompetitionRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public FloorballCompetitionRepository(FloorballDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a floorball competition by ID
        /// </summary>
        /// <param name="id">The competition ID</param>
        /// <returns>The competition if found, null otherwise</returns>
        public async Task<FloorballCompetition?> GetByIdAsync(Guid? id)
        {
            return await _entities
                .Include(s => s.Teams)
                .Include(s => s.Matches)
                    .ThenInclude(m => m.HomeTeam)
                .Include(s => s.Matches)
                    .ThenInclude(m => m.AwayTeam)
                .FirstOrDefaultAsync(s => s.Id == id) ?? throw new KeyNotFoundException($"Competition with ID {id} not found.");
        }

        /// <summary>
        /// Gets all floorball competitions
        /// </summary>
        /// <returns>A collection of all floorball competitions</returns>
        public override async Task<IEnumerable<FloorballCompetition>> GetAllAsync()
        {
            return await _entities
                .Include(s => s.Teams)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a floorball competition by name
        /// </summary>
        /// <param name="name">The competition name</param>
        /// <returns>The competition if found, null otherwise</returns>
        public async Task<FloorballCompetition?> GetByNameAsync(string name)
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
        /// Gets active floorball competitions
        /// </summary>
        /// <returns>A collection of active floorball competitions</returns>
        public async Task<IEnumerable<FloorballCompetition>> GetActiveAsync()
        {
            return await _entities
                .Include(s => s.Teams)
                .Where(s => s.IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// Gets completed floorball competitions
        /// </summary>
        /// <returns>A collection of completed floorball competitions</returns>
        public async Task<IEnumerable<FloorballCompetition>> GetCompletedAsync()
        {
            return await _entities
                .Include(s => s.Teams)
                .Where(s => s.IsCompleted)
                .ToListAsync();
        }

        /// <summary>
        /// Gets floorball competitions by division
        /// </summary>
        /// <param name="divisionId">The division to filter by</param>
        /// <returns>A collection of floorball competitions for the specified division</returns>
        public async Task<IEnumerable<FloorballCompetition>> GetByDivisionAsync(Guid divisionId)
        {
            HashSet<Guid> competitionIds = await _dbContext.Set<FloorballCompetitionDivision>()
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
        public async Task<IEnumerable<FloorballCompetition>> GetByTeamIdAsync(Guid teamId)
        {
            return await _entities
                .Include(s => s.Teams)
                .Where(s => s.Teams.Any(t => t.Id == teamId))
                .ToListAsync();
        }

        /// <summary>
        /// Gets the current or upcoming competition for a division
        /// </summary>
        /// <param name="divisionId">The division</param>
        /// <returns>The current or next competition for the division</returns>
        public async Task<FloorballCompetition> GetCurrentOrUpcomingAsync(Guid divisionId)
        {
            DateTime now = DateTime.UtcNow;
            
            HashSet<Guid> competitionIds = await _dbContext.Set<FloorballCompetitionDivision>()
                .Where(sd => sd.DivisionId == divisionId)
                .Select(sd => sd.CompetitionId)
                .ToHashSetAsync();
            
            FloorballCompetition? activeCompetition = await _entities
                .Include(s => s.Teams)
                .Where(s => competitionIds.Contains(s.Id) && s.IsActive)
                .FirstOrDefaultAsync();
                
            if (activeCompetition != null)
                return activeCompetition;
                
            FloorballCompetition? futureCompetition = await _entities
                .Include(s => s.Teams)
                .Where(s => competitionIds.Contains(s.Id) && s.StartDate > now && !s.IsCompleted)
                .OrderBy(s => s.StartDate)
                .FirstOrDefaultAsync();
                
            return futureCompetition ?? throw new KeyNotFoundException($"No current or upcoming competition found for division {divisionId}.");
        }

        /// <summary>
        /// Adds a new floorball competition
        /// </summary>
        /// <param name="competition">The competition to add</param>
        public override async Task AddAsync(FloorballCompetition competition)
        {
            await base.AddAsync(competition);
        }

        /// <summary>
        /// Updates an existing floorball competition
        /// </summary>
        /// <param name="competition">The competition to update</param>
        public override async Task UpdateAsync(FloorballCompetition competition)
        {
            await base.UpdateAsync(competition);
        }

        /// <summary>
        /// Deletes a floorball competition by ID
        /// </summary>
        /// <param name="id">The ID of the competition to delete</param>
        public async Task DeleteAsync(Guid id)
        {
            FloorballCompetition? competition = await _entities.FindAsync(id);
            if (competition != null)
            {
                await DeleteAsync(competition);
            }
        }

        /// <summary>
        /// Checks if a floorball competition exists
        /// </summary>
        /// <param name="id">The competition ID</param>
        /// <returns>True if the competition exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _entities.AnyAsync(s => s.Id == id);
        }
    }
}
