using Domain.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;

namespace Domain.Repositories.Floorball;

/// <summary>
/// Repository for managing floorball matches
/// </summary>
public interface IFloorballMatchRepository
{
    /// <summary>
    /// Gets a floorball match by ID
    /// </summary>
    /// <param name="id">The match ID</param>
    /// <returns>The match if found, null otherwise</returns>
    Task<FloorballMatch?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets all floorball matches
    /// </summary>
    /// <returns>A collection of all floorball matches</returns>
    Task<IEnumerable<FloorballMatch>> GetAllAsync();
    
    /// <summary>
    /// Gets paginated floorball matches with filtering support
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="competitionId">Optional competition ID filter</param>
    /// <param name="teamId">Optional team ID filter (home or away)</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="status">Optional match status filter</param>
    /// <param name="sortOrder">Optional sort order ("asc" or "desc")</param>
    /// <param name="searchQuery">Optional search query to filter by team names (case-insensitive, partial match)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of floorball matches</returns>
    Task<PagedResult<FloorballMatch>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? competitionId = null,
        Guid? teamId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        FloorballMatchStatus? status = null,
        string sortOrder = "desc",
        string? searchQuery = null,
        Guid? tournamentGroupId = null,
        FloorballCompetitionType? competitionType = null,
        CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Gets the total count of floorball matches with filtering
    /// </summary>
    /// <param name="competitionId">Optional competition ID filter</param>
    /// <param name="teamId">Optional team ID filter (home or away)</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="status">Optional match status filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total count of matching floorball matches</returns>
    Task<int> GetCountAsync(
        Guid? competitionId = null,
        Guid? teamId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        FloorballMatchStatus? status = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets matches for a specified competition
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <returns>A collection of matches in the competition</returns>
    Task<IEnumerable<FloorballMatch>> GetByCompetitionIdAsync(Guid competitionId);

    /// <summary>
    /// Gets matches assigned to a specific tournament group, optionally filtered by status.
    /// </summary>
    /// <param name="tournamentGroupId">The tournament group ID</param>
    /// <param name="status">Optional match status filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A collection of matches in the tournament group</returns>
    Task<IEnumerable<FloorballMatch>> GetByTournamentGroupAsync(
        Guid tournamentGroupId,
        FloorballMatchStatus? status = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets matches for a specified team
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <returns>A collection of matches involving the team</returns>
    Task<IEnumerable<FloorballMatch>> GetByTeamIdAsync(Guid teamId);
    
    /// <summary>
    /// Gets upcoming matches for a specified team
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <param name="count">Maximum number of matches to return</param>
    /// <returns>A collection of upcoming matches for the team</returns>
    Task<IEnumerable<FloorballMatch>> GetUpcomingByTeamIdAsync(Guid teamId, int count = 5);
    
    /// <summary>
    /// Gets past matches for a specified team
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <param name="count">Maximum number of matches to return</param>
    /// <returns>A collection of past matches for the team</returns>
    Task<IEnumerable<FloorballMatch>> GetPastByTeamIdAsync(Guid teamId, int count = 5);
    
    /// <summary>
    /// Gets matches by status
    /// </summary>
    /// <param name="status">The match status</param>
    /// <returns>A collection of matches with the specified status</returns>
    Task<IEnumerable<FloorballMatch>> GetByStatusAsync(FloorballMatchStatus status);
    
    /// <summary>
    /// Gets matches requiring officials
    /// </summary>
    /// <returns>A collection of matches needing officials</returns>
    Task<IEnumerable<FloorballMatch>> GetMatchesNeedingOfficialsAsync();
    
    /// <summary>
    /// Gets matches scheduled for a date range
    /// </summary>
    /// <param name="startDate">The start date</param>
    /// <param name="endDate">The end date</param>
    /// <returns>A collection of matches scheduled in the date range</returns>
    Task<IEnumerable<FloorballMatch>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets matches at a specific venue
    /// </summary>
    /// <param name="venue">The venue name</param>
    /// <returns>A collection of matches at the venue</returns>
    Task<IEnumerable<FloorballMatch>> GetByVenueAsync(string venue);

    /// <summary>
    /// Gets today's matches for a specified team
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A collection of today's matches involving the team</returns>
    Task<IEnumerable<FloorballMatch>> GetTodaysMatchesByTeamAsync(Guid teamId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Adds a new floorball match
    /// </summary>
    /// <param name="match">The match to add</param>
    Task AddAsync(FloorballMatch match);
    
    /// <summary>
    /// Updates an existing floorball match
    /// </summary>
    /// <param name="match">The match to update</param>
    Task UpdateAsync(FloorballMatch match);
    
    /// <summary>
    /// Deletes a floorball match
    /// </summary>
    /// <param name="id">The ID of the match to delete</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Bulk-deletes every match (and its directly dependent rows) that belongs to the given
    /// competition. Used by the tournament-delete handler when a Draft tournament is removed so
    /// that the <c>FloorballMatch.TournamentGroupId</c> RESTRICT FK doesn't block the cascade
    /// from <c>FloorballCompetition → FloorballTournamentGroup</c>.
    ///
    /// Implementation must:
    ///   • Clean up <c>FloorballMatchTeamStatistics</c> rows manually (no DB-level FK because
    ///     the Match navigation is <c>Ignored</c> in the EF configuration).
    ///   • Break any <c>NextMatchId</c> self-references first so the bracket can collapse from
    ///     either end without tripping the self-reference RESTRICT FK.
    ///   • Issue a single bulk <c>DELETE</c> for the match rows themselves, letting the DB cascade
    ///     events / period scores / officials via the configured Cascade FKs.
    ///
    /// Runs outside the change tracker (uses <c>ExecuteDeleteAsync</c>) and persists immediately —
    /// callers do NOT need to follow up with <c>SaveChangesAsync</c> for this specific operation,
    /// but should still call it for any other tracked changes.
    /// </summary>
    /// <param name="competitionId">Competition (tournament / season) id whose matches should be wiped.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of <c>FloorballMatch</c> rows deleted.</returns>
    Task<int> DeleteAllByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a floorball match exists
    /// </summary>
    /// <param name="id">The match ID</param>
    /// <returns>True if the match exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Marks a match event as added
    /// </summary>
    /// <param name="matchEvent"></param>
    void MarkEventAsAdded(FloorballMatchEvent matchEvent);

    /// <summary>
    /// Gets last five game form
    /// </summary>
    /// <param name="teamId"></param>
    /// <param name="competitionId"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    Task<IEnumerable<FloorballMatch>> GetLastCompletedByTeamAsync(Guid teamId, Guid? competitionId = null, int count = 5);
} 
