using Domain.DomainEvents;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a floorball season is completed
/// </summary>
public class FloorballSeasonCompletedEvent : IDomainEvent
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
    /// Initializes a new instance of the FloorballSeasonCompletedEvent class
    /// </summary>
    /// <param name="seasonId">The ID of the season</param>
    public FloorballSeasonCompletedEvent(Guid seasonId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        SeasonId = seasonId;
    }
} 