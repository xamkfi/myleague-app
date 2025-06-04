namespace Domain.DomainEvents.Hockey;

/// <summary>
/// Event raised when a Hockey match is started
/// </summary>
public class HockeyMatchStartedEvent : IDomainEvent
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
    /// Initializes a new instance of the HockeyMatchStartedEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="startTime">The actual start time of the match</param>
    public HockeyMatchStartedEvent(
        Guid matchId,
        DateTime startTime)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        MatchId = matchId;
        StartTime = startTime;
    }
}
