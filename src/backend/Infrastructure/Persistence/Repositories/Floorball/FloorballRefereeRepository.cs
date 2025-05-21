using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Floorball
{
    /// <summary>
    /// Implementation of the floorball referee repository
    /// </summary>
    public class FloorballRefereeRepository : RepositoryBase<FloorballReferee>, IFloorballRefereeRepository
    {
        /// <summary>
        /// Initializes a new instance of the FloorballRefereeRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public FloorballRefereeRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a floorball referee by ID
        /// </summary>
        /// <param name="id">The referee ID</param>
        /// <returns>The referee if found, null otherwise</returns>
        public override async Task<FloorballReferee?> GetByIdAsync(Guid id)
        {
            return await _entities
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <summary>
        /// Gets all floorball referees
        /// </summary>
        /// <returns>A collection of all floorball referees</returns>
        public override async Task<IEnumerable<FloorballReferee>> GetAllAsync()
        {
            return await _entities
                .ToListAsync();
        }

        /// <summary>
        /// Gets all active floorball referees
        /// </summary>
        /// <returns>A collection of active floorball referees</returns>
        public async Task<IEnumerable<FloorballReferee>> GetActiveAsync()
        {
            return await _entities
                .Where(r => r.IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// Gets floorball referees by match ID
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>A collection of referees assigned to the match</returns>
        public async Task<IEnumerable<FloorballReferee>> GetByMatchIdAsync(Guid matchId)
        {
            FloorballMatch? match = await _dbContext.FloorballMatches
                .Include(m => m.Officials)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            return match?.Officials ?? new List<FloorballReferee>();
        }

        /// <summary>
        /// Gets floorball referees whose license is expiring soon
        /// </summary>
        /// <param name="withinDays">Days until expiry</param>
        /// <returns>A collection of referees whose license is expiring soon</returns>
        public async Task<IEnumerable<FloorballReferee>> GetWithExpiringLicenseAsync(int withinDays)
        {
            DateTime cutoffDate = DateTime.UtcNow.AddDays(withinDays);
            
            return await _entities
                .Where(r => r.LicenseExpiryDate <= cutoffDate && r.IsActive)
                .OrderBy(r => r.LicenseExpiryDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gets floorball referees ordered by number of matches officiated
        /// </summary>
        /// <param name="count">Maximum number of referees to return</param>
        /// <returns>The most experienced referees</returns>
        public async Task<IEnumerable<FloorballReferee>> GetMostExperiencedAsync(int count = 10)
        {
            // Get all referees
            List<FloorballReferee> referees = await _entities
                .ToListAsync();

            // Get all completed matches
            List<FloorballMatch> matches = await _dbContext.FloorballMatches
                .Include(m => m.Officials)
                .Where(m => m.Status == FloorballMatchStatus.Completed)
                .ToListAsync();

            // Count matches per referee
            Dictionary<Guid, int> refereesMatchCount = new Dictionary<Guid, int>();
            
            foreach (FloorballReferee referee in referees)
            {
                int matchCount = matches
                    .Count(m => m.Officials.Any(r => r.Id == referee.Id));
                
                refereesMatchCount[referee.Id] = matchCount;
            }

            // Return referees sorted by match count
            return referees
                .OrderByDescending(r => refereesMatchCount.GetValueOrDefault(r.Id, 0))
                .Take(count);
        }

        /// <summary>
        /// Deletes a floorball referee by ID
        /// </summary>
        /// <param name="id">The ID of the referee to delete</param>
        public async Task DeleteAsync(Guid id)
        {
            FloorballReferee? referee = await _entities.FindAsync(id);
            if (referee != null)
            {
                await DeleteAsync(referee);
            }
        }

        /// <summary>
        /// Searches for floorball referees by name
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <returns>A collection of floorball referees matching the search term</returns>
        public async Task<IEnumerable<FloorballReferee>> SearchByNameAsync(string searchTerm)
        {
            return await _entities
                .Include(r => r.Person)
                .Where(r => r.Person.FirstName.Contains(searchTerm) || 
                           r.Person.LastName.Contains(searchTerm))
                .ToListAsync();
        }

        /// <summary>
        /// Checks if a floorball referee exists
        /// </summary>
        /// <param name="id">The referee ID</param>
        /// <returns>True if the referee exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _entities.AnyAsync(r => r.Id == id);
        }
    }
} 
