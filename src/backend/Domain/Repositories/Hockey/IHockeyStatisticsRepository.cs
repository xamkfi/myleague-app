using Domain.Common;
using Domain.Entities.Hockey.Statistics;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Statistics;

namespace Domain.Repositories.Hockey;

/// <summary>
/// Repository for hockey match and competition statistics.
/// </summary>
public interface IHockeyStatisticsRepository
{
    Task<IReadOnlyList<HockeyMatchTeamStatistics>> GetMatchTeamStatisticsAsync(Guid matchId);

    Task<IReadOnlyList<HockeyMatchPlayerStatistics>> GetMatchPlayerStatisticsAsync(Guid matchId);

    Task<IReadOnlyList<HockeyGoalieMatchStatistics>> GetGoalieMatchStatisticsAsync(Guid matchId);

    Task ReplaceMatchStatisticsAsync(
        Guid matchId,
        IReadOnlyList<HockeyMatchTeamStatistics> teams,
        IReadOnlyList<HockeyMatchPlayerStatistics> players,
        IReadOnlyList<HockeyGoalieMatchStatistics> goalies);

    Task<IReadOnlyList<HockeyTeamCompetitionStatistics>> GetTeamCompetitionStatisticsAsync(
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null);

    Task<HockeyTeamCompetitionStatistics?> GetTeamCompetitionStatisticsAsync(
        Guid teamId,
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null);

    Task<IReadOnlyList<HockeyPlayerCompetitionStatistics>> GetPlayerCompetitionStatisticsAsync(
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null);

    Task<HockeyPlayerCompetitionStatistics?> GetPlayerCompetitionStatisticsAsync(
        Guid playerId,
        Guid teamId,
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null);

    Task<IReadOnlyList<HockeyGoalieCompetitionStatistics>> GetGoalieCompetitionStatisticsAsync(
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null);

    Task<HockeyGoalieCompetitionStatistics?> GetGoalieCompetitionStatisticsAsync(
        Guid playerId,
        Guid teamId,
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null);

    Task ReplaceCompetitionStatisticsAsync(
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId,
        Guid? tournamentGroupId,
        Guid? playoffSeriesId,
        IReadOnlyList<HockeyTeamCompetitionStatistics> teams,
        IReadOnlyList<HockeyPlayerCompetitionStatistics> players,
        IReadOnlyList<HockeyGoalieCompetitionStatistics> goalies);

    Task ResetCompetitionStatisticsAsync(
        Guid competitionId,
        HockeyStatisticsScope? scope = null,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null);

    Task<IReadOnlyList<HockeyPlayerCompetitionStatistics>> GetTopScorersAsync(
        Guid competitionId,
        HockeyStatisticsScope scope,
        int topN,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null);

    Task<IReadOnlyList<HockeyGoalieCompetitionStatistics>> GetTopGoaliesAsync(
        Guid competitionId,
        HockeyStatisticsScope scope,
        int topN,
        int minimumGamesPlayed = 1,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null);

    Task<HockeyStatisticsCache?> GetCachedStatisticsAsync(string cacheKey, CancellationToken cancellationToken = default);

    Task SaveCachedStatisticsAsync(HockeyStatisticsCache cache, CancellationToken cancellationToken = default);

    Task<int> RemoveExpiredCacheAsync(CancellationToken cancellationToken = default);

    Task RemoveCompetitionCacheAsync(Guid competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets public competition-scope skater rows for an all-time ranking.
    /// Division, group, and playoff slices are omitted so a player is not counted twice.
    /// Loan profiles and unpublished competitions are excluded.
    /// </summary>
    Task<List<AllTimePlayerStatRow>> GetAllTimePlayerStatRowsAsync(
        TeamCategory teamCategory,
        AllTimeCompetitionFilter competitionType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sums, ranks, and pages public all-time player totals in the database.
    /// Loan profiles, players without games, and unpublished competitions are excluded.
    /// </summary>
    Task<PagedResult<AllTimePlayerTotals>> GetAllTimePlayerPageAsync(
        TeamCategory teamCategory,
        AllTimeCompetitionFilter competitionType,
        AllTimePlayerPageRequest request,
        CancellationToken cancellationToken = default);
}
