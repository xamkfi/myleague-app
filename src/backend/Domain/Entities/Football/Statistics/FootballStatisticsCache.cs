using Domain.Entities.Football.Competitions;

namespace Domain.Entities.Football.Statistics;

/// <summary>
/// Cached statistics JSON for a football competition.
/// </summary>
public class FootballStatisticsCache : BaseEntity
{
    public string CacheKey { get; private set; }
    public Guid? CompetitionId { get; private set; }
    public FootballCompetition? Competition { get; private set; }
    public string JsonData { get; private set; }
    public DateTime LastUpdated { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    private FootballStatisticsCache()
    {
        CacheKey = string.Empty;
        JsonData = string.Empty;
    }

    public FootballStatisticsCache(string cacheKey, string jsonData, int expirationMinutes = 60, Guid? competitionId = null)
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

    public void Expire() => ExpiresAt = DateTime.UtcNow.AddMinutes(-1);

    public static string CreateTopScorersKey(Guid competitionId, int topN = 10) => $"FootballTopScorers_{competitionId}_{topN}";
    public static string CreateTopAssistsKey(Guid competitionId, int topN = 10) => $"FootballTopAssists_{competitionId}_{topN}";
    public static string CreateStandingsKey(Guid competitionId) => $"FootballStandings_{competitionId}";
    public static string CreateSeasonStatsKey(Guid competitionId) => $"FootballSeasonStats_{competitionId}";
    public static string CreateTeamStatsKey(Guid competitionId, Guid teamId) => $"FootballTeamStats_{competitionId}_{teamId}";
    public static string CreatePlayerStatsKey(Guid competitionId, Guid playerId) => $"FootballPlayerStats_{competitionId}_{playerId}";
    public static string CreateMatchStatsKey(Guid matchId) => $"FootballMatchStats_{matchId}";
}
