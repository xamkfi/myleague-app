using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common
{
    /// <summary>
    /// Implementation of the division repository
    /// </summary>
    public class DivisionRepository : RepositoryBase<Division, CommonDbContext>, IDivisionRepository
    {
        /// <summary>
        /// Initializes a new instance of the DivisionRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public DivisionRepository(CommonDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a division by ID
        /// </summary>
        /// <param name="id">The division ID</param>
        /// <returns>The division if found, null otherwise</returns>
        public override async Task<Division?> GetByIdAsync(Guid id)
        {
            return await _entities
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        /// <summary>
        /// Gets all divisions
        /// </summary>
        /// <returns>A collection of all divisions</returns>
        public override async Task<IEnumerable<Division>> GetAllAsync()
        {
            return await _entities
                .OrderBy(d => d.SportType)
                .ThenBy(d => d.Level)
                .ThenBy(d => d.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Gets divisions by sport type
        /// </summary>
        /// <param name="sportType">The sport type to filter by</param>
        /// <returns>A collection of divisions for the specified sport type</returns>
        public async Task<IEnumerable<Division>> GetBySportTypeAsync(SportsCategory sportType)
        {
            return await _entities
                .Where(d => d.SportType == sportType)
                .OrderBy(d => d.Level)
                .ThenBy(d => d.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Gets active divisions by sport type
        /// </summary>
        /// <param name="sportType">The sport type to filter by</param>
        /// <returns>A collection of active divisions for the specified sport type</returns>
        public async Task<IEnumerable<Division>> GetActiveBySportTypeAsync(SportsCategory sportType)
        {
            return await _entities
                .Where(d => d.SportType == sportType && d.IsActive)
                .OrderBy(d => d.Level)
                .ThenBy(d => d.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a division by name and sport type
        /// </summary>
        /// <param name="name">The name of the division</param>
        /// <param name="sportType">The sport type</param>
        /// <returns>The division if found, null otherwise</returns>
        public async Task<Division?> GetByNameAndSportTypeAsync(string name, SportsCategory sportType)
        {
            return await _entities
                .FirstOrDefaultAsync(d => d.Name == name && d.SportType == sportType);
        }

        /// <summary>
        /// Adds a new division
        /// </summary>
        /// <param name="division">The division to add</param>
        public override async Task AddAsync(Division division)
        {
            await _entities.AddAsync(division);
        }

        /// <summary>
        /// Updates an existing division
        /// </summary>
        /// <param name="division">The division to update</param>
        public override Task UpdateAsync(Division division)
        {
            _entities.Update(division);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Deletes a division
        /// </summary>
        /// <param name="division">The division to delete</param>
        public override async Task DeleteAsync(Division division)
        {
            _entities.Remove(division);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Checks if a division exists by name and sport type
        /// </summary>
        /// <param name="name">The name of the division</param>
        /// <param name="sportType">The sport type</param>
        /// <returns>True if the division exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(string name, SportsCategory sportType)
        {
            return await _entities
                .AnyAsync(d => d.Name == name && d.SportType == sportType);
        }
    }
} 
