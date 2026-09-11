using Domain.Common;
using Domain.Entities.Floorball;

namespace Domain.Repositories.Floorball;

/// <summary>
/// Repository for managing floorball team managers
/// </summary>
public interface IFloorballTeamManagerRepository
{
    /// <summary>
    /// Gets a floorball team manager by ID
    /// </summary>
    /// <param name="id">The team manager ID</param>
    /// <returns>The team manager if found, null otherwise</returns>
    Task<FloorballTeamManager?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets a floorball team manager by Person ID
    /// </summary>
    /// <param name="personId">The person ID</param>
    /// <returns>The team manager if found, null otherwise</returns>
    Task<FloorballTeamManager?> GetByPersonIdAsync(Guid personId);

    /// <summary>
    /// Gets all floorball team manager rows for a person
    /// </summary>
    /// <param name="personId">The person ID</param>
    /// <returns>All team manager rows for the person</returns>
    Task<IEnumerable<FloorballTeamManager>> GetAllByPersonIdAsync(Guid personId);

    /// <summary>
    /// Checks whether a person is an active manager of a specific team
    /// </summary>
    /// <param name="personId">The person ID</param>
    /// <param name="teamId">The team ID</param>
    /// <returns>True if an active manager row exists for the person and team</returns>
    Task<bool> IsActiveManagerOfTeamAsync(Guid personId, Guid teamId);

    /// <summary>
    /// Gets a floorball team manager row for a specific person and team
    /// </summary>
    /// <param name="personId">The person ID</param>
    /// <param name="teamId">The team ID</param>
    /// <returns>The team manager row if found, null otherwise</returns>
    Task<FloorballTeamManager?> GetByPersonAndTeamAsync(Guid personId, Guid teamId);
    
    /// <summary>
    /// Gets all floorball team managers
    /// </summary>
    /// <returns>A collection of all floorball team managers</returns>
    Task<IEnumerable<FloorballTeamManager>> GetAllAsync();
    
    /// <summary>
    /// Gets paginated floorball team managers with filtering support
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="isActive">Optional active status filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of floorball team managers</returns>
    Task<PagedResult<FloorballTeamManager>> GetPagedAsync(
        int page, 
        int pageSize, 
        bool? isActive = null,
        CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Gets active floorball team managers
    /// </summary>
    /// <returns>A collection of active floorball team managers</returns>
    Task<IEnumerable<FloorballTeamManager>> GetActiveAsync();
    
    /// <summary>
    /// Adds a new floorball team manager
    /// </summary>
    /// <param name="teamManager">The team manager to add</param>
    Task AddAsync(FloorballTeamManager teamManager);
    
    /// <summary>
    /// Updates an existing floorball team manager
    /// </summary>
    /// <param name="teamManager">The team manager to update</param>
    Task UpdateAsync(FloorballTeamManager teamManager);
    
    /// <summary>
    /// Deletes a floorball team manager
    /// </summary>
    /// <param name="id">The ID of the team manager to delete</param>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Checks if a floorball team manager exists
    /// </summary>
    /// <param name="id">The team manager ID</param>
    /// <returns>True if the team manager exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);
    
    /// <summary>
    /// Checks if a floorball team manager exists for a specific person
    /// </summary>
    /// <param name="personId">The person ID</param>
    /// <returns>True if a team manager profile exists for the person, false otherwise</returns>
    Task<bool> ExistsByPersonIdAsync(Guid personId);
} 