using Domain.DomainEvents;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a goalie is changed during a floorball match
/// </summary>
public class FloorballGoalieChangedEvent : IDomainEvent
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
    /// Gets the ID of the team
    /// </summary>
    public Guid TeamId { get; }

    /// <summary>
    /// Gets the ID of the previous goalie (null if no previous goalie)
    /// </summary>
    public Guid? PreviousGoalieId { get; }

    /// <summary>
    /// Gets the ID of the new active goalie
    /// </summary>
    public Guid NewGoalieId { get; }

    /// <summary>
    /// Initializes a new instance of the FloorballGoalieChangedEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="teamId">The ID of the team</param>
    /// <param name="previousGoalieId">The ID of the previous goalie</param>
    /// <param name="newGoalieId">The ID of the new goalie</param>
    public FloorballGoalieChangedEvent(Guid matchId, Guid teamId, Guid? previousGoalieId, Guid newGoalieId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        MatchId = matchId;
        TeamId = teamId;
        PreviousGoalieId = previousGoalieId;
        NewGoalieId = newGoalieId;
    }
}
