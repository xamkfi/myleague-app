using Domain.Entities;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents cached statistics data for performance optimization
/// </summary>
public class FloorballStatisticsCache : BaseEntity
{
    /// <summary>
    /// Gets the cache key that uniquely identifies this cached data
    /// </summary>
    public string CacheKey { get; private set; }

    /// <summary>
    /// Gets the ID of the competition this cache entry is associated with (optional)
    /// </summary>
    public Guid? CompetitionId { get; private set; }

    /// <summary>
    /// Gets the competition this cache entry is associated with (optional)
    /// </summary>
    public FloorballCompetition? Competition { get; private set; }

    /// <summary>
    /// Gets the serialized JSON data containing the cached statistics
    /// </summary>
    public string JsonData { get; private set; }

    /// <summary>
    /// Gets the timestamp when this cache entry was last updated
    /// </summary>
    public DateTime LastUpdated { get; private set; }

    /// <summary>
    /// Gets the timestamp when this cache entry expires
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Gets whether this cache entry has expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballStatisticsCache()
    {
        CacheKey = string.Empty;
        JsonData = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of FloorballStatisticsCache
    /// </summary>
    /// <param name="cacheKey">The unique cache key</param>
    /// <param name="jsonData">The serialized data to cache</param>
    /// <param name="expirationMinutes">Minutes until expiration (default: 60)</param>
    /// <param name="competitionId">Optional competition ID this cache is associated with</param>
    public FloorballStatisticsCache(string cacheKey, string jsonData, int expirationMinutes = 60, Guid? competitionId = null)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
            throw new ArgumentException("Cache key cannot be null or empty.", nameof(cacheKey));

        if (string.IsNullOrWhiteSpace(jsonData))
            throw new ArgumentException("JSON data cannot be null or empty.", nameof(jsonData));

        if (expirationMinutes <= 0)
            throw new ArgumentException("Expiration minutes must be positive.", nameof(expirationMinutes));

        CacheKey = cacheKey;
        JsonData = jsonData;
        CompetitionId = competitionId;
        LastUpdated = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
    }

    /// <summary>
    /// Updates the cached data and refreshes the expiration time
    /// </summary>
    /// <param name="jsonData">The new serialized data</param>
    /// <param name="expirationMinutes">Minutes until expiration (default: 60)</param>
    public void UpdateData(string jsonData, int expirationMinutes = 60)
    {
        if (string.IsNullOrWhiteSpace(jsonData))
            throw new ArgumentException("JSON data cannot be null or empty.", nameof(jsonData));

        if (expirationMinutes <= 0)
            throw new ArgumentException("Expiration minutes must be positive.", nameof(expirationMinutes));

        JsonData = jsonData;
        LastUpdated = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
    }

    /// <summary>
    /// Extends the expiration time without updating the data
    /// </summary>
    /// <param name="additionalMinutes">Additional minutes to extend expiration</param>
    public void ExtendExpiration(int additionalMinutes)
    {
        if (additionalMinutes <= 0)
            throw new ArgumentException("Additional minutes must be positive.", nameof(additionalMinutes));

        ExpiresAt = ExpiresAt.AddMinutes(additionalMinutes);
    }

    /// <summary>
    /// Marks the cache entry as expired immediately
    /// </summary>
    public void Expire()
    {
        ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
    }

    /// <summary>
    /// Creates a cache key for top scorers
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="topN">Number of top scorers</param>
    /// <returns>The cache key</returns>
    public static string CreateTopScorersKey(Guid competitionId, int topN = 10)
    {
        return $"TopScorers_{competitionId}_{topN}";
    }

    /// <summary>
    /// Creates a cache key for top assists
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="topN">Number of top assists</param>
    /// <returns>The cache key</returns>
    public static string CreateTopAssistsKey(Guid competitionId, int topN = 10)
    {
        return $"TopAssists_{competitionId}_{topN}";
    }

    /// <summary>
    /// Creates a cache key for top goalies
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="topN">Number of top goalies</param>
    /// <returns>The cache key</returns>
    public static string CreateTopGoaliesKey(Guid competitionId, int topN = 10)
    {
        return $"TopGoalies_{competitionId}_{topN}";
    }

    /// <summary>
    /// Creates a cache key for team standings
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <returns>The cache key</returns>
    public static string CreateStandingsKey(Guid competitionId)
    {
        return $"Standings_{competitionId}";
    }

    /// <summary>
    /// Creates a cache key for competition statistics
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <returns>The cache key</returns>
    public static string CreateSeasonStatsKey(Guid competitionId)
    {
        return $"SeasonStats_{competitionId}";
    }

    /// <summary>
    /// Creates a cache key for team statistics
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="teamId">The team ID</param>
    /// <returns>The cache key</returns>
    public static string CreateTeamStatsKey(Guid competitionId, Guid teamId)
    {
        return $"TeamStats_{competitionId}_{teamId}";
    }

    /// <summary>
    /// Creates a cache key for player statistics
    /// </summary>
    /// <param name="competitionId">The competition ID</param>
    /// <param name="playerId">The player ID</param>
    /// <returns>The cache key</returns>
    public static string CreatePlayerStatsKey(Guid competitionId, Guid playerId)
    {
        return $"PlayerStats_{competitionId}_{playerId}";
    }

    /// <summary>
    /// Creates a cache key for match statistics
    /// </summary>
    /// <param name="matchId">The match ID</param>
    /// <returns>The cache key</returns>
    public static string CreateMatchStatsKey(Guid matchId)
    {
        return $"MatchStats_{matchId}";
    }
}
