using Domain.DomainEvents;
using Domain.Enums.Floorball;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a player is added to a floorball team
/// </summary>
public class FloorballPlayerAddedToTeamEvent : IDomainEvent
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
    /// Gets the ID of the team
    /// </summary>
    public Guid TeamId { get; }

    /// <summary>
    /// Gets the ID of the player
    /// </summary>
    public Guid PlayerId { get; }

    /// <summary>
    /// Gets the position of the player
    /// </summary>
    public FloorballPosition Position { get; }

    /// <summary>
    /// Gets the jersey number of the player
    /// </summary>
    public int? JerseyNumber { get; }

    /// <summary>
    /// Initializes a new instance of the FloorballPlayerAddedToTeamEvent class
    /// </summary>
    /// <param name="teamId">The ID of the team</param>
    /// <param name="playerId">The ID of the player</param>
    /// <param name="position">The position of the player</param>
    /// <param name="jerseyNumber">The jersey number of the player</param>
    public FloorballPlayerAddedToTeamEvent(Guid teamId, Guid playerId, FloorballPosition position, int? jerseyNumber)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        TeamId = teamId;
        PlayerId = playerId;
        Position = position;
        JerseyNumber = jerseyNumber;
    }
} 