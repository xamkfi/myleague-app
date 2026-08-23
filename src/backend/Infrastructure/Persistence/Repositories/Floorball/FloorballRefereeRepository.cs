using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Floorball
{
    /// <summary>
    /// Implementation of the floorball referee repository
    /// </summary>
    public class FloorballRefereeRepository : RepositoryBase<FloorballReferee, FloorballDbContext>, IFloorballRefereeRepository
    {
        /// <summary>
        /// Initializes a new instance of the FloorballRefereeRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public FloorballRefereeRepository(FloorballDbContext dbContext) : base(dbContext)
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
        /// Gets paginated floorball referees with filtering support
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="isActive">Optional active status filter</param>
        /// <param name="searchTerm">Optional search term for referee names</param>
        /// <param name="licenseExpiringWithinDays">Optional filter for referees with license expiring within specified days</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated collection of floorball referees</returns>
        public async Task<PagedResult<FloorballReferee>> GetPagedAsync(
            int page, 
            int pageSize, 
            bool? isActive = null,
            string? searchTerm = null,
            int? licenseExpiringWithinDays = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FloorballReferee> query = _entities.AsQueryable();

            // Apply active status filter
            if (isActive.HasValue)
            {
                query = query.Where(r => r.IsActive == isActive.Value);
            }

            // Apply search term filter (search in Person names - note: Person is ignored in EF config)
            // Since Person navigation is ignored, we'll search by PersonId for now
            // In a real implementation, you might want to join with Person table or use a different approach
            if (!string.IsNullOrEmpty(searchTerm))
            {
                // For now, we'll skip the name search since Person navigation is ignored
                // This could be enhanced by joining with the Person table from CommonDbContext
                // or by storing referee name denormalized in the FloorballReferee entity
            }

            // Apply license expiring filter
            if (licenseExpiringWithinDays.HasValue)
            {
                DateTime cutoffDate = DateTime.UtcNow.AddDays(licenseExpiringWithinDays.Value);
                query = query.Where(r => r.LicenseExpiryDate <= cutoffDate && r.IsActive);
            }

            // Apply default ordering (by license expiry date, then by matches officiated)
            query = query.OrderBy(r => r.LicenseExpiryDate ?? DateTime.MaxValue)
                        .ThenByDescending(r => r.MatchesOfficiated);

            // Get total count before pagination
            int totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            List<FloorballReferee> items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PagedResult.Create(items, totalCount, page, pageSize);
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
        /// Adds a new floorball referee
        /// </summary>
        /// <param name="referee">The referee to add</param>
        public override async Task AddAsync(FloorballReferee referee)
        {
            await base.AddAsync(referee);
        }

        /// <summary>
        /// Updates an existing floorball referee
        /// </summary>
        /// <param name="referee">The referee to update</param>
        public override async Task UpdateAsync(FloorballReferee referee)
        {
            await base.UpdateAsync(referee);
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

        public async Task<FloorballReferee?> GetByPersonIdAsync(Guid personId)
        {
            return await _entities.FirstOrDefaultAsync(r => r.PersonId == personId);
        }

        public async Task<bool> IsAssignedToAnyMatchAsync(Guid refereeId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Dictionary<string, object>>("FloorballMatchOfficial")
                .AnyAsync(row => EF.Property<Guid>(row, "OfficialsId") == refereeId, cancellationToken);
        }
    }
} 
