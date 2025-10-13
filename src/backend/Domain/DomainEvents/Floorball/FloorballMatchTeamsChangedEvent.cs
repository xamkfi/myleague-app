using Domain.Entities.Floorball;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a floorball match's teams are changed
/// </summary>
public class FloorballMatchTeamsChangedEvent : FloorballDomainEvent
{
    /// <summary>
    /// Gets the ID of the match
    /// </summary>
    public Guid MatchId { get; }
    
    /// <summary>
    /// Gets the previous home team ID
    /// </summary>
    public Guid PreviousHomeTeamId { get; }
    
    /// <summary>
    /// Gets the new home team ID
    /// </summary>
    public Guid NewHomeTeamId { get; }
    
    /// <summary>
    /// Gets the previous away team ID
    /// </summary>
    public Guid PreviousAwayTeamId { get; }
    
    /// <summary>
    /// Gets the new away team ID
    /// </summary>
    public Guid NewAwayTeamId { get; }
    
    /// <summary>
    /// Initializes a new instance of the FloorballMatchTeamsChangedEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="previousHomeTeamId">The previous home team ID</param>
    /// <param name="newHomeTeamId">The new home team ID</param>
    /// <param name="previousAwayTeamId">The previous away team ID</param>
    /// <param name="newAwayTeamId">The new away team ID</param>
    public FloorballMatchTeamsChangedEvent(
        Guid matchId,
        Guid previousHomeTeamId,
        Guid newHomeTeamId,
        Guid previousAwayTeamId,
        Guid newAwayTeamId)
    {
        MatchId = matchId;
        PreviousHomeTeamId = previousHomeTeamId;
        NewHomeTeamId = newHomeTeamId;
        PreviousAwayTeamId = previousAwayTeamId;
        NewAwayTeamId = newAwayTeamId;
    }

    private FloorballMatchTeamsChangedEvent() { }
} 