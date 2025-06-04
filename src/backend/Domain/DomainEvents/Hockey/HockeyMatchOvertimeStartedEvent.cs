namespace Domain.DomainEvents.Hockey;

/// <summary>
/// Event raised when a Hockey match goes to overtime
/// </summary>
public class HockeyMatchOvertimeStartedEvent : IDomainEvent
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
    /// Initializes a new instance of the HockeyMatchOvertimeStartedEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    public HockeyMatchOvertimeStartedEvent(Guid matchId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        MatchId = matchId;
    }
}
