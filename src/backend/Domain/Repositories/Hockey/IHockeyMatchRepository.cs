using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;

namespace Domain.Repositories.Hockey;

/// <summary>
/// Repository for hockey matches.
/// </summary>
public interface IHockeyMatchRepository
{
    Task AddAsync(HockeyMatch match);

    Task<HockeyMatch?> GetByIdAsync(Guid id);

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
}
