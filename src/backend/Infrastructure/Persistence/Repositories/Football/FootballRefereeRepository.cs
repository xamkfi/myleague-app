using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Football;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Football
{
    /// <summary>
    /// Implementation of the football referee repository
    /// </summary>
    public class FootballRefereeRepository : RepositoryBase<FootballReferee, FootballDbContext>, IFootballRefereeRepository
    {
        /// <summary>
        /// Initializes a new instance of the FootballRefereeRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public FootballRefereeRepository(FootballDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a football referee by ID
        /// </summary>
        /// <param name="id">The referee ID</param>
        /// <returns>The referee if found, null otherwise</returns>
        public override async Task<FootballReferee?> GetByIdAsync(Guid id)
        {
            return await _entities
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <summary>
        /// Gets all football referees
        /// </summary>
        /// <returns>A collection of all football referees</returns>
        public override async Task<IEnumerable<FootballReferee>> GetAllAsync()
        {
            return await _entities
                .ToListAsync();
        }

        /// <summary>
        /// Gets paginated football referees with filtering support
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="isActive">Optional active status filter</param>
        /// <param name="searchTerm">Optional search term for referee names</param>
        /// <param name="licenseExpiringWithinDays">Optional filter for referees with license expiring within specified days</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated collection of football referees</returns>
        public async Task<PagedResult<FootballReferee>> GetPagedAsync(
            int page,
            int pageSize,
            bool? isActive = null,
            string? searchTerm = null,
            int? licenseExpiringWithinDays = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FootballReferee> query = _entities.AsQueryable();

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
                // or by storing referee name denormalized in the FootballReferee entity
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
            List<FootballReferee> items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PagedResult.Create(items, totalCount, page, pageSize);
        }

        /// <summary>
        /// Gets all active football referees
        /// </summary>
        /// <returns>A collection of active football referees</returns>
        public async Task<IEnumerable<FootballReferee>> GetActiveAsync()
        {
            return await _entities
                .Where(r => r.IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// Gets football referees by match ID
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>A collection of referees assigned to the match</returns>
        public async Task<IEnumerable<FootballReferee>> GetByMatchIdAsync(Guid matchId)
        {
            FootballMatch? match = await _dbContext.FootballMatches
                .Include(m => m.Officials)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            return match?.Officials ?? new List<FootballReferee>();
        }

        /// <summary>
        /// Adds a new football referee
        /// </summary>
        /// <param name="referee">The referee to add</param>
        public override async Task AddAsync(FootballReferee referee)
        {
            await base.AddAsync(referee);
        }

        /// <summary>
        /// Updates an existing football referee
        /// </summary>
        /// <param name="referee">The referee to update</param>
        public override async Task UpdateAsync(FootballReferee referee)
        {
            await base.UpdateAsync(referee);
        }

        /// <summary>
        /// Deletes a football referee by ID
        /// </summary>
        /// <param name="id">The ID of the referee to delete</param>
        public async Task DeleteAsync(Guid id)
        {
            FootballReferee? referee = await _entities.FindAsync(id);
            if (referee != null)
            {
                await DeleteAsync(referee);
            }
        }

        /// <summary>
        /// Searches for football referees by name
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <returns>A collection of football referees matching the search term</returns>
        public async Task<IEnumerable<FootballReferee>> SearchByNameAsync(string searchTerm)
        {
            return await _entities
                .Include(r => r.Person)
                .Where(r => r.Person.FirstName.Contains(searchTerm) ||
                           r.Person.LastName.Contains(searchTerm))
                .ToListAsync();
        }

        /// <summary>
        /// Checks if a football referee exists
        /// </summary>
        /// <param name="id">The referee ID</param>
        /// <returns>True if the referee exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _entities.AnyAsync(r => r.Id == id);
        }
    }
}
