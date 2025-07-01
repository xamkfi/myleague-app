using Domain.DomainEvents;

namespace Domain.EventSourcing;

/// <summary>
/// Interface for event store implementations
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Saves a collection of domain events to the event store
    /// </summary>
    /// <param name="aggregateId">The ID of the aggregate the events belong to</param>
    /// <param name="events">The collection of events to save</param>
    /// <param name="expectedVersion">The expected version of the aggregate after the last event was applied</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task SaveEventsAsync(Guid aggregateId, IEnumerable<IDomainEvent> events, int expectedVersion, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves all domain events for a given aggregate
    /// </summary>
    /// <param name="aggregateId">The ID of the aggregate to retrieve events for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A collection of domain events for the specified aggregate</returns>
    Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current version of an aggregate
    /// </summary>
    /// <param name="aggregateId">The ID of the aggregate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The current version of the aggregate</returns>
    Task<int> GetAggregateVersionAsync(Guid aggregateId, CancellationToken cancellationToken = default);
} 
