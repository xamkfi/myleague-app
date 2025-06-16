using Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MyLeague.Infrastructure.Persistence;

/// <summary>
/// Base repository class providing common functionality including pagination
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TContext">The database context type</typeparam>
public abstract class PaginatedRepositoryBase<TEntity, TContext>
    where TEntity : class
    where TContext : DbContext
{
    protected readonly TContext _context;
    protected readonly DbSet<TEntity> _entities;

    /// <summary>
    /// Initializes a new instance of the PaginatedRepositoryBase class
    /// </summary>
    /// <param name="context">The database context</param>
    protected PaginatedRepositoryBase(TContext context)
    {
        _context = context;
        _entities = context.Set<TEntity>();
    }

    /// <summary>
    /// Gets a paginated result from a queryable with optional ordering
    /// </summary>
    /// <typeparam name="TKey">The type of the ordering key</typeparam>
    /// <param name="query">The base query to paginate</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="orderBy">Optional ordering expression</param>
    /// <param name="orderDescending">Whether to order in descending order</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated result</returns>
    protected async Task<PagedResult<TEntity>> GetPagedAsync<TKey>(
        IQueryable<TEntity> query,
        int page,
        int pageSize,
        Expression<Func<TEntity, TKey>>? orderBy = null,
        bool orderDescending = false,
        CancellationToken cancellationToken = default)
    {
        // Apply ordering if specified
        if (orderBy != null)
        {
            query = orderDescending 
                ? query.OrderByDescending(orderBy)
                : query.OrderBy(orderBy);
        }

        // Get total count before pagination
        int totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        List<TEntity> items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult.Create(items, totalCount, page, pageSize);
    }

    /// <summary>
    /// Gets the count of items matching the query
    /// </summary>
    /// <param name="query">The query to count</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of matching items</returns>
    protected async Task<int> GetCountAsync(
        IQueryable<TEntity> query,
        CancellationToken cancellationToken = default)
    {
        return await query.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Builds a filtered query based on the provided conditions
    /// </summary>
    /// <param name="baseQuery">The base query to filter</param>
    /// <param name="filters">Collection of filter expressions</param>
    /// <returns>Filtered query</returns>
    protected IQueryable<TEntity> ApplyFilters(
        IQueryable<TEntity> baseQuery,
        params Expression<Func<TEntity, bool>>[] filters)
    {
        return filters.Aggregate(baseQuery, (current, filter) => current.Where(filter));
    }

    /// <summary>
    /// Virtual methods for basic CRUD operations
    /// </summary>
    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await _entities.FindAsync(id);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _entities.ToListAsync();
    }

    public virtual async Task AddAsync(TEntity entity)
    {
        await _entities.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        _entities.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(TEntity entity)
    {
        _entities.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        return await _entities.FindAsync(id) != null;
    }
} 