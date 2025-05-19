namespace Domain2.DomainEvents.Floorball;

/// <summary>
/// Event raised when a floorball match is started
/// </summary>
public class FloorballMatchStartedEvent : IDomainEvent
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
    /// Gets the actual start time of the match
    /// </summary>
    public DateTime StartTime { get; }
    
    /// <summary>
    /// Initializes a new instance of the FloorballMatchStartedEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="startTime">The actual start time of the match</param>
    public FloorballMatchStartedEvent(
        Guid matchId,
        DateTime startTime)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        MatchId = matchId;
        StartTime = startTime;
    }
} 