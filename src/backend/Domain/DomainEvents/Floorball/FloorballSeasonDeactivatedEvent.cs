using Domain.DomainEvents;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a floorball season is deactivated
/// </summary>
public class FloorballSeasonDeactivatedEvent : IDomainEvent
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
    /// Gets the ID of the season
    /// </summary>
    public Guid SeasonId { get; }

    /// <summary>
    /// Initializes a new instance of the FloorballSeasonDeactivatedEvent class
    /// </summary>
    /// <param name="seasonId">The ID of the season</param>
    public FloorballSeasonDeactivatedEvent(Guid seasonId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        SeasonId = seasonId;
    }
} 