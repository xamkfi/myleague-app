namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a floorball match goes to shootout
/// </summary>
public class FloorballMatchShootoutStartedEvent : IDomainEvent
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
    /// Initializes a new instance of the FloorballMatchShootoutStartedEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    public FloorballMatchShootoutStartedEvent(Guid matchId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        MatchId = matchId;
    }
} 
