using Domain.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Common;

namespace Domain.Repositories.Floorball;

/// <summary>
/// Repository for managing floorball competitions
/// </summary>
public interface IFloorballCompetitionRepository
{
    /// <summary>
    /// Gets a floorball competition by ID
    /// </summary>
    /// <param name="id">The competition ID</param>
    /// <returns>The competition if found, null otherwise</returns>
    Task<FloorballCompetition?> GetByIdAsync(Guid? id);
    
    /// <summary>
    /// Gets a floorball competition by name
    /// </summary>
    /// <param name="name">The competition name</param>
    /// <returns>The competition if found, null otherwise</returns>
    Task<FloorballCompetition?> GetByNameAsync(string name);
    
    /// <summary>
    /// Gets all floorball competitions
    /// </summary>
    /// <returns>A collection of all floorball competitions</returns>
    Task<IEnumerable<FloorballCompetition>> GetAllAsync();
    
    /// <summary>
    /// Gets active floorball competitions
    /// </summary>
    /// <returns>A collection of active floorball competitions</returns>
    Task<IEnumerable<FloorballCompetition>> GetActiveAsync();
    
    /// <summary>
    /// Gets completed floorball competitions
    /// </summary>
    /// <returns>A collection of completed floorball competitions</returns>
    Task<IEnumerable<FloorballCompetition>> GetCompletedAsync();
    
    /// <summary>
    /// Gets floorball competitions by division
    /// </summary>
    /// <param name="divisionId">The division to filter by</param>
    /// <returns>A collection of floorball competitions for the specified division</returns>
    Task<IEnumerable<FloorballCompetition>> GetByDivisionAsync(Guid divisionId);
    
    /// <summary>
    /// Gets competitions containing a specific team
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <returns>A collection of competitions with the team participating</returns>
    Task<IEnumerable<FloorballCompetition>> GetByTeamIdAsync(Guid teamId);
    
    /// <summary>
    /// Gets the current or upcoming competition for a division
    /// </summary>
    /// <param name="divisionId">The division</param>
    /// <returns>The current or next competition for the division</returns>
    Task<FloorballCompetition> GetCurrentOrUpcomingAsync(Guid divisionId);
    
    /// <summary>
    /// Adds a new floorball competition
    /// </summary>
    /// <param name="competition">The competition to add</param>
    Task AddAsync(FloorballCompetition competition);
    
    /// <summary>
    /// Updates an existing floorball competition
    /// </summary>
    /// <param name="competition">The competition to update</param>
    Task UpdateAsync(FloorballCompetition competition);
    
    /// <summary>
    /// Deletes a floorball competition
    /// </summary>
    /// <param name="id">The ID of the competition to delete</param>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Checks if a floorball competition exists
    /// </summary>
    /// <param name="id">The competition ID</param>
    /// <returns>True if the competition exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Gets lightweight date summaries for league seasons (excludes tournaments).
    /// </summary>
    Task<IReadOnlyList<FloorballSeasonDateSummary>> GetSeasonDateSummariesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paged list of league seasons, optionally filtered by season-year bounds.
    /// </summary>
    Task<PagedResult<FloorballSeason>> GetSeasonsPagedAsync(
        int page,
        int pageSize,
        int? startYear,
        int? endYear,
        Domain.Enums.Common.TeamCategory? teamCategory = null,
        CancellationToken cancellationToken = default);

    Task<FloorballSeason?> GetSeasonWithContentBlocksAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<FloorballSeason?> GetFeaturedSeasonWithContentBlocksAsync(
        int? startYear,
        int? endYear,
        CancellationToken cancellationToken = default);

    void MarkNewContentBlocksAdded(FloorballSeason season, IReadOnlyCollection<Guid> existingBlockIds);
} 
