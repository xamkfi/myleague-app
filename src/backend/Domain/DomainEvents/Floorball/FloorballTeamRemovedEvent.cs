using Domain.DomainEvents;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a floorball team is removed from a club
/// </summary>
public class FloorballTeamRemovedEvent : IDomainEvent
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
    /// Gets the ID of the club
    /// </summary>
    public Guid ClubId { get; }

    /// <summary>
    /// Gets the ID of the team
    /// </summary>
    public Guid TeamId { get; }

    /// <summary>
    /// Initializes a new instance of the FloorballTeamRemovedEvent class
    /// </summary>
    /// <param name="clubId">The ID of the club</param>
    /// <param name="teamId">The ID of the team</param>
    public FloorballTeamRemovedEvent(Guid clubId, Guid teamId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        ClubId = clubId;
        TeamId = teamId;
    }
} 