using Domain.Entities.Floorball;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a goal is deleted from a floorball match
/// </summary>
public class FloorballGoalDeletedEvent : FloorballDomainEvent
{
    /// <summary>
    /// Gets the ID of the match
    /// </summary>
    public Guid MatchId { get; }

    /// <summary>
    /// Gets the ID of the team that scored the deleted goal
    /// </summary>
    public Guid TeamId { get; }

    /// <summary>
    /// Gets the ID of the player who scored the deleted goal
    /// </summary>
    public Guid? PlayerId { get; }

    /// <summary>
    /// Gets the period number when the goal was scored
    /// </summary>
    public int PeriodNumber { get; }
    
    /// <summary>
    /// Gets the time in seconds when the goal was scored in the period
    /// </summary>
    public int TimeInSeconds { get; }

    /// <summary>
    /// Gets the ID of the player who assisted the deleted goal (if any)
    /// </summary>
    public Guid? AssisterId { get; }

    /// <summary>
    /// Initializes a new instance of the FloorballGoalDeletedEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="teamId">The ID of the team that scored</param>
    /// <param name="playerId">The ID of the player who scored</param>
    /// <param name="periodNumber">The period number when the goal was scored</param>
    /// <param name="timeInSeconds">The time in seconds when the goal was scored</param>
    /// <param name="assisterId">The ID of the player who assisted (if any)</param>
    public FloorballGoalDeletedEvent(
        Guid matchId,
        Guid teamId,
        Guid? playerId,
        int periodNumber,
        int timeInSeconds,
        Guid? assisterId)
    {
        MatchId = matchId;
        TeamId = teamId;
        PlayerId = playerId;
        PeriodNumber = periodNumber;
        TimeInSeconds = timeInSeconds;
        AssisterId = assisterId;
    }
} 