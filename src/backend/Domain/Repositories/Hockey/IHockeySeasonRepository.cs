using Domain.Entities.Hockey;
using Domain.Enums.Hockey;

namespace Domain.Repositories.Hockey;

/// <summary>
/// Repository for managing Hockey seasons
/// </summary>
public interface IHockeySeasonRepository
{
    /// <summary>
    /// Gets a Hockey season by ID
    /// </summary>
    /// <param name="id">The season ID</param>
    /// <returns>The season if found, null otherwise</returns>
    Task<HockeySeason> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a Hockey season by name
    /// </summary>
    /// <param name="name">The season name</param>
    /// <returns>The season if found, null otherwise</returns>
    Task<HockeySeason?> GetByNameAsync(string name);

    /// <summary>
    /// Gets all Hockey seasons
    /// </summary>
    /// <returns>A collection of all Hockey seasons</returns>
    Task<IEnumerable<HockeySeason>> GetAllAsync();

    /// <summary>
    /// Gets active Hockey seasons
    /// </summary>
    /// <returns>A collection of active Hockey seasons</returns>
    Task<IEnumerable<HockeySeason>> GetActiveAsync();

    /// <summary>
    /// Gets completed Hockey seasons
    /// </summary>
    /// <returns>A collection of completed Hockey seasons</returns>
    Task<IEnumerable<HockeySeason>> GetCompletedAsync();

    /// <summary>
    /// Gets Hockey seasons by division
    /// </summary>
    /// <param name="division">The division to filter by</param>
    /// <returns>A collection of Hockey seasons for the specified division</returns>
    Task<IEnumerable<HockeySeason>> GetByDivisionAsync(HockeyDivision division);

    /// <summary>
    /// Gets seasons containing a specific team
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <returns>A collection of seasons with the team participating</returns>
    Task<IEnumerable<HockeySeason>> GetByTeamIdAsync(Guid teamId);

    /// <summary>
    /// Gets the current or upcoming season for a division
    /// </summary>
    /// <param name="division">The division</param>
    /// <returns>The current or next season for the division</returns>
    Task<HockeySeason> GetCurrentOrUpcomingAsync(HockeyDivision division);

    /// <summary>
    /// Adds a new Hockey season
    /// </summary>
    /// <param name="season">The season to add</param>
    Task AddAsync(HockeySeason season);

    /// <summary>
    /// Updates an existing Hockey season
    /// </summary>
    /// <param name="season">The season to update</param>
    Task UpdateAsync(HockeySeason season);

    /// <summary>
    /// Deletes a Hockey season
    /// </summary>
    /// <param name="id">The ID of the season to delete</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Checks if a Hockey season exists
    /// </summary>
    /// <param name="id">The season ID</param>
    /// <returns>True if the season exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);
}
