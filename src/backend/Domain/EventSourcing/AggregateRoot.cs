using Domain.DomainEvents;
using Domain.Entities;

namespace Domain.EventSourcing;

/// <summary>
/// Base class for all aggregate roots in the domain model
/// </summary>
public abstract class AggregateRoot : BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets the domain events raised by this aggregate
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Protected constructor for aggregate roots
    /// </summary>
    protected AggregateRoot() : base()
    {
    }

    /// <summary>
    /// Protected constructor with existing ID
    /// </summary>
    /// <param name="id">The existing ID for the entity</param>
    protected AggregateRoot(Guid id) : base(id)
    {
    }

    /// <summary>
    /// Adds a domain event to this aggregate
    /// </summary>
    /// <param name="domainEvent">The domain event to add</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
} 
