using Domain.Entities.Hockey;
using Domain.Enums.Hockey;

namespace Domain.Repositories.Hockey;

/// <summary>
/// Repository for managing Hockey players
/// </summary>
public interface IHockeyPlayerRepository
{
    /// <summary>
    /// Gets a Hockey player by ID
    /// </summary>
    /// <param name="id">The player ID</param>
    /// <returns>The player if found, null otherwise</returns>
    Task<HockeyPlayer> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all Hockey players
    /// </summary>
    /// <returns>A collection of all Hockey players</returns>
    Task<IEnumerable<HockeyPlayer>> GetAllAsync();

    /// <summary>
    /// Gets Hockey players by team ID
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <returns>A collection of Hockey players in the team</returns>
    Task<IEnumerable<HockeyPlayer>> GetByTeamIdAsync(Guid teamId);

    /// <summary>
    /// Gets active Hockey players by position
    /// </summary>
    /// <param name="position">The position to filter by</param>
    /// <returns>A collection of active Hockey players playing the specified position</returns>
    Task<IEnumerable<HockeyPlayer>> GetActiveByPositionAsync(HockeyPosition position);

    /// <summary>
    /// Gets top scorers for the specified season
    /// </summary>
    /// <param name="seasonId">The season ID</param>
    /// <param name="count">Maximum number of players to return</param>
    /// <returns>A collection of top scoring players in the season</returns>
    Task<IEnumerable<HockeyPlayer>> GetTopScorersAsync(Guid seasonId, int count = 10);

    /// <summary>
    /// Gets top assisters for the specified season
    /// </summary>
    /// <param name="seasonId">The season ID</param>
    /// <param name="count">Maximum number of players to return</param>
    /// <returns>A collection of top assisting players in the season</returns>
    Task<IEnumerable<HockeyPlayer>> GetTopAssistersAsync(Guid seasonId, int count = 10);

    /// <summary>
    /// Adds a new Hockey player
    /// </summary>
    /// <param name="player">The player to add</param>
    Task AddAsync(HockeyPlayer player);

    /// <summary>
    /// Updates an existing Hockey player
    /// </summary>
    /// <param name="player">The player to update</param>
    Task UpdateAsync(HockeyPlayer player);

    /// <summary>
    /// Deletes a Hockey player
    /// </summary>
    /// <param name="id">The ID of the player to delete</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Searches for Hockey players by name
    /// </summary>
    /// <param name="searchTerm">The search term</param>
    /// <returns>A collection of Hockey players matching the search term</returns>
    Task<IEnumerable<HockeyPlayer>> SearchByNameAsync(string searchTerm);

    /// <summary>
    /// Checks if a Hockey player exists
    /// </summary>
    /// <param name="id">The player ID</param>
    /// <returns>True if the player exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);
}
