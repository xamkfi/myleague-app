using Domain.Common;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Matches;

namespace Domain.Repositories.Hockey;

/// <summary>
/// Repository for hockey matches.
/// </summary>
public interface IHockeyMatchRepository
{
    Task AddAsync(HockeyMatch match);

    Task<HockeyMatch?> GetByIdAsync(Guid id);

    /// <summary>
    /// Loads matches for a competition without events, lines, or on-ice state.
    /// </summary>
    Task<IReadOnlyList<HockeyMatch>> GetByCompetitionIdAsync(Guid competitionId);

    /// <summary>
    /// True when the competition has a match that has started, finished, or been cancelled.
    /// Scheduled and postponed matches do not block season deletion. Does not track entities.
    /// </summary>
    Task<bool> HasMatchThatBlocksSeasonDeleteAsync(Guid competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes every match of a competition after clearing rows that restrict the match,
    /// its teams, or the playoff <c>NextMatchId</c> self-reference.
    /// Persists immediately via <c>ExecuteDelete</c>.
    /// </summary>
    Task<int> DeleteAllByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads matches where the given career team appears as home or away.
    /// </summary>
    Task<IReadOnlyList<HockeyMatch>> GetByTeamIdAsync(Guid teamId);

    Task<bool> HasAnyForTeamAsync(Guid teamId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a match with events and roster data needed for statistics recalculation.
    /// </summary>
    Task<HockeyMatch?> GetByIdForStatisticsAsync(Guid id);

    /// <summary>
    /// Loads competition matches with events and rosters for aggregate recalculation.
    /// </summary>
    Task<IReadOnlyList<HockeyMatch>> GetByCompetitionIdForStatisticsAsync(Guid competitionId);

    /// <summary>
    /// Marks a newly created match event as added for EF change tracking.
    /// </summary>
    void MarkEventAsAdded(HockeyMatchEvent matchEvent);

    /// <summary>
    /// Marks a removed match event as deleted for EF change tracking.
    /// </summary>
    void MarkEventAsDeleted(HockeyMatchEvent matchEvent);

    Task<PagedResult<HockeyMatch>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? competitionId = null,
        Guid? teamId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        HockeyMatchStatus? status = null,
        string sortOrder = "desc",
        string? searchQuery = null,
        TeamCategory? teamCategory = null,
        bool excludeDraftCompetitions = false,
        CancellationToken cancellationToken = default);
}
