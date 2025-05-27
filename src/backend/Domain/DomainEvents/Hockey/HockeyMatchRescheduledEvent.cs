namespace Domain.DomainEvents.Hockey;

/// <summary>
/// Event raised when a Hockey match is rescheduled
/// </summary>
public class HockeyMatchRescheduledEvent : IDomainEvent
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
    /// Gets the previous scheduled date and time
    /// </summary>
    public DateTime PreviousScheduledDateTime { get; }

    /// <summary>
    /// Gets the new scheduled date and time
    /// </summary>
    public DateTime NewScheduledDateTime { get; }

    /// <summary>
    /// Gets the previous venue
    /// </summary>
    public string PreviousVenue { get; }

    /// <summary>
    /// Gets the new venue
    /// </summary>
    public string NewVenue { get; }

    /// <summary>
    /// Initializes a new instance of the HockeyMatchRescheduledEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="previousScheduledDateTime">The previous scheduled date and time</param>
    /// <param name="newScheduledDateTime">The new scheduled date and time</param>
    /// <param name="previousVenue">The previous venue</param>
    /// <param name="newVenue">The new venue</param>
    public HockeyMatchRescheduledEvent(
        Guid matchId,
        DateTime previousScheduledDateTime,
        DateTime newScheduledDateTime,
        string previousVenue,
        string newVenue)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        MatchId = matchId;
        PreviousScheduledDateTime = previousScheduledDateTime;
        NewScheduledDateTime = newScheduledDateTime;
        PreviousVenue = previousVenue;
        NewVenue = newVenue;
    }
}
