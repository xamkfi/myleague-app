using Domain.Entities.Hockey;
using Domain.Enums.Hockey;

namespace Domain.DomainEvents.Hockey;

/// <summary>
/// Event raised when a Hockey match status changes
/// </summary>
public class HockeyMatchStatusChangedEvent : IDomainEvent
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
    /// Gets the previous status
    /// </summary>
    public HockeyMatchStatus PreviousStatus { get; }

    /// <summary>
    /// Gets the new status
    /// </summary>
    public HockeyMatchStatus NewStatus { get; }

    /// <summary>
    /// Initializes a new instance of the HockeyMatchStatusChangedEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="previousStatus">The previous status</param>
    /// <param name="newStatus">The new status</param>
    public HockeyMatchStatusChangedEvent(
        Guid matchId,
        HockeyMatchStatus previousStatus,
        HockeyMatchStatus newStatus)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        MatchId = matchId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
    }

    /// <summary>
    /// Initializes a new instance of the HockeyMatchStatusChangedEvent class from a match
    /// </summary>
    /// <param name="match">The match whose status changed</param>
    /// <param name="previousStatus">The previous status</param>
    public HockeyMatchStatusChangedEvent(
        HockeyMatch match,
        HockeyMatchStatus previousStatus)
    {
        ArgumentNullException.ThrowIfNull(match);

        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        MatchId = match.Id;
        PreviousStatus = previousStatus;
        NewStatus = match.Status;
    }
}
