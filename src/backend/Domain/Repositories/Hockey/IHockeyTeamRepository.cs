using Domain.Entities.Hockey;
using Domain.Enums.Hockey;

namespace Domain.Repositories.Hockey;

/// <summary>
/// Repository for managing Hockey teams
/// </summary>
public interface IHockeyTeamRepository
{
    /// <summary>
    /// Gets a Hockey team by ID
    /// </summary>
    /// <param name="id">The team ID</param>
    /// <returns>The team if found, null otherwise</returns>
    Task<HockeyTeam> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a Hockey team by name
    /// </summary>
    /// <param name="name">The team name</param>
    /// <returns>The team if found, null otherwise</returns>
    Task<HockeyTeam> GetByNameAsync(string name);

    /// <summary>
    /// Gets all Hockey teams
    /// </summary>
    /// <returns>A collection of all Hockey teams</returns>
    Task<IEnumerable<HockeyTeam>> GetAllAsync();

    /// <summary>
    /// Gets Hockey teams by club ID
    /// </summary>
    /// <param name="clubId">The club ID</param>
    /// <returns>A collection of Hockey teams belonging to the club</returns>
    Task<IEnumerable<HockeyTeam>> GetByClubIdAsync(Guid clubId);

    /// <summary>
    /// Gets Hockey teams by division
    /// </summary>
    /// <param name="division">The division to filter by</param>
    /// <returns>A collection of Hockey teams in the specified division</returns>
    Task<IEnumerable<HockeyTeam>> GetByDivisionAsync(HockeyDivision division);

    /// <summary>
    /// Gets Hockey teams participating in a season
    /// </summary>
    /// <param name="seasonId">The season ID</param>
    /// <returns>A collection of Hockey teams in the season</returns>
    Task<IEnumerable<HockeyTeam>> GetBySeasonIdAsync(Guid seasonId);

    /// <summary>
    /// Gets the team standings for a season
    /// </summary>
    /// <param name="seasonId">The season ID</param>
    /// <returns>Teams ordered by their standing in the season</returns>
    Task<IEnumerable<HockeyTeam>> GetStandingsAsync(Guid seasonId);

    /// <summary>
    /// Adds a new Hockey team
    /// </summary>
    /// <param name="team">The team to add</param>
    Task AddAsync(HockeyTeam team);

    /// <summary>
    /// Updates an existing Hockey team
    /// </summary>
    /// <param name="team">The team to update</param>
    Task UpdateAsync(HockeyTeam team);

    /// <summary>
    /// Deletes a Hockey team
    /// </summary>
    /// <param name="id">The ID of the team to delete</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Searches for Hockey teams by name
    /// </summary>
    /// <param name="searchTerm">The search term</param>
    /// <returns>A collection of Hockey teams matching the search term</returns>
    Task<IEnumerable<HockeyTeam>> SearchByNameAsync(string searchTerm);

    /// <summary>
    /// Checks if a Hockey team exists
    /// </summary>
    /// <param name="id">The team ID</param>
    /// <returns>True if the team exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);
}
