using Domain.DomainEvents;

namespace Domain.DomainEvents.Common;

/// <summary>
/// Event raised when a division is activated
/// </summary>
public class DivisionActivatedEvent : IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the event
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the date and time when the event occurred
    /// </summary>
    public DateTime OccurredOn { get; }

    /// <summary>
    /// Gets the ID of the division
    /// </summary>
    public Guid DivisionId { get; }

    /// <summary>
    /// Initializes a new instance of the DivisionActivatedEvent class
    /// </summary>
    /// <param name="divisionId">The ID of the division</param>
    public DivisionActivatedEvent(Guid divisionId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        DivisionId = divisionId;
    }
} 