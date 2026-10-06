using Domain.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Enums.Football;

namespace Domain.Repositories.Football;

/// <summary>
/// Repository for football matches.
/// </summary>
public interface IFootballMatchRepository
{
    Task<FootballMatch?> GetByIdAsync(Guid id);
    Task<IEnumerable<FootballMatch>> GetAllAsync();
    Task<PagedResult<FootballMatch>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? competitionId = null,
        Guid? teamId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        FootballMatchStatus? status = null,
        string sortOrder = "desc",
        string? searchQuery = null,
        Guid? tournamentGroupId = null,
        FootballCompetitionType? competitionType = null,
        Domain.Enums.Common.TeamCategory? teamCategory = null,
        bool excludeDraftCompetitions = false,
        bool activeCompetitionsOnly = false,
        IReadOnlyCollection<Guid>? teamIds = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts matches per status with the same filters as the admin match list.
    /// Statuses without matches are absent from the result.
    /// </summary>
    Task<IReadOnlyDictionary<FootballMatchStatus, int>> GetStatusCountsAsync(
        Guid? competitionId,
        string? searchQuery,
        FootballCompetitionType? competitionType,
        bool excludeDraftCompetitions,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<FootballMatch>> GetByCompetitionIdAsync(Guid competitionId);

    /// <summary>
    /// Same shape as <see cref="GetByCompetitionIdAsync"/> but does not track entities. Use for reads only.
    /// </summary>
    Task<IReadOnlyList<FootballMatch>> GetByCompetitionIdReadOnlyAsync(Guid competitionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FootballMatch>> GetByTournamentGroupAsync(
        Guid tournamentGroupId,
        FootballMatchStatus? status = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<FootballMatch>> GetByTeamIdAsync(Guid teamId);

    /// <summary>
    /// Latest completed matches of <paramref name="teamIds"/> where the player was in the lineup
    /// or recorded an event. Filtered and limited in SQL; only that page loads events.
    /// Does not track entities.
    /// </summary>
    Task<IReadOnlyList<FootballMatch>> GetRecentCompletedForPlayerAsync(
        Guid playerId,
        IReadOnlyCollection<Guid> teamIds,
        int limit,
        CancellationToken cancellationToken = default);

    Task<bool> HasAnyForTeamAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FootballMatch>> GetUpcomingByTeamIdAsync(Guid teamId, int count = 5);
    Task<IEnumerable<FootballMatch>> GetPastByTeamIdAsync(Guid teamId, int count = 5);
    Task<IEnumerable<FootballMatch>> GetByStatusAsync(FootballMatchStatus status);
    Task<IEnumerable<FootballMatch>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<FootballMatch>> GetTodaysMatchesByTeamAsync(Guid teamId, CancellationToken cancellationToken);
    Task AddAsync(FootballMatch match);
    Task UpdateAsync(FootballMatch match);
    Task DeleteAsync(Guid id);
    /// <summary>
    /// True when the competition has a match that has started, finished, or been cancelled.
    /// Scheduled and postponed matches do not block season deletion. Does not track entities.
    /// </summary>
    Task<bool> HasMatchThatBlocksSeasonDeleteAsync(Guid competitionId, CancellationToken cancellationToken = default);

    Task<int> DeleteAllByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id);
    void MarkEventAsAdded(FootballMatchEvent matchEvent);
    Task<IEnumerable<FootballMatch>> GetLastCompletedByTeamAsync(Guid teamId, Guid? competitionId = null, int count = 5);

    /// <summary>
    /// Completed matches in one competition that involve any of the given teams, newest first.
    /// Callers keep the latest matches per team.
    /// </summary>
    Task<IEnumerable<FootballMatch>> GetLastCompletedForTeamsAsync(
        Guid competitionId,
        IEnumerable<Guid> teamIds,
        CancellationToken cancellationToken = default);
}
