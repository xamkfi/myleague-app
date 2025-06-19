using Domain.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Common;

namespace Domain.Repositories.Floorball;

/// <summary>
/// Repository for managing floorball teams
/// </summary>
public interface IFloorballTeamRepository
{
    /// <summary>
    /// Gets a floorball team by ID
    /// </summary>
    /// <param name="id">The team ID</param>
    /// <returns>The team if found, null otherwise</returns>
    Task<FloorballTeam?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets a floorball team by name
    /// </summary>
    /// <param name="name">The team name</param>
    /// <returns>The team if found, null otherwise</returns>
    Task<FloorballTeam?> GetByNameAsync(string name);
    
    /// <summary>
    /// Gets all floorball teams
    /// </summary>
    /// <returns>A collection of all floorball teams</returns>
    Task<IEnumerable<FloorballTeam>> GetAllAsync();
    
    /// <summary>
    /// Gets paginated floorball teams with filtering support
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="clubId">Optional club ID filter</param>
    /// <param name="division">Optional division filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of floorball teams</returns>
    Task<PagedResult<FloorballTeam>> GetPagedAsync(
        int page, 
        int pageSize, 
        Guid? clubId = null,
        Guid? divisionId = null,
        CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Gets the total count of floorball teams with filtering
    /// </summary>
    /// <param name="clubId">Optional club ID filter</param>
    /// <param name="division">Optional division filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total count of matching floorball teams</returns>
    Task<int> GetCountAsync(
        Guid? clubId = null,
        Guid? divisionId = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets floorball teams by club ID
    /// </summary>
    /// <param name="clubId">The club ID</param>
    /// <returns>A collection of floorball teams belonging to the club</returns>
    Task<IEnumerable<FloorballTeam?>> GetByClubIdAsync(Guid clubId);
    
    /// <summary>
    /// Gets floorball teams where a specific player is in the roster
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <returns>A collection of floorball teams containing the player</returns>
    Task<IEnumerable<FloorballTeam>> GetByPlayerIdAsync(Guid playerId);
    
    /// <summary>
    /// Gets floorball teams by division
    /// </summary>
    /// <param name="division">The division to filter by</param>
    /// <returns>A collection of floorball teams in the specified division</returns>
    Task<IEnumerable<FloorballTeam>> GetByDivisionAsync(Guid divisionId);
    
    /// <summary>
    /// Gets floorball teams participating in a season
    /// </summary>
    /// <param name="seasonId">The season ID</param>
    /// <returns>A collection of floorball teams in the season</returns>
    Task<IEnumerable<FloorballTeam>> GetBySeasonIdAsync(Guid seasonId);
    
    /// <summary>
    /// Gets the team standings for a season
    /// </summary>
    /// <param name="seasonId">The season ID</param>
    /// <returns>Teams ordered by their standing in the season</returns>
    Task<IEnumerable<FloorballTeam>> GetStandingsAsync(Guid seasonId);
    
    /// <summary>
    /// Adds a new floorball team
    /// </summary>
    /// <param name="team">The team to add</param>
    Task AddAsync(FloorballTeam team);
    
    /// <summary>
    /// Updates an existing floorball team
    /// </summary>
    /// <param name="team">The team to update</param>
    Task UpdateAsync(FloorballTeam team);
    
    /// <summary>
    /// Deletes a floorball team
    /// </summary>
    /// <param name="id">The ID of the team to delete</param>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Searches for floorball teams by name
    /// </summary>
    /// <param name="searchTerm">The search term</param>
    /// <returns>A collection of floorball teams matching the search term</returns>
    Task<IEnumerable<FloorballTeam>> SearchByNameAsync(string searchTerm);
    
    /// <summary>
    /// Checks if a floorball team exists
    /// </summary>
    /// <param name="id">The team ID</param>
    /// <returns>True if the team exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Gets floorball teams by player ID
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <returns>A collection of floorball teams the player is in</returns>
    Task<IEnumerable<FloorballTeam>> GetTeamsByPlayerIdAsync(Guid playerId);
} 
