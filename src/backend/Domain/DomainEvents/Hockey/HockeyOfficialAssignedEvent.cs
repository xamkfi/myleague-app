using Domain.Entities.Hockey;

namespace Domain.DomainEvents.Hockey;

/// <summary>
/// Event raised when an official is assigned to a Hockey match
/// </summary>
public class HockeyOfficialAssignedEvent : IDomainEvent
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
    /// Initializes a new instance of the HockeyOfficialAssignedEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="refereeId">The ID of the referee</param>
    public HockeyOfficialAssignedEvent(Guid matchId, Guid refereeId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        MatchId = matchId;
        RefereeId = refereeId;
    }
}
