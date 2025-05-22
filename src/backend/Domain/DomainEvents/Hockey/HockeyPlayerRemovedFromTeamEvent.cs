using Domain.DomainEvents;

namespace Domain.DomainEvents.Hockey;

/// <summary>
/// Event raised when a player is removed from a Hockey team
/// </summary>
public class HockeyPlayerRemovedFromTeamEvent : IDomainEvent
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
    /// Initializes a new instance of the HockeyPlayerRemovedFromTeamEvent class
    /// </summary>
    /// <param name="teamId">The ID of the team</param>
    /// <param name="playerId">The ID of the player</param>
    public HockeyPlayerRemovedFromTeamEvent(Guid teamId, Guid playerId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        TeamId = teamId;
        PlayerId = playerId;
    }
}
