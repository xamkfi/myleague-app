using Domain.Entities.Floorball;

namespace Domain.Services.Floorball;

/// <summary>
/// Domain service interface for floorball statistics operations
/// </summary>
public interface IFloorballStatisticService
{
    #region Core Statistics Methods (As Requested)

    /// <summary>
    /// Gets comprehensive team statistics for a specific competition
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="teamId">The team ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Team season statistics</returns>
    Task<FloorballTeamSeasonStatistics?> GetTeamStatistics(Guid competitionId, Guid teamId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets comprehensive player statistics for a specific competition
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="playerId">The player ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Player season statistics</returns>
    Task<FloorballPlayerSeasonStatistics?> GetPlayerStatistics(Guid competitionId, Guid playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets detailed match statistics for both teams
    /// </summary>
    /// <param name="matchId">The match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of match team statistics</returns>
    Task<IEnumerable<FloorballMatchTeamStatistics>> GetMatchStatistics(Guid matchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets top scorers for a specific competition
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="topN">Number of top scorers to return (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Top scoring players</returns>
    Task<List<FloorballPlayerSeasonStatistics>> GetTopScorers(Guid competitionId, int topN = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets comprehensive competition statistics summary
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Competition statistics including team standings and top performers</returns>
    Task<CompetitionStatisticsSummary> GetCompetitionStatistics(Guid competitionId, CancellationToken cancellationToken = default);

    #endregion

    #region Additional Useful Statistics Methods

    /// <summary>
    /// Gets team standings for a specific competition ordered by points
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Team standings ordered by points (descending)</returns>
    Task<List<FloorballTeamSeasonStatistics>> GetTeamStandings(Guid competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets top assists leaders for a specific competition
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="topN">Number of top assists to return (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Top assist leaders</returns>
    Task<List<FloorballPlayerSeasonStatistics>> GetTopAssists(Guid competitionId, int topN = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets top goalies for a specific competition based on save percentage
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="topN">Number of top goalies to return (default: 10)</param>
    /// <param name="minimumGames">Minimum games played to qualify (default: 5)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Top performing goalies</returns>
    Task<List<FloorballGoalieSeasonStatistics>> GetTopGoalies(Guid competitionId, int topN = 10, int minimumGames = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets career statistics for a specific player across all seasons
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Player career statistics summary</returns>
    Task<PlayerCareerStatistics> GetPlayerCareerStatistics(Guid playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all statistics for a player in a specific competition (includes both field player and goalie stats if applicable)
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="playerId">The player ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete player statistics for the competition</returns>
    Task<CompletePlayerSeasonStatistics> GetCompletePlayerStatistics(Guid competitionId, Guid playerId, CancellationToken cancellationToken = default);

    #endregion

    #region Statistics Update Methods

    /// <summary>
    /// Updates statistics for a specific match (calculates and persists team and player stats)
    /// </summary>
    /// <param name="matchId">The match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task UpdateMatchStatistics(Guid matchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recalculates and updates all statistics for a specific competition
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task UpdateCompetitionStatistics(Guid competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes cached statistics data
    /// </summary>
    /// <param name="competitionId">Optional competition ID to refresh specific competition cache</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task RefreshStatisticsCache(Guid? competitionId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates statistics for a specific player after a match event
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="teamId">The team ID</param>
    /// <param name="matchId">The match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task UpdatePlayerStatistics(Guid playerId, Guid competitionId, Guid teamId, Guid matchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates statistics for a specific team after a match
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="matchId">The match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task UpdateTeamStatistics(Guid teamId, Guid competitionId, Guid matchId, CancellationToken cancellationToken = default);

    #endregion

    #region Cache Management Methods

    /// <summary>
    /// Gets cached statistics data by key
    /// </summary>
    /// <param name="cacheKey">The cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached statistics data or null if not found/expired</returns>
    Task<FloorballStatisticsCache?> GetCachedStatistics(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets cached statistics data
    /// </summary>
    /// <param name="cacheKey">The cache key</param>
    /// <param name="jsonData">The data to cache (serialized JSON)</param>
    /// <param name="competitionId">Optional competition ID</param>
    /// <param name="expirationMinutes">Cache expiration in minutes (default: 60)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task SetCachedStatistics(string cacheKey, string jsonData, Guid? competitionId = null, int expirationMinutes = 60, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates cached statistics for a specific competition
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task InvalidateCompetitionCache(Guid competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up expired cache entries
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of expired entries removed</returns>
    Task<int> CleanupExpiredCache(CancellationToken cancellationToken = default);

    #endregion
}

#region Statistics Summary DTOs

/// <summary>
/// Comprehensive competition statistics summary
/// </summary>
public class CompetitionStatisticsSummary
{
    /// <summary>
    /// Gets or sets the competition ID
    /// </summary>
    public Guid CompetitionId { get; set; }

    /// <summary>
    /// Gets or sets team standings ordered by points
    /// </summary>
    public IReadOnlyCollection<FloorballTeamSeasonStatistics>? TeamStandings { get; }

    /// <summary>
    /// Gets or sets top scoring players
    /// </summary>
    public IReadOnlyCollection<FloorballPlayerSeasonStatistics>? TopScorers { get; }

    /// <summary>
    /// Gets or sets top assist leaders
    /// </summary>
    public IReadOnlyCollection<FloorballPlayerSeasonStatistics>? TopAssists { get; }

    /// <summary>
    /// Gets or sets top performing goalies
    /// </summary>
    public IReadOnlyCollection<FloorballGoalieSeasonStatistics>? TopGoalies { get; }

    /// <summary>
    /// Gets or sets total games played in the season
    /// </summary>
    public int TotalGames { get; set; }

    /// <summary>
    /// Gets or sets total goals scored in the season
    /// </summary>
    public int TotalGoals { get; set; }

    /// <summary>
    /// Gets or sets average goals per game
    /// </summary>
    public decimal AverageGoalsPerGame { get; set; }
}

/// <summary>
/// Player career statistics across all seasons
/// </summary>
public class PlayerCareerStatistics
{
    /// <summary>
    /// Gets or sets the player ID
    /// </summary>
    public Guid PlayerId { get; set; }

    /// <summary>
    /// Gets or sets career totals
    /// </summary>
    public FloorballPlayerSeasonStatistics CareerTotals { get; set; } = null!;

    /// <summary>
    /// Gets or sets season-by-season statistics
    /// </summary>
    public IReadOnlyCollection<FloorballPlayerSeasonStatistics>? SeasonStatistics { get; }

    /// <summary>
    /// Gets or sets career goalie statistics (if applicable)
    /// </summary>
    public IReadOnlyCollection<FloorballGoalieSeasonStatistics>? GoalieStatistics { get; }
}

/// <summary>
/// Complete player statistics for a season (includes both field and goalie stats)
/// </summary>
public class CompletePlayerSeasonStatistics
{
    /// <summary>
    /// Gets or sets the player ID
    /// </summary>
    public Guid PlayerId { get; set; }

    /// <summary>
    /// Gets or sets the competition ID
    /// </summary>
    public Guid CompetitionId { get; set; }

    /// <summary>
    /// Gets or sets field player statistics
    /// </summary>
    public FloorballPlayerSeasonStatistics? FieldPlayerStats { get; set; }

    /// <summary>
    /// Gets or sets goalie statistics (if the player played as goalie)
    /// </summary>
    public FloorballGoalieSeasonStatistics? GoalieStats { get; set; }

    /// <summary>
    /// Gets whether the player has both field and goalie statistics
    /// </summary>
    public bool IsHybridPlayer => FieldPlayerStats != null && GoalieStats != null;
}

#endregion
