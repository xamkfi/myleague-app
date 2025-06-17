using Domain.DomainEvents;
using Domain.Enums.Floorball;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a player's information is updated in a floorball team
/// </summary>
public class FloorballPlayerUpdatedInTeamEvent : IDomainEvent
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
    /// Gets the updated position of the player
    /// </summary>
    public FloorballPosition Position { get; }

    /// <summary>
    /// Gets the updated jersey number of the player
    /// </summary>
    public int? JerseyNumber { get; }

    /// <summary>
    /// Gets the updated active status of the player
    /// </summary>
    public bool IsActive { get; }

    /// <summary>
    /// Initializes a new instance of the FloorballPlayerUpdatedInTeamEvent class
    /// </summary>
    /// <param name="teamId">The ID of the team</param>
    /// <param name="playerId">The ID of the player</param>
    /// <param name="position">The updated position of the player</param>
    /// <param name="jerseyNumber">The updated jersey number of the player</param>
    /// <param name="isActive">The updated active status of the player</param>
    public FloorballPlayerUpdatedInTeamEvent(Guid teamId, Guid playerId, FloorballPosition position, int? jerseyNumber, bool isActive)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        TeamId = teamId;
        PlayerId = playerId;
        Position = position;
        JerseyNumber = jerseyNumber;
        IsActive = isActive;
    }
} 