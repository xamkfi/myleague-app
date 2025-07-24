using Domain.Entities.Floorball;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a floorball match's season is changed
/// </summary>
public class FloorballMatchSeasonChangedEvent : FloorballDomainEvent
{
    /// <summary>
    /// Gets the ID of the match
    /// </summary>
    public Guid MatchId { get; }
    
    /// <summary>
    /// Gets the previous season ID
    /// </summary>
    public Guid PreviousSeasonId { get; }
    
    /// <summary>
    /// Gets the new season ID
    /// </summary>
    public Guid NewSeasonId { get; }
    
    /// <summary>
    /// Initializes a new instance of the FloorballMatchSeasonChangedEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="previousSeasonId">The previous season ID</param>
    /// <param name="newSeasonId">The new season ID</param>
    public FloorballMatchSeasonChangedEvent(
        Guid matchId,
        Guid previousSeasonId,
        Guid newSeasonId)
    {
        MatchId = matchId;
        PreviousSeasonId = previousSeasonId;
        NewSeasonId = newSeasonId;
    }

    private FloorballMatchSeasonChangedEvent() { }
} 