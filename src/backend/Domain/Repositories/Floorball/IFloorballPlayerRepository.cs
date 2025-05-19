using Domain2.Entities.Floorball;
using Domain2.Enums.Floorball;

namespace Domain2.Repositories.Floorball;

/// <summary>
/// Repository for managing floorball players
/// </summary>
public interface IFloorballPlayerRepository
{
    /// <summary>
    /// Gets a floorball player by ID
    /// </summary>
    /// <param name="id">The player ID</param>
    /// <returns>The player if found, null otherwise</returns>
    Task<FloorballPlayer> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets all floorball players
    /// </summary>
    /// <returns>A collection of all floorball players</returns>
    Task<IEnumerable<FloorballPlayer>> GetAllAsync();
    
    /// <summary>
    /// Gets floorball players by team ID
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <returns>A collection of floorball players in the team</returns>
    Task<IEnumerable<FloorballPlayer>> GetByTeamIdAsync(Guid teamId);
    
    /// <summary>
    /// Gets active floorball players by position
    /// </summary>
    /// <param name="position">The position to filter by</param>
    /// <returns>A collection of active floorball players playing the specified position</returns>
    Task<IEnumerable<FloorballPlayer>> GetActiveByPositionAsync(FloorballPosition position);
    
    /// <summary>
    /// Gets top scorers for the specified season
    /// </summary>
    /// <param name="seasonId">The season ID</param>
    /// <param name="count">Maximum number of players to return</param>
    /// <returns>A collection of top scoring players in the season</returns>
    Task<IEnumerable<FloorballPlayer>> GetTopScorersAsync(Guid seasonId, int count = 10);
    
    /// <summary>
    /// Gets top assisters for the specified season
    /// </summary>
    /// <param name="seasonId">The season ID</param>
    /// <param name="count">Maximum number of players to return</param>
    /// <returns>A collection of top assisting players in the season</returns>
    Task<IEnumerable<FloorballPlayer>> GetTopAssistersAsync(Guid seasonId, int count = 10);
    
    /// <summary>
    /// Adds a new floorball player
    /// </summary>
    /// <param name="player">The player to add</param>
    Task AddAsync(FloorballPlayer player);
    
    /// <summary>
    /// Updates an existing floorball player
    /// </summary>
    /// <param name="player">The player to update</param>
    Task UpdateAsync(FloorballPlayer player);
    
    /// <summary>
    /// Deletes a floorball player
    /// </summary>
    /// <param name="id">The ID of the player to delete</param>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Searches for floorball players by name
    /// </summary>
    /// <param name="searchTerm">The search term</param>
    /// <returns>A collection of floorball players matching the search term</returns>
    Task<IEnumerable<FloorballPlayer>> SearchByNameAsync(string searchTerm);

    /// <summary>
    /// Checks if a floorball player exists
    /// </summary>
    /// <param name="id">The player ID</param>
    /// <returns>True if the player exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);
} 