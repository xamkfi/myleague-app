using Domain.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories;

namespace MyLeague.Infrastructure.Persistence.Repositories.Floorball
{
    /// <summary>
    /// Implementation of the floorball coach repository
    /// </summary>
    public class FloorballCoachRepository : RepositoryBase<FloorballCoach, FloorballDbContext>, IFloorballCoachRepository
    {
        /// <summary>
        /// Initializes a new instance of the FloorballCoachRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public FloorballCoachRepository(FloorballDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a floorball coach by Person ID
        /// </summary>
        /// <param name="personId">The person ID</param>
        /// <returns>The coach if found, null otherwise</returns>
        public async Task<FloorballCoach?> GetByPersonIdAsync(Guid personId)
        {
            return await _entities
                .FirstOrDefaultAsync(c => c.PersonId == personId);
        }

        /// <summary>
        /// Gets paginated floorball coaches with filtering support
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="isActive">Optional active status filter</param>
        /// <param name="specialization">Optional specialization filter</param>
        /// <param name="certificationLevel">Optional certification level filter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated collection of floorball coaches</returns>
        public async Task<PagedResult<FloorballCoach>> GetPagedAsync(
            int page, 
            int pageSize, 
            bool? isActive = null,
            string? specialization = null,
            string? certificationLevel = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FloorballCoach> query = _entities.AsQueryable();

            // Apply filters
            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            if (!string.IsNullOrEmpty(specialization))
            {
                query = query.Where(c => c.Specialization == specialization);
            }

            if (!string.IsNullOrEmpty(certificationLevel))
            {
                query = query.Where(c => c.CertificationLevel == certificationLevel);
            }

            // Apply ordering by years of experience (descending)
            query = query.OrderByDescending(c => c.YearsOfExperience);

            // Get total count before pagination
            int totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            List<FloorballCoach> items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PagedResult.Create(items, totalCount, page, pageSize);
        }

        /// <summary>
        /// Gets active floorball coaches
        /// </summary>
        /// <returns>A collection of active floorball coaches</returns>
        public async Task<IEnumerable<FloorballCoach>> GetActiveAsync()
        {
            return await _entities
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.YearsOfExperience)
                .ToListAsync();
        }

        /// <summary>
        /// Gets floorball coaches by specialization
        /// </summary>
        /// <param name="specialization">The specialization to filter by</param>
        /// <returns>A collection of floorball coaches with the specified specialization</returns>
        public async Task<IEnumerable<FloorballCoach>> GetBySpecializationAsync(string specialization)
        {
            return await _entities
                .Where(c => c.Specialization == specialization)
                .OrderByDescending(c => c.YearsOfExperience)
                .ToListAsync();
        }

        /// <summary>
        /// Gets floorball coaches by certification level
        /// </summary>
        /// <param name="certificationLevel">The certification level to filter by</param>
        /// <returns>A collection of floorball coaches with the specified certification level</returns>
        public async Task<IEnumerable<FloorballCoach>> GetByCertificationLevelAsync(string certificationLevel)
        {
            return await _entities
                .Where(c => c.CertificationLevel == certificationLevel)
                .OrderByDescending(c => c.YearsOfExperience)
                .ToListAsync();
        }

        /// <summary>
        /// Deletes a floorball coach
        /// </summary>
        /// <param name="id">The ID of the coach to delete</param>
        public async Task DeleteAsync(Guid id)
        {
            FloorballCoach? coach = await GetByIdAsync(id);
            if (coach != null)
            {
                _entities.Remove(coach);
            }
        }

        /// <summary>
        /// Checks if a floorball coach exists
        /// </summary>
        /// <param name="id">The coach ID</param>
        /// <returns>True if the coach exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _entities.AnyAsync(c => c.Id == id);
        }

        /// <summary>
        /// Checks if a floorball coach exists for a specific person
        /// </summary>
        /// <param name="personId">The person ID</param>
        /// <returns>True if a coach profile exists for the person, false otherwise</returns>
        public async Task<bool> ExistsByPersonIdAsync(Guid personId)
        {
            return await _entities.AnyAsync(c => c.PersonId == personId);
        }
    }
} 