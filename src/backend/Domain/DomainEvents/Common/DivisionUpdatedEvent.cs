using Domain.DomainEvents;

namespace Domain.DomainEvents.Common;

/// <summary>
/// Event raised when a division is updated
/// </summary>
public class DivisionUpdatedEvent : IDomainEvent
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
    /// Gets the updated name of the division
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the updated description of the division
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the updated level of the division
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// Initializes a new instance of the DivisionUpdatedEvent class
    /// </summary>
    /// <param name="divisionId">The ID of the division</param>
    /// <param name="name">The updated name of the division</param>
    /// <param name="description">The updated description of the division</param>
    /// <param name="level">The updated level of the division</param>
    public DivisionUpdatedEvent(Guid divisionId, string name, string description, int level)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        DivisionId = divisionId;
        Name = name;
        Description = description;
        Level = level;
    }
} 