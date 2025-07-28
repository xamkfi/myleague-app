using Domain.Entities.Common;

namespace Domain.Repositories.Common;

/// <summary>
/// Repository for managing clubs
/// </summary>
public interface IClubRepository
{
    /// <summary>
    /// Gets a club by ID
    /// </summary>
    /// <param name="id">The club ID</param>
    /// <returns>The club if found, null otherwise</returns>
    Task<Club?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets multiple clubs by their IDs.
    /// </summary>
    /// <param name="ids">The collection of club IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A dictionary mapping club IDs to their respective clubs.</returns>
    Task<Dictionary<Guid, Club>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a club by name
    /// </summary>
    /// <param name="name">The club name</param>
    /// <returns>The club if found, null otherwise</returns>
    Task<Club?> GetByNameAsync(string name);
    
    /// <summary>
    /// Gets all clubs
    /// </summary>
    /// <returns>A collection of all clubs</returns>
    Task<IEnumerable<Club>> GetAllAsync();
    
    /// <summary>
    /// Gets clubs by country
    /// </summary>
    /// <param name="country">The country to filter by</param>
    /// <returns>A collection of clubs in the specified country</returns>
    Task<IEnumerable<Club>> GetByCountryAsync(string country);
    
    /// <summary>
    /// Gets clubs by city
    /// </summary>
    /// <param name="city">The city to filter by</param>
    /// <returns>A collection of clubs in the specified city</returns>
    Task<IEnumerable<Club>> GetByCityAsync(string city);
    
    /// <summary>
    /// Adds a new club
    /// </summary>
    /// <param name="club">The club to add</param>
    Task AddAsync(Club club);
    
    /// <summary>
    /// Updates an existing club
    /// </summary>
    /// <param name="club">The club to update</param>
    Task UpdateAsync(Club club);
    
    /// <summary>
    /// Deletes a club
    /// </summary>
    /// <param name="id">The ID of the club to delete</param>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Searches for clubs by name, returning a specified number of results.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="count">The maximum number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of clubs matching the search term.</returns>
    Task<IEnumerable<Club>> SearchByNameAsync(string searchTerm, int count, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a club exists
    /// </summary>
    /// <param name="id">The club ID</param>
    /// <returns>True if the club exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);
    
    /// <summary>
    /// Checks if a club with the given name exists
    /// </summary>
    /// <param name="name">The club name</param>
    /// <returns>True if a club with the name exists, false otherwise</returns>
    Task<bool> ExistsByNameAsync(string name);
} 