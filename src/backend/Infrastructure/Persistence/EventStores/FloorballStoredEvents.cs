
using Domain.DomainEvents;

namespace MyLeague.Infrastructure.Persistence.EventStores;

/// <summary>
/// Entity for storing events in the event store
/// </summary>
public class FloorballStoredEvent
{
    public Guid Id          { get; set; }           // PK
    public Guid AggregateId { get; set; }           // To which aggregate the event belongs
    public string EventType { get; set; } = null!;  // E.g. "FloorballMatchStartedEvent"
    public string Data      { get; set; } = null!;  // JSON-serialized event
    public int Version      { get; set; }           // Aggregate vers   ion (1,2,3…)
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;  // When the event occurred
}
