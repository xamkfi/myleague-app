using Domain2.DomainEvents;

namespace Domain2.EventSourcing;

/// <summary>
/// Interface for event store implementations
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Saves events to the event store
    /// </summary>
    /// <param name="aggregateId">The ID of the aggregate that generated the events</param>
    /// <param name="events">The events to save</param>
    /// <param name="expectedVersion">The expected version of the aggregate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task SaveEventsAsync(Guid aggregateId, IEnumerable<IDomainEvent> events, int expectedVersion, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets events for a specific aggregate
    /// </summary>
    /// <param name="aggregateId">The ID of the aggregate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A collection of events for the aggregate</returns>
    Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current version of an aggregate
    /// </summary>
    /// <param name="aggregateId">The ID of the aggregate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The current version of the aggregate</returns>
    Task<int> GetAggregateVersionAsync(Guid aggregateId, CancellationToken cancellationToken = default);
} 