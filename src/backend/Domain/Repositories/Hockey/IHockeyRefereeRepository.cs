using Domain.Entities.Hockey;

namespace Domain.Repositories.Hockey;

/// <summary>
/// Repository for managing Hockey referees
/// </summary>
public interface IHockeyRefereeRepository
{
    /// <summary>
    /// Gets a Hockey referee by ID
    /// </summary>
    /// <param name="id">The referee ID</param>
    /// <returns>The referee if found, null otherwise</returns>
    Task<HockeyReferee> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all Hockey referees
    /// </summary>
    /// <returns>A collection of all Hockey referees</returns>
    Task<IEnumerable<HockeyReferee>> GetAllAsync();

    /// <summary>
    /// Gets all active Hockey referees
    /// </summary>
    /// <returns>A collection of active Hockey referees</returns>
    Task<IEnumerable<HockeyReferee>> GetActiveAsync();

    /// <summary>
    /// Gets Hockey referees by match ID
    /// </summary>
    /// <param name="matchId">The match ID</param>
    /// <returns>A collection of referees assigned to the match</returns>
    Task<IEnumerable<HockeyReferee>> GetByMatchIdAsync(Guid matchId);

    /// <summary>
    /// Gets Hockey referees whose license is expiring soon
    /// </summary>
    /// <param name="withinDays">Days until expiry</param>
    /// <returns>A collection of referees whose license is expiring soon</returns>
    Task<IEnumerable<HockeyReferee>> GetWithExpiringLicenseAsync(int withinDays);

    /// <summary>
    /// Gets Hockey referees ordered by number of matches officiated
    /// </summary>
    /// <param name="count">Maximum number of referees to return</param>
    /// <returns>The most experienced referees</returns>
    Task<IEnumerable<HockeyReferee>> GetMostExperiencedAsync(int count = 10);

    /// <summary>
    /// Adds a new Hockey referee
    /// </summary>
    /// <param name="referee">The referee to add</param>
    Task AddAsync(HockeyReferee referee);

    /// <summary>
    /// Updates an existing Hockey referee
    /// </summary>
    /// <param name="referee">The referee to update</param>
    Task UpdateAsync(HockeyReferee referee);

    /// <summary>
    /// Deletes a Hockey referee
    /// </summary>
    /// <param name="id">The ID of the referee to delete</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Searches for Hockey referees by name
    /// </summary>
    /// <param name="searchTerm">The search term</param>
    /// <returns>A collection of Hockey referees matching the search term</returns>
    Task<IEnumerable<HockeyReferee>> SearchByNameAsync(string searchTerm);

    /// <summary>
    /// Checks if a Hockey referee exists
    /// </summary>
    /// <param name="id">The referee ID</param>
    /// <returns>True if the referee exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);
}
