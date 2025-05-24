namespace Domain.DomainEvents.Hockey;

/// <summary>
/// Event raised when a Hockey match goes to shootout
/// </summary>
public class HockeyMatchShootoutStartedEvent : IDomainEvent
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
    /// Initializes a new instance of the HockeyMatchShootoutStartedEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    public HockeyMatchShootoutStartedEvent(Guid matchId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        MatchId = matchId;
    }
}
