using Domain.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories;

namespace MyLeague.Infrastructure.Persistence.Repositories.Floorball
{
    /// <summary>
    /// Implementation of the floorball team manager repository
    /// </summary>
    public class FloorballTeamManagerRepository : RepositoryBase<FloorballTeamManager, FloorballDbContext>, IFloorballTeamManagerRepository
    {
        /// <summary>
        /// Initializes a new instance of the FloorballTeamManagerRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public FloorballTeamManagerRepository(FloorballDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a floorball team manager by Person ID
        /// </summary>
        /// <param name="personId">The person ID</param>
        /// <returns>The team manager if found, null otherwise</returns>
        public async Task<FloorballTeamManager?> GetByPersonIdAsync(Guid personId)
        {
            return await _entities
                .FirstOrDefaultAsync(tm => tm.PersonId == personId);
        }

        /// <summary>
        /// Gets all floorball team manager rows for a person
        /// </summary>
        /// <param name="personId">The person ID</param>
        /// <returns>All team manager rows for the person</returns>
        public async Task<IEnumerable<FloorballTeamManager>> GetAllByPersonIdAsync(Guid personId)
        {
            return await _entities
                .Where(tm => tm.PersonId == personId)
                .ToListAsync();
        }

        /// <summary>
        /// Checks whether a person is an active manager of a specific team
        /// </summary>
        /// <param name="personId">The person ID</param>
        /// <param name="teamId">The team ID</param>
        /// <returns>True if an active manager row exists for the person and team</returns>
        public async Task<bool> IsActiveManagerOfTeamAsync(Guid personId, Guid teamId)
        {
            return await _entities
                .AnyAsync(tm => tm.PersonId == personId && tm.TeamId == teamId && tm.IsActive);
        }

        /// <summary>
        /// Gets a floorball team manager row for a specific person and team
        /// </summary>
        /// <param name="personId">The person ID</param>
        /// <param name="teamId">The team ID</param>
        /// <returns>The team manager row if found, null otherwise</returns>
        public async Task<FloorballTeamManager?> GetByPersonAndTeamAsync(Guid personId, Guid teamId)
        {
            return await _entities
                .FirstOrDefaultAsync(tm => tm.PersonId == personId && tm.TeamId == teamId);
        }

        /// <summary>
        /// Gets paginated floorball team managers with filtering support
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="isActive">Optional active status filter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated collection of floorball team managers</returns>
        public async Task<PagedResult<FloorballTeamManager>> GetPagedAsync(
            int page, 
            int pageSize, 
            bool? isActive = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FloorballTeamManager> query = _entities.AsQueryable();

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
            List<FloorballTeamManager> items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PagedResult.Create(items, totalCount, page, pageSize);
        }

        /// <summary>
        /// Gets active floorball team managers
        /// </summary>
        /// <returns>A collection of active floorball team managers</returns>
        public async Task<IEnumerable<FloorballTeamManager>> GetActiveAsync()
        {
            return await _entities
                .Where(tm => tm.IsActive)
                .OrderBy(tm => tm.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Deletes a floorball team manager
        /// </summary>
        /// <param name="id">The ID of the team manager to delete</param>
        public async Task DeleteAsync(Guid id)
        {
            FloorballTeamManager? teamManager = await GetByIdAsync(id);
            if (teamManager != null)
            {
                _entities.Remove(teamManager);
            }
        }

        /// <summary>
        /// Checks if a floorball team manager exists
        /// </summary>
        /// <param name="id">The team manager ID</param>
        /// <returns>True if the team manager exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _entities.AnyAsync(tm => tm.Id == id);
        }

        /// <summary>
        /// Checks if a floorball team manager exists for a specific person
        /// </summary>
        /// <param name="personId">The person ID</param>
        /// <returns>True if a team manager profile exists for the person, false otherwise</returns>
        public async Task<bool> ExistsByPersonIdAsync(Guid personId)
        {
            return await _entities.AnyAsync(tm => tm.PersonId == personId);
        }
    }
} 