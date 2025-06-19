using Domain.DomainEvents;

namespace Domain.DomainEvents.Common;

/// <summary>
/// Event raised when a division is created
/// </summary>
public class DivisionCreatedEvent : IDomainEvent
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
    /// Gets the name of the division
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the division
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the level of the division
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// Gets the sport type of the division
    /// </summary>
    public string SportType { get; }

    /// <summary>
    /// Initializes a new instance of the DivisionCreatedEvent class
    /// </summary>
    /// <param name="divisionId">The ID of the division</param>
    /// <param name="name">The name of the division</param>
    /// <param name="description">The description of the division</param>
    /// <param name="level">The level of the division</param>
    /// <param name="sportType">The sport type of the division</param>
    public DivisionCreatedEvent(Guid divisionId, string name, string description, int level, string sportType)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        DivisionId = divisionId;
        Name = name;
        Description = description;
        Level = level;
        SportType = sportType;
    }
} 