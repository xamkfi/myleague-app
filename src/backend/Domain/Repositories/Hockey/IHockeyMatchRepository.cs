using Domain.Common;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Statistics;

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
    /// Deletes one match that is still scheduled. Started and finished matches are left in place.
    /// </summary>
    Task<bool> DeleteIfScheduledAsync(Guid matchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads matches where the given career team appears as home or away,
    /// without events, lines, on-ice state, or player selections. Does not track entities.
    /// </summary>
    Task<IReadOnlyList<HockeyMatch>> GetByTeamIdAsync(Guid teamId);

    /// <summary>
    /// Loads scheduled matches starting at or after <paramref name="from"/> for any of the given career teams,
    /// earliest first, with the same graph as <see cref="GetByTeamIdAsync"/>. Does not track entities.
    /// </summary>
    Task<IReadOnlyList<HockeyMatch>> GetScheduledForTeamsAsync(
        IReadOnlyCollection<Guid> teamIds,
        DateTime from,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Latest matches where any of <paramref name="teamPlayerIds"/> was dressed, newest first,
    /// with player selections but no events, lines, or on-ice state. Does not track entities.
    /// </summary>
    Task<IReadOnlyList<HockeyMatch>> GetRecentForTeamPlayersAsync(
        IReadOnlyCollection<Guid> teamPlayerIds,
        int limit,
        CancellationToken cancellationToken = default);

    Task<bool> HasAnyForTeamAsync(Guid teamId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a match with events and roster data needed for statistics recalculation.
    /// </summary>
    Task<HockeyMatch?> GetByIdForStatisticsAsync(Guid id);

    /// <summary>
    /// Loads statistics-eligible matches of one aggregate scope with events and rosters.
    /// The scope filter runs in SQL; <see cref="HockeyStatisticsScope.Competition"/> loads the whole competition.
    /// </summary>
    Task<IReadOnlyList<HockeyMatch>> GetForStatisticsAsync(
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a newly created match event as added for EF change tracking.
    /// </summary>
    void MarkEventAsAdded(HockeyMatchEvent matchEvent);

    /// <summary>
    /// Marks a removed match event as deleted for EF change tracking.
    /// </summary>
    void MarkEventAsDeleted(HockeyMatchEvent matchEvent);

    /// <summary>
    /// No-tracking load of matches that are live, scheduled to start by <paramref name="upcomingUntil"/>,
    /// or finished since <paramref name="finishedSince"/>. Includes match teams and competition only.
    /// </summary>
    Task<IReadOnlyList<HockeyMatch>> GetLiveAsync(
        Guid? competitionId,
        DateTime upcomingUntil,
        DateTime finishedSince,
        CancellationToken cancellationToken = default);

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
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<HockeyMatchStatus>? statuses = null,
        bool activeSeasonsOnly = false);
}
