using Domain.Common;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Football;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories;

namespace MyLeague.Infrastructure.Persistence.Repositories.Football
{
    /// <summary>
    /// Implementation of the football team manager repository
    /// </summary>
    public class FootballTeamManagerRepository : RepositoryBase<FootballTeamManager, FootballDbContext>, IFootballTeamManagerRepository
    {
        /// <summary>
        /// Initializes a new instance of the FootballTeamManagerRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public FootballTeamManagerRepository(FootballDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a football team manager by Person ID
        /// </summary>
        /// <param name="personId">The person ID</param>
        /// <returns>The team manager if found, null otherwise</returns>
        public async Task<FootballTeamManager?> GetByPersonIdAsync(Guid personId)
        {
            return await _entities
                .FirstOrDefaultAsync(tm => tm.PersonId == personId);
        }

        /// <summary>
        /// Gets paginated football team managers with filtering support
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="isActive">Optional active status filter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated collection of football team managers</returns>
        public async Task<PagedResult<FootballTeamManager>> GetPagedAsync(
            int page,
            int pageSize,
            bool? isActive = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FootballTeamManager> query = _entities.AsQueryable();

            // Apply filters
            if (isActive.HasValue)
            {
                query = query.Where(tm => tm.IsActive == isActive.Value);
            }

            // Apply ordering by ID (for consistent ordering)
            query = query.OrderBy(tm => tm.Id);

            // Get total count before pagination
            int totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            List<FootballTeamManager> items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PagedResult.Create(items, totalCount, page, pageSize);
        }

        /// <summary>
        /// Gets active football team managers
        /// </summary>
        /// <returns>A collection of active football team managers</returns>
        public async Task<IEnumerable<FootballTeamManager>> GetActiveAsync()
        {
            return await _entities
                .Where(tm => tm.IsActive)
                .OrderBy(tm => tm.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Deletes a football team manager
        /// </summary>
        /// <param name="id">The ID of the team manager to delete</param>
        public async Task DeleteAsync(Guid id)
        {
            FootballTeamManager? teamManager = await GetByIdAsync(id);
            if (teamManager != null)
            {
                _entities.Remove(teamManager);
            }
        }

        /// <summary>
        /// Checks if a football team manager exists
        /// </summary>
        /// <param name="id">The team manager ID</param>
        /// <returns>True if the team manager exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _entities.AnyAsync(tm => tm.Id == id);
        }

        /// <summary>
        /// Checks if a football team manager exists for a specific person
        /// </summary>
        /// <param name="personId">The person ID</param>
        /// <returns>True if a team manager profile exists for the person, false otherwise</returns>
        public async Task<bool> ExistsByPersonIdAsync(Guid personId)
        {
            return await _entities.AnyAsync(tm => tm.PersonId == personId);
        }
    }
}
