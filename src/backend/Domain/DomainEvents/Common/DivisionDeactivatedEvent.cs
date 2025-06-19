using Domain.DomainEvents;

namespace Domain.DomainEvents.Common;

/// <summary>
/// Event raised when a division is deactivated
/// </summary>
public class DivisionDeactivatedEvent : IDomainEvent
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
    /// Initializes a new instance of the DivisionDeactivatedEvent class
    /// </summary>
    /// <param name="divisionId">The ID of the division</param>
    public DivisionDeactivatedEvent(Guid divisionId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        DivisionId = divisionId;
    }
} 