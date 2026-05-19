using Domain.Common;
using Domain.Entities.Floorball;

namespace Domain.Repositories.Floorball;

/// <summary>
/// Repository interface for managing floorball statistics entities
/// </summary>
public interface IFloorballStatisticsRepository
{
    #region Team Season Statistics

    /// <summary>
    /// Gets team season statistics by team and competition ID
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Team season statistics or null if not found</returns>
    Task<FloorballTeamSeasonStatistics?> GetTeamSeasonStatisticsAsync(Guid teamId, Guid competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all team statistics for a specific competition
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of team season statistics</returns>
    Task<IEnumerable<FloorballTeamSeasonStatistics>> GetTeamStatisticsByCompetitionAsync(Guid competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets team standings (ordered by points descending)
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Team standings ordered by points</returns>
    Task<List<FloorballTeamSeasonStatistics>> GetTeamStandingsAsync(Guid competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds or updates team season statistics
    /// </summary>
    /// <param name="statistics">The statistics to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveTeamSeasonStatisticsAsync(FloorballTeamSeasonStatistics statistics, CancellationToken cancellationToken = default);

    #endregion

    #region Player Season Statistics

    /// <summary>
    /// Gets player season statistics by player, team, and competition ID
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <param name="teamId">The team ID</param>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Player season statistics or null if not found</returns>
    Task<FloorballPlayerSeasonStatistics?> GetPlayerSeasonStatisticsAsync(Guid playerId, Guid teamId, Guid competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all player statistics for a specific competition
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of player season statistics</returns>
    Task<IEnumerable<FloorballPlayerSeasonStatistics>> GetPlayerStatisticsByCompetitionAsync(Guid competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all player statistics for a specific team in a specific competition
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of player season statistics for the team</returns>
    Task<List<FloorballPlayerSeasonStatistics>> GetPlayerStatisticsByTeamAndCompetitionAsync(Guid teamId, Guid competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets top scorers for a specific competition
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="topN">Number of top scorers to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Top scoring players</returns>
    Task<List<FloorballPlayerSeasonStatistics>> GetTopScorersAsync(Guid competitionId, int topN, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets top assists for a specific competition
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="topN">Number of top assists to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Top assist leaders</returns>
    Task<List<FloorballPlayerSeasonStatistics>> GetTopAssistsAsync(Guid competitionId, int topN, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets player statistics across all seasons for career totals
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Player statistics across all seasons</returns>
    Task<List<FloorballPlayerSeasonStatistics>> GetPlayerCareerStatisticsAsync(Guid playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds or updates player season statistics
    /// </summary>
    /// <param name="statistics">The statistics to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task SavePlayerSeasonStatisticsAsync(FloorballPlayerSeasonStatistics statistics, CancellationToken cancellationToken = default);

    #endregion

    #region Goalie Season Statistics

    /// <summary>
    /// Gets goalie season statistics by player, team, and competition ID
    /// </summary>
    /// <param name="playerId">The goalie player ID</param>
    /// <param name="teamId">The team ID</param>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Goalie season statistics or null if not found</returns>
    Task<FloorballGoalieSeasonStatistics?> GetGoalieSeasonStatisticsAsync(Guid playerId, Guid teamId, Guid competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all goalie statistics for a specific competition
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of goalie season statistics</returns>
    Task<IEnumerable<FloorballGoalieSeasonStatistics>> GetGoalieStatisticsByCompetitionAsync(Guid competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets top goalies for a specific competition based on save percentage
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="topN">Number of top goalies to return</param>
    /// <param name="minimumGames">Minimum games played to qualify</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Top performing goalies</returns>
    Task<List<FloorballGoalieSeasonStatistics>> GetTopGoaliesAsync(Guid competitionId, int topN, int minimumGames, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets goalie statistics across all seasons for career totals
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Goalie statistics across all seasons</returns>
    Task<List<FloorballGoalieSeasonStatistics>> GetGoalieCareerStatisticsAsync(Guid playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds or updates goalie season statistics
    /// </summary>
    /// <param name="statistics">The statistics to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveGoalieSeasonStatisticsAsync(FloorballGoalieSeasonStatistics statistics, CancellationToken cancellationToken = default);

    #endregion

    #region Match Team Statistics

    /// <summary>
    /// Gets match team statistics by match and team ID
    /// </summary>
    /// <param name="matchId">The match ID</param>
    /// <param name="teamId">The team ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Match team statistics or null if not found</returns>
    Task<FloorballMatchTeamStatistics?> GetMatchTeamStatisticsAsync(Guid matchId, Guid teamId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all team statistics for a specific match
    /// </summary>
    /// <param name="matchId">The match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of match team statistics</returns>
    Task<IEnumerable<FloorballMatchTeamStatistics>> GetMatchStatisticsAsync(Guid matchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds or updates match team statistics
    /// </summary>
    /// <param name="statistics">The statistics to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveMatchTeamStatisticsAsync(FloorballMatchTeamStatistics statistics, CancellationToken cancellationToken = default);

    #endregion

    #region Statistics Cache

    /// <summary>
    /// Gets cached statistics by cache key
    /// </summary>
    /// <param name="cacheKey">The cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached statistics or null if not found/expired</returns>
    Task<FloorballStatisticsCache?> GetCachedStatisticsAsync(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves cached statistics
    /// </summary>
    /// <param name="cache">The cache entry to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveCachedStatisticsAsync(FloorballStatisticsCache cache, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes expired cache entries
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entries removed</returns>
    Task<int> RemoveExpiredCacheAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cache entries for a specific competition
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task RemoveCompetitionCacheAsync(Guid competitionId, CancellationToken cancellationToken = default);

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Saves multiple team season statistics in a batch
    /// </summary>
    /// <param name="statistics">Collection of team statistics to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveTeamSeasonStatisticsBatchAsync(IEnumerable<FloorballTeamSeasonStatistics> statistics, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves multiple player season statistics in a batch
    /// </summary>
    /// <param name="statistics">Collection of player statistics to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task SavePlayerSeasonStatisticsBatchAsync(IEnumerable<FloorballPlayerSeasonStatistics> statistics, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves multiple goalie season statistics in a batch
    /// </summary>
    /// <param name="statistics">Collection of goalie statistics to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveGoalieSeasonStatisticsBatchAsync(IEnumerable<FloorballGoalieSeasonStatistics> statistics, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets all statistics for a specific competition (useful for recalculation)
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task ResetCompetitionStatisticsAsync(Guid competitionId, CancellationToken cancellationToken = default);

    #endregion
}
