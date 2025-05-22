using Domain.DomainEvents;
using Domain.ValueObjects.Hockey;

namespace Domain.DomainEvents.Hockey;

/// <summary>
/// Event raised when a hockey player is registered
/// </summary>
public class HockeyPlayerRegisteredEvent : IDomainEvent
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
    /// Gets the ID of the person this player profile belongs to
    /// </summary>
    public Guid PersonId { get; }

    /// <summary>
    /// Gets the player's position information
    /// </summary>
    public Position Position { get; }

    /// <summary>
    /// Initializes a new instance of the HockeyPlayerRegisteredEvent class
    /// </summary>
    /// <param name="playerId">The ID of the player</param>
    /// <param name="personId">The ID of the person this player profile belongs to</param>
    /// <param name="position">The player's position information</param>
    public HockeyPlayerRegisteredEvent(Guid playerId, Guid personId, Position position)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        PlayerId = playerId;
        PersonId = personId;
        Position = position;
    }
} 