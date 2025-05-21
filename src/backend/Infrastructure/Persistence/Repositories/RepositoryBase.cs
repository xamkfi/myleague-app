using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;
using System.Linq.Expressions;

namespace MyLeague.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Base repository providing common functionality for all repositories
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    public abstract class RepositoryBase<TEntity> where TEntity : class
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly DbSet<TEntity> _entities;

        /// <summary>
        /// Initializes a new instance of the RepositoryBase class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        protected RepositoryBase(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _entities = dbContext.Set<TEntity>();
        }

        /// <summary>
        /// Gets an entity by its ID
        /// </summary>
        /// <param name="id">The entity ID</param>
        /// <returns>The entity if found, null otherwise</returns>
        public virtual async Task<TEntity?> GetByIdAsync(Guid id)
        {
            return await _entities.FindAsync(id);
        }

        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <returns>A collection of all entities</returns>
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _entities.ToListAsync();
        }

        /// <summary>
        /// Finds entities matching a predicate
        /// </summary>
        /// <param name="predicate">The predicate to match</param>
        /// <returns>A collection of matching entities</returns>
        public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _entities.Where(predicate).ToListAsync();
        }

        /// <summary>
        /// Adds a new entity
        /// </summary>
        /// <param name="entity">The entity to add</param>
        public virtual async Task AddAsync(TEntity entity)
        {
            await _entities.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">The entity to update</param>
        public virtual async Task UpdateAsync(TEntity entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        public virtual async Task DeleteAsync(TEntity entity)
        {
            _entities.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Checks if an entity exists
        /// </summary>
        /// <param name="predicate">The predicate to match</param>
        /// <returns>True if an entity matching the predicate exists, false otherwise</returns>
        public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _entities.AnyAsync(predicate);
        }
    }
} 
