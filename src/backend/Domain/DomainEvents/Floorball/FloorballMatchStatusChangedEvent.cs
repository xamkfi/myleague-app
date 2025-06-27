using Domain.Entities.Floorball;
using Domain.Enums.Floorball;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a floorball match status changes
/// </summary>
public class FloorballMatchStatusChangedEvent : FloorballDomainEvent
{
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
        AggregateId = matchId;
        MatchId = matchId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
    }
    
    // Private constructor for EF Core/deserialization
    private FloorballMatchStatusChangedEvent() { }
} 
