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
    /// Marks a newly created match event as added for EF change tracking.
    /// </summary>
    void MarkEventAsAdded(HockeyMatchEvent matchEvent);
}
