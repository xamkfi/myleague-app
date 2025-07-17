using Domain.Entities.Floorball;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a floorball match's venue is changed
/// </summary>
public class FloorballMatchVenueChangedEvent : FloorballDomainEvent
{
    /// <summary>
    /// Gets the ID of the match
    /// </summary>
    public Guid MatchId { get; }
    
    /// <summary>
    /// Gets the previous venue
    /// </summary>
    public string? PreviousVenue { get; }
    
    /// <summary>
    /// Gets the new venue
    /// </summary>
    public string? NewVenue { get; }
    
    /// <summary>
    /// Initializes a new instance of the FloorballMatchVenueChangedEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="previousVenue">The previous venue</param>
    /// <param name="newVenue">The new venue</param>
    public FloorballMatchVenueChangedEvent(
        Guid matchId,
        string? previousVenue,
        string? newVenue)
    {
        MatchId = matchId;
        PreviousVenue = previousVenue;
        NewVenue = newVenue;
    }

    private FloorballMatchVenueChangedEvent() { }
} 