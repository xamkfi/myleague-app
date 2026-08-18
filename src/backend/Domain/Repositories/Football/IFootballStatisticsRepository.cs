using Domain.Entities.Football.Statistics;

namespace Domain.Repositories.Football;

/// <summary>
/// Repository for football statistics entities.
/// </summary>
public interface IFootballStatisticsRepository
{
    Task<FootballTeamSeasonStatistics?> GetTeamSeasonStatisticsAsync(Guid teamId, Guid competitionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FootballTeamSeasonStatistics>> GetTeamStatisticsByCompetitionAsync(Guid competitionId, CancellationToken cancellationToken = default);
    Task<List<FootballTeamSeasonStatistics>> GetTeamStandingsAsync(Guid competitionId, CancellationToken cancellationToken = default);
    Task<List<FootballTeamSeasonStatistics>> GetTeamSeasonStatisticsForTeamAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task SaveTeamSeasonStatisticsAsync(FootballTeamSeasonStatistics statistics, CancellationToken cancellationToken = default);

    Task<FootballPlayerSeasonStatistics?> GetPlayerSeasonStatisticsAsync(Guid playerId, Guid teamId, Guid competitionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FootballPlayerSeasonStatistics>> GetPlayerStatisticsByCompetitionAsync(Guid competitionId, CancellationToken cancellationToken = default);
    Task<List<FootballPlayerSeasonStatistics>> GetPlayerStatisticsByTeamAndCompetitionAsync(Guid teamId, Guid competitionId, CancellationToken cancellationToken = default);
    Task<List<FootballPlayerSeasonStatistics>> GetPlayerStatisticsForTeamAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task<List<FootballPlayerSeasonStatistics>> GetTopScorersAsync(Guid competitionId, int topN, CancellationToken cancellationToken = default);
    Task<List<FootballPlayerSeasonStatistics>> GetTopAssistsAsync(Guid competitionId, int topN, CancellationToken cancellationToken = default);
    Task<List<FootballPlayerSeasonStatistics>> GetPlayerCareerStatisticsAsync(Guid playerId, CancellationToken cancellationToken = default);
    Task SavePlayerSeasonStatisticsAsync(FootballPlayerSeasonStatistics statistics, CancellationToken cancellationToken = default);

    Task<FootballMatchTeamStatistics?> GetMatchTeamStatisticsAsync(Guid matchId, Guid teamId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FootballMatchTeamStatistics>> GetMatchStatisticsAsync(Guid matchId, CancellationToken cancellationToken = default);
    Task SaveMatchTeamStatisticsAsync(FootballMatchTeamStatistics statistics, CancellationToken cancellationToken = default);

    Task<FootballStatisticsCache?> GetCachedStatisticsAsync(string cacheKey, CancellationToken cancellationToken = default);
    Task SaveCachedStatisticsAsync(FootballStatisticsCache cache, CancellationToken cancellationToken = default);
    Task<int> RemoveExpiredCacheAsync(CancellationToken cancellationToken = default);
    Task RemoveCompetitionCacheAsync(Guid competitionId, CancellationToken cancellationToken = default);

    Task ResetCompetitionStatisticsAsync(Guid competitionId, CancellationToken cancellationToken = default);
}
