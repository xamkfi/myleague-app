using Domain.Entities.Hockey.Statistics;
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
}
