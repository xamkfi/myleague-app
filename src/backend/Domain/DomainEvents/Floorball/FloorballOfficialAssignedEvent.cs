using Domain2.Entities.Floorball;

namespace Domain2.DomainEvents.Floorball;

/// <summary>
/// Event raised when an official is assigned to a floorball match
/// </summary>
public class FloorballOfficialAssignedEvent : IDomainEvent
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
    /// Gets the ID of the match
    /// </summary>
    public Guid MatchId { get; }

    /// <summary>
    /// Gets the ID of the referee
    /// </summary>
    public Guid RefereeId { get; }

    /// <summary>
    /// Initializes a new instance of the FloorballOfficialAssignedEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="refereeId">The ID of the referee</param>
    public FloorballOfficialAssignedEvent(Guid matchId, Guid refereeId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        MatchId = matchId;
        RefereeId = refereeId;
    }
} 