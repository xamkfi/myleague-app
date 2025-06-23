using Domain.Common;
using Domain.Entities.Floorball;

namespace Domain.Repositories.Floorball;

/// <summary>
/// Repository for managing floorball coaches
/// </summary>
public interface IFloorballCoachRepository
{
    /// <summary>
    /// Gets a floorball coach by ID
    /// </summary>
    /// <param name="id">The coach ID</param>
    /// <returns>The coach if found, null otherwise</returns>
    Task<FloorballCoach?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets a floorball coach by Person ID
    /// </summary>
    /// <param name="personId">The person ID</param>
    /// <returns>The coach if found, null otherwise</returns>
    Task<FloorballCoach?> GetByPersonIdAsync(Guid personId);
    
    /// <summary>
    /// Gets all floorball coaches
    /// </summary>
    /// <returns>A collection of all floorball coaches</returns>
    Task<IEnumerable<FloorballCoach>> GetAllAsync();
    
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
    Task<PagedResult<FloorballCoach>> GetPagedAsync(
        int page, 
        int pageSize, 
        bool? isActive = null,
        string? specialization = null,
        string? certificationLevel = null,
        CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Gets active floorball coaches
    /// </summary>
    /// <returns>A collection of active floorball coaches</returns>
    Task<IEnumerable<FloorballCoach>> GetActiveAsync();
    
    /// <summary>
    /// Gets floorball coaches by specialization
    /// </summary>
    /// <param name="specialization">The specialization to filter by</param>
    /// <returns>A collection of floorball coaches with the specified specialization</returns>
    Task<IEnumerable<FloorballCoach>> GetBySpecializationAsync(string specialization);
    
    /// <summary>
    /// Gets floorball coaches by certification level
    /// </summary>
    /// <param name="certificationLevel">The certification level to filter by</param>
    /// <returns>A collection of floorball coaches with the specified certification level</returns>
    Task<IEnumerable<FloorballCoach>> GetByCertificationLevelAsync(string certificationLevel);
    
    /// <summary>
    /// Adds a new floorball coach
    /// </summary>
    /// <param name="coach">The coach to add</param>
    Task AddAsync(FloorballCoach coach);
    
    /// <summary>
    /// Updates an existing floorball coach
    /// </summary>
    /// <param name="coach">The coach to update</param>
    Task UpdateAsync(FloorballCoach coach);
    
    /// <summary>
    /// Deletes a floorball coach
    /// </summary>
    /// <param name="id">The ID of the coach to delete</param>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Checks if a floorball coach exists
    /// </summary>
    /// <param name="id">The coach ID</param>
    /// <returns>True if the coach exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);
    
    /// <summary>
    /// Checks if a floorball coach exists for a specific person
    /// </summary>
    /// <param name="personId">The person ID</param>
    /// <returns>True if a coach profile exists for the person, false otherwise</returns>
    Task<bool> ExistsByPersonIdAsync(Guid personId);
} 