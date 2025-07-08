using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Floorball
{
    /// <summary>
    /// Implementation of the floorball season repository
    /// </summary>
    public class FloorballSeasonRepository : RepositoryBase<FloorballSeason, FloorballDbContext>, IFloorballSeasonRepository
    {
        /// <summary>
        /// Initializes a new instance of the FloorballSeasonRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public FloorballSeasonRepository(FloorballDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a floorball season by ID
        /// </summary>
        /// <param name="id">The season ID</param>
        /// <returns>The season if found, null otherwise</returns>
        public override async Task<FloorballSeason?> GetByIdAsync(Guid id)
        {
            return await _entities
                .Include(s => s.Teams)
                .Include(s => s.Matches)
                    .ThenInclude(m => m.HomeTeam)
                .Include(s => s.Matches)
                    .ThenInclude(m => m.AwayTeam)
                .FirstOrDefaultAsync(s => s.Id == id) ?? throw new KeyNotFoundException($"Season with ID {id} not found.");
        }

        /// <summary>
        /// Gets all floorball seasons
        /// </summary>
        /// <returns>A collection of all floorball seasons</returns>
        public override async Task<IEnumerable<FloorballSeason>> GetAllAsync()
        {
            return await _entities
                .Include(s => s.Teams)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a floorball season by name
        /// </summary>
        /// <param name="name">The season name</param>
        /// <returns>The season if found, null otherwise</returns>
        public async Task<FloorballSeason?> GetByNameAsync(string name)
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
        /// Gets active floorball seasons
        /// </summary>
        /// <returns>A collection of active floorball seasons</returns>
        public async Task<IEnumerable<FloorballSeason>> GetActiveAsync()
        {
            return await _entities
                .Include(s => s.Teams)
                .Where(s => s.IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// Gets completed floorball seasons
        /// </summary>
        /// <returns>A collection of completed floorball seasons</returns>
        public async Task<IEnumerable<FloorballSeason>> GetCompletedAsync()
        {
            return await _entities
                .Include(s => s.Teams)
                .Where(s => s.IsCompleted)
                .ToListAsync();
        }

        /// <summary>
        /// Gets floorball seasons by division
        /// </summary>
        /// <param name="division">The division to filter by</param>
        /// <returns>A collection of floorball seasons for the specified division</returns>
        public async Task<IEnumerable<FloorballSeason>> GetByDivisionAsync(Guid divisionId)
        {
            return await _entities
                .Include(s => s.Teams)
                .Where(s => s.DivisionId == divisionId)
                .ToListAsync();
        }

        /// <summary>
        /// Gets seasons containing a specific team
        /// </summary>
        /// <param name="teamId">The team ID</param>
        /// <returns>A collection of seasons with the team participating</returns>
        public async Task<IEnumerable<FloorballSeason>> GetByTeamIdAsync(Guid teamId)
        {
            return await _entities
                .Include(s => s.Teams)
                .Where(s => s.Teams.Any(t => t.Id == teamId))
                .ToListAsync();
        }

        /// <summary>
        /// Gets the current or upcoming season for a division
        /// </summary>
        /// <param name="divisionId">The division</param>
        /// <returns>The current or next season for the division</returns>
        public async Task<FloorballSeason> GetCurrentOrUpcomingAsync(Guid divisionId)
        {
            DateTime now = DateTime.UtcNow;
            
            // First try to find an active season
            FloorballSeason? activeSeason = await _entities
                .Include(s => s.Teams)
                .Where(s => s.DivisionId == divisionId && s.IsActive)
                .FirstOrDefaultAsync();
                
            if (activeSeason != null)
                return activeSeason;
                
            // If no active season, try to find a future season
            FloorballSeason? futureSeason = await _entities
                .Include(s => s.Teams)
                .Where(s => s.DivisionId == divisionId && s.StartDate > now && !s.IsCompleted)
                .OrderBy(s => s.StartDate)
                .FirstOrDefaultAsync();
                
            return futureSeason ?? throw new KeyNotFoundException($"No current or upcoming season found for division {divisionId}.");
        }

        /// <summary>
        /// Adds a new floorball season
        /// </summary>
        /// <param name="season">The season to add</param>
        public override async Task AddAsync(FloorballSeason season)
        {
            await base.AddAsync(season);
        }

        /// <summary>
        /// Updates an existing floorball season
        /// </summary>
        /// <param name="season">The season to update</param>
        public override async Task UpdateAsync(FloorballSeason season)
        {
            await base.UpdateAsync(season);
        }

        /// <summary>
        /// Deletes a floorball season by ID
        /// </summary>
        /// <param name="id">The ID of the season to delete</param>
        public async Task DeleteAsync(Guid id)
        {
            FloorballSeason? season = await _entities.FindAsync(id);
            if (season != null)
            {
                await DeleteAsync(season);
            }
        }

        /// <summary>
        /// Checks if a floorball season exists
        /// </summary>
        /// <param name="id">The season ID</param>
        /// <returns>True if the season exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _entities.AnyAsync(s => s.Id == id);
        }
    }
} 
