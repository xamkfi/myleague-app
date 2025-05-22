using Domain.DomainEvents;
using Domain.ValueObjects.Hockey;

namespace Domain.DomainEvents.Hockey;

/// <summary>
/// Event raised when a hockey player's position is updated
/// </summary>
public class HockeyPlayerPositionUpdatedEvent : IDomainEvent
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
    /// Gets the ID of the player
    /// </summary>
    public Guid PlayerId { get; }

    /// <summary>
    /// Gets the player's new position information
    /// </summary>
    public Position Position { get; }

    /// <summary>
    /// Initializes a new instance of the HockeyPlayerPositionUpdatedEvent class
    /// </summary>
    /// <param name="playerId">The ID of the player</param>
    /// <param name="position">The player's new position information</param>
    public HockeyPlayerPositionUpdatedEvent(Guid playerId, Position position)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        PlayerId = playerId;
        Position = position;
    }
} 