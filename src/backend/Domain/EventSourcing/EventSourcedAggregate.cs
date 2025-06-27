using System.Reflection;
using Domain.DomainEvents;

namespace Domain.EventSourcing;

/// <summary>
/// Base class for event-sourced aggregates
/// </summary>
public abstract class EventSourcedAggregate
{
    private readonly List<IDomainEvent> _uncommittedEvents = new();
    
    /// <summary>
    /// Gets the ID of the aggregate
    /// </summary>
    public Guid Id { get; protected set; }
    
    /// <summary>
    /// Gets the current version of the aggregate
    /// </summary>
    public int Version { get; protected set; }
    
    /// <summary>
    /// Gets the uncommitted events
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();
    
    /// <summary>
    /// Applies an event to the aggregate
    /// </summary>
    /// <param name="event">The event to apply</param>
    protected void ApplyEvent(IDomainEvent @event)
    {
        ApplyChange(@event, true);
    }
    
    /// <summary>
    /// Applies a change to the aggregate
    /// </summary>
    /// <param name="event">The event to apply</param>
    /// <param name="isNew">Whether the event is new or loaded from history</param>
    private void ApplyChange(IDomainEvent @event, bool isNew)
    {
        // Use reflection to call the Apply method that matches the event type
        var method = GetType().GetMethod("Apply", BindingFlags.NonPublic | BindingFlags.Instance, new[] { @event.GetType() });

        if (method == null)
        {
            throw new InvalidOperationException($"Could not find a protected 'Apply' method for event type {@event.GetType().Name} in aggregate {GetType().Name}.");
        }

        method.Invoke(this, new object[] { @event });
        
        if (isNew)
        {
            _uncommittedEvents.Add(@event);
        }
        else
        {
            Version++;
        }
    }
    
    /// <summary>
    /// Loads the aggregate from history
    /// </summary>
    /// <param name="history">The history of events</param>
    public void LoadFromHistory(IEnumerable<IDomainEvent> history)
    {
        ArgumentNullException.ThrowIfNull(history);

        foreach (IDomainEvent @event in history)
        {
            ApplyChange(@event, false);
        }
    }
    
    /// <summary>
    /// Marks all events as committed
    /// </summary>
    public void MarkEventsAsCommitted()
    {
        Version += _uncommittedEvents.Count;
        _uncommittedEvents.Clear();
    }
    
    /// <summary>
    /// Initializes a new instance of the EventSourcedAggregate class
    /// </summary>
    protected EventSourcedAggregate()
    {
        Version = -1; // No events applied yet
    }
} 
