using Domain.DomainEvents;
using Domain.Entities.Hockey;
using Domain.EventSourcing;

namespace Domain.Repositories.Hockey;

/// <summary>
/// Repository for managing event-sourced Hockey matches
/// </summary>
public interface IEventSourcedHockeyMatchRepository
{
    /// <summary>
    /// Gets a match by ID
    /// </summary>
    /// <param name="id">The match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The match if found, null otherwise</returns>
    Task<EventSourcedHockeyMatch> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a match
    /// </summary>
    /// <param name="match">The match to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task SaveAsync(EventSourcedHockeyMatch match, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the full event history for a match
    /// </summary>
    /// <param name="matchId">The match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sequence of events for the match</returns>
    Task<IEnumerable<IDomainEvent>> GetHistoryAsync(Guid matchId, CancellationToken cancellationToken = default);
}
