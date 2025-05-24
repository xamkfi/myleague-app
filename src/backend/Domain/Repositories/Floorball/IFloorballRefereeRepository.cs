using Domain.Entities.Floorball;

namespace Domain.Repositories.Floorball;

/// <summary>
/// Repository for managing floorball referees
/// </summary>
public interface IFloorballRefereeRepository
{
    /// <summary>
    /// Gets a floorball referee by ID
    /// </summary>
    /// <param name="id">The referee ID</param>
    /// <returns>The referee if found, null otherwise</returns>
    Task<FloorballReferee?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets all floorball referees
    /// </summary>
    /// <returns>A collection of all floorball referees</returns>
    Task<IEnumerable<FloorballReferee>> GetAllAsync();
    
    /// <summary>
    /// Gets all active floorball referees
    /// </summary>
    /// <returns>A collection of active floorball referees</returns>
    Task<IEnumerable<FloorballReferee>> GetActiveAsync();
    
    /// <summary>
    /// Gets floorball referees by match ID
    /// </summary>
    /// <param name="matchId">The match ID</param>
    /// <returns>A collection of referees assigned to the match</returns>
    Task<IEnumerable<FloorballReferee>> GetByMatchIdAsync(Guid matchId);
    
    /// <summary>
    /// Gets floorball referees whose license is expiring soon
    /// </summary>
    /// <param name="withinDays">Days until expiry</param>
    /// <returns>A collection of referees whose license is expiring soon</returns>
    Task<IEnumerable<FloorballReferee>> GetWithExpiringLicenseAsync(int withinDays);
    
    /// <summary>
    /// Gets floorball referees ordered by number of matches officiated
    /// </summary>
    /// <param name="count">Maximum number of referees to return</param>
    /// <returns>The most experienced referees</returns>
    Task<IEnumerable<FloorballReferee>> GetMostExperiencedAsync(int count = 10);
    
    /// <summary>
    /// Adds a new floorball referee
    /// </summary>
    /// <param name="referee">The referee to add</param>
    Task AddAsync(FloorballReferee referee);
    
    /// <summary>
    /// Updates an existing floorball referee
    /// </summary>
    /// <param name="referee">The referee to update</param>
    Task UpdateAsync(FloorballReferee referee);
    
    /// <summary>
    /// Deletes a floorball referee
    /// </summary>
    /// <param name="id">The ID of the referee to delete</param>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Searches for floorball referees by name
    /// </summary>
    /// <param name="searchTerm">The search term</param>
    /// <returns>A collection of floorball referees matching the search term</returns>
    Task<IEnumerable<FloorballReferee>> SearchByNameAsync(string searchTerm);
    
    /// <summary>
    /// Checks if a floorball referee exists
    /// </summary>
    /// <param name="id">The referee ID</param>
    /// <returns>True if the referee exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);
} 
