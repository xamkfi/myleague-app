using Domain.Entities.Floorball;
using Domain.Enums.Floorball;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a floorball match status changes
/// </summary>
public class FloorballMatchStatusChangedEvent : IDomainEvent
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
    public FloorballMatchStatus PreviousStatus { get; }
    
    /// <summary>
    /// Gets the new status
    /// </summary>
    public FloorballMatchStatus NewStatus { get; }
    
    /// <summary>
    /// Initializes a new instance of the FloorballMatchStatusChangedEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="previousStatus">The previous status</param>
    /// <param name="newStatus">The new status</param>
    public FloorballMatchStatusChangedEvent(
        Guid matchId,
        FloorballMatchStatus previousStatus,
        FloorballMatchStatus newStatus)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        MatchId = matchId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
    }
    
    /// <summary>
    /// Initializes a new instance of the FloorballMatchStatusChangedEvent class from a match
    /// </summary>
    /// <param name="match">The match whose status changed</param>
    /// <param name="previousStatus">The previous status</param>
    public FloorballMatchStatusChangedEvent(
        FloorballMatch match,
        FloorballMatchStatus previousStatus)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        MatchId = match.Id;
        PreviousStatus = previousStatus;
        NewStatus = match.Status;
    }
} 
