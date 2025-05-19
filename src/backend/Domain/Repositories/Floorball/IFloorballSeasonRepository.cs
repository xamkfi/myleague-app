using Domain.Entities.Floorball;
using Domain.Enums.Floorball;

namespace Domain.Repositories.Floorball;

/// <summary>
/// Repository for managing floorball seasons
/// </summary>
public interface IFloorballSeasonRepository
{
    /// <summary>
    /// Gets a floorball season by ID
    /// </summary>
    /// <param name="id">The season ID</param>
    /// <returns>The season if found, null otherwise</returns>
    Task<FloorballSeason> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets a floorball season by name
    /// </summary>
    /// <param name="name">The season name</param>
    /// <returns>The season if found, null otherwise</returns>
    Task<FloorballSeason?> GetByNameAsync(string name);
    
    /// <summary>
    /// Gets all floorball seasons
    /// </summary>
    /// <returns>A collection of all floorball seasons</returns>
    Task<IEnumerable<FloorballSeason>> GetAllAsync();
    
    /// <summary>
    /// Gets active floorball seasons
    /// </summary>
    /// <returns>A collection of active floorball seasons</returns>
    Task<IEnumerable<FloorballSeason>> GetActiveAsync();
    
    /// <summary>
    /// Gets completed floorball seasons
    /// </summary>
    /// <returns>A collection of completed floorball seasons</returns>
    Task<IEnumerable<FloorballSeason>> GetCompletedAsync();
    
    /// <summary>
    /// Gets floorball seasons by division
    /// </summary>
    /// <param name="division">The division to filter by</param>
    /// <returns>A collection of floorball seasons for the specified division</returns>
    Task<IEnumerable<FloorballSeason>> GetByDivisionAsync(FloorballDivision division);
    
    /// <summary>
    /// Gets seasons containing a specific team
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <returns>A collection of seasons with the team participating</returns>
    Task<IEnumerable<FloorballSeason>> GetByTeamIdAsync(Guid teamId);
    
    /// <summary>
    /// Gets the current or upcoming season for a division
    /// </summary>
    /// <param name="division">The division</param>
    /// <returns>The current or next season for the division</returns>
    Task<FloorballSeason> GetCurrentOrUpcomingAsync(FloorballDivision division);
    
    /// <summary>
    /// Adds a new floorball season
    /// </summary>
    /// <param name="season">The season to add</param>
    Task AddAsync(FloorballSeason season);
    
    /// <summary>
    /// Updates an existing floorball season
    /// </summary>
    /// <param name="season">The season to update</param>
    Task UpdateAsync(FloorballSeason season);
    
    /// <summary>
    /// Deletes a floorball season
    /// </summary>
    /// <param name="id">The ID of the season to delete</param>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Checks if a floorball season exists
    /// </summary>
    /// <param name="id">The season ID</param>
    /// <returns>True if the season exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);
} 
