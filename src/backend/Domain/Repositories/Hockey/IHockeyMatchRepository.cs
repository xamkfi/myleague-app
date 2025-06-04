using Domain.Entities.Hockey;
using Domain.Enums.Hockey;

namespace Domain.Repositories.Hockey;

/// <summary>
/// Repository for managing Hockey matches
/// </summary>
public interface IHockeyMatchRepository
{
    /// <summary>
    /// Gets a Hockey match by ID
    /// </summary>
    /// <param name="id">The match ID</param>
    /// <returns>The match if found, null otherwise</returns>
    Task<HockeyMatch> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all Hockey matches
    /// </summary>
    /// <returns>A collection of all Hockey matches</returns>
    Task<IEnumerable<HockeyMatch>> GetAllAsync();

    /// <summary>
    /// Gets matches for a specified season
    /// </summary>
    /// <param name="seasonId">The season ID</param>
    /// <returns>A collection of matches in the season</returns>
    Task<IEnumerable<HockeyMatch>> GetBySeasonIdAsync(Guid seasonId);

    /// <summary>
    /// Gets matches for a specified team
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <returns>A collection of matches involving the team</returns>
    Task<IEnumerable<HockeyMatch>> GetByTeamIdAsync(Guid teamId);

    /// <summary>
    /// Gets upcoming matches for a specified team
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <param name="count">Maximum number of matches to return</param>
    /// <returns>A collection of upcoming matches for the team</returns>
    Task<IEnumerable<HockeyMatch>> GetUpcomingByTeamIdAsync(Guid teamId, int count = 5);

    /// <summary>
    /// Gets past matches for a specified team
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <param name="count">Maximum number of matches to return</param>
    /// <returns>A collection of past matches for the team</returns>
    Task<IEnumerable<HockeyMatch>> GetPastByTeamIdAsync(Guid teamId, int count = 5);

    /// <summary>
    /// Gets matches by status
    /// </summary>
    /// <param name="status">The match status</param>
    /// <returns>A collection of matches with the specified status</returns>
    Task<IEnumerable<HockeyMatch>> GetByStatusAsync(HockeyMatchStatus status);

    /// <summary>
    /// Gets matches requiring officials
    /// </summary>
    /// <returns>A collection of matches needing officials</returns>
    Task<IEnumerable<HockeyMatch>> GetMatchesNeedingOfficialsAsync();

    /// <summary>
    /// Gets matches scheduled for a date range
    /// </summary>
    /// <param name="startDate">The start date</param>
    /// <param name="endDate">The end date</param>
    /// <returns>A collection of matches scheduled in the date range</returns>
    Task<IEnumerable<HockeyMatch>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets matches at a specific venue
    /// </summary>
    /// <param name="venue">The venue name</param>
    /// <returns>A collection of matches at the venue</returns>
    Task<IEnumerable<HockeyMatch>> GetByVenueAsync(string venue);

    /// <summary>
    /// Adds a new Hockey match
    /// </summary>
    /// <param name="match">The match to add</param>
    Task AddAsync(HockeyMatch match);

    /// <summary>
    /// Updates an existing Hockey match
    /// </summary>
    /// <param name="match">The match to update</param>
    Task UpdateAsync(HockeyMatch match);

    /// <summary>
    /// Deletes a Hockey match
    /// </summary>
    /// <param name="id">The ID of the match to delete</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Checks if a Hockey match exists
    /// </summary>
    /// <param name="id">The match ID</param>
    /// <returns>True if the match exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);
}
