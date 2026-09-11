using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Statistics;

namespace Domain.Entities.Hockey.Statistics;

/// <summary>
/// Fast read-model cache for hockey statistics. Not the source of truth.
/// </summary>
public class HockeyStatisticsCache : BaseEntity
{
    public string CacheKey { get; private set; } = string.Empty;

    public Guid? CompetitionId { get; private set; }
    public HockeyCompetition? Competition { get; private set; }

    public HockeyStatisticsScope? Scope { get; private set; }

    public Guid? CompetitionDivisionId { get; private set; }
    public HockeyCompetitionDivision? CompetitionDivision { get; private set; }

    public Guid? TournamentGroupId { get; private set; }
    public HockeyTournamentGroup? TournamentGroup { get; private set; }

    public Guid? PlayoffSeriesId { get; private set; }
    public HockeyPlayoffSeries? PlayoffSeries { get; private set; }

    public Guid? TeamId { get; private set; }
    public HockeyTeam? Team { get; private set; }

    public Guid? PlayerId { get; private set; }
    public HockeyPlayer? Player { get; private set; }

    public Guid? MatchId { get; private set; }
    public HockeyMatch? Match { get; private set; }

    public string JsonData { get; private set; } = string.Empty;
    public DateTime LastUpdated { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    private HockeyStatisticsCache() { }

    public HockeyStatisticsCache(
        string cacheKey,
        string jsonData,
        int expirationMinutes = 60,
        Guid? competitionId = null,
        HockeyStatisticsScope? scope = null,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null,
        Guid? teamId = null,
        Guid? playerId = null,
        Guid? matchId = null)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
            throw new ArgumentException("Cache key cannot be null or empty.", nameof(cacheKey));
        if (string.IsNullOrWhiteSpace(jsonData))
            throw new ArgumentException("JSON data cannot be null or empty.", nameof(jsonData));
        if (expirationMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(expirationMinutes), "Expiration minutes must be positive.");
        if (competitionId == Guid.Empty)
            throw new ArgumentException("Competition id cannot be empty.", nameof(competitionId));
        if (competitionDivisionId == Guid.Empty)
            throw new ArgumentException("Competition division id cannot be empty.", nameof(competitionDivisionId));
        if (tournamentGroupId == Guid.Empty)
            throw new ArgumentException("Tournament group id cannot be empty.", nameof(tournamentGroupId));
        if (playoffSeriesId == Guid.Empty)
            throw new ArgumentException("Playoff series id cannot be empty.", nameof(playoffSeriesId));
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team id cannot be empty.", nameof(teamId));
        if (playerId == Guid.Empty)
            throw new ArgumentException("Player id cannot be empty.", nameof(playerId));
        if (matchId == Guid.Empty)
            throw new ArgumentException("Match id cannot be empty.", nameof(matchId));

        CacheKey = cacheKey;
        JsonData = jsonData;
        CompetitionId = competitionId;
        Scope = scope;
        CompetitionDivisionId = competitionDivisionId;
        TournamentGroupId = tournamentGroupId;
        PlayoffSeriesId = playoffSeriesId;
        TeamId = teamId;
        PlayerId = playerId;
        MatchId = matchId;
        LastUpdated = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
    }

    public void UpdateData(string jsonData, int expirationMinutes = 60)
    {
        if (string.IsNullOrWhiteSpace(jsonData))
            throw new ArgumentException("JSON data cannot be null or empty.", nameof(jsonData));
        if (expirationMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(expirationMinutes), "Expiration minutes must be positive.");

        JsonData = jsonData;
        LastUpdated = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
    }

    public void ExtendExpiration(int additionalMinutes)
    {
        if (additionalMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(additionalMinutes), "Additional minutes must be positive.");
        ExpiresAt = ExpiresAt.AddMinutes(additionalMinutes);
    }

    public void Expire() => ExpiresAt = DateTime.UtcNow.AddMinutes(-1);

    public static string CreateCompetitionStatsKey(Guid competitionId, HockeyStatisticsScope scope) =>
        $"CompetitionStats_{competitionId}_{scope}";

    public static string CreateDivisionStatsKey(Guid competitionId, Guid competitionDivisionId) =>
        $"DivisionStats_{competitionId}_{competitionDivisionId}";

    public static string CreateTournamentGroupStatsKey(Guid competitionId, Guid tournamentGroupId) =>
        $"TournamentGroupStats_{competitionId}_{tournamentGroupId}";

    public static string CreatePlayoffSeriesStatsKey(Guid competitionId, Guid playoffSeriesId) =>
        $"PlayoffSeriesStats_{competitionId}_{playoffSeriesId}";

    public static string CreateStandingsKey(
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? scopeEntityId = null) =>
        scopeEntityId is Guid id
            ? $"Standings_{competitionId}_{scope}_{id}"
            : $"Standings_{competitionId}_{scope}";

    public static string CreateTeamStatsKey(
        Guid competitionId,
        Guid teamId,
        HockeyStatisticsScope scope,
        Guid? scopeEntityId = null) =>
        scopeEntityId is Guid id
            ? $"TeamStats_{competitionId}_{teamId}_{scope}_{id}"
            : $"TeamStats_{competitionId}_{teamId}_{scope}";

    public static string CreatePlayerStatsKey(
        Guid competitionId,
        Guid playerId,
        HockeyStatisticsScope scope,
        Guid? scopeEntityId = null) =>
        scopeEntityId is Guid id
            ? $"PlayerStats_{competitionId}_{playerId}_{scope}_{id}"
            : $"PlayerStats_{competitionId}_{playerId}_{scope}";

    public static string CreateMatchStatsKey(Guid matchId) =>
        $"MatchStats_{matchId}";
}
