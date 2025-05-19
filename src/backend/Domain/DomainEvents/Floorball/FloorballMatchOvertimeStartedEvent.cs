namespace Domain2.DomainEvents.Floorball;

/// <summary>
/// Event raised when a floorball match goes to overtime
/// </summary>
public class FloorballMatchOvertimeStartedEvent : IDomainEvent
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
    /// Initializes a new instance of the FloorballMatchOvertimeStartedEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    public FloorballMatchOvertimeStartedEvent(Guid matchId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        MatchId = matchId;
    }
} 