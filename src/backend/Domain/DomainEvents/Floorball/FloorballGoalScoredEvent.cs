using Domain.Entities.Floorball;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a goal is scored in a floorball match
/// </summary>
public class FloorballGoalScoredEvent : FloorballDomainEvent
{
    /// <summary>
    /// Gets the ID of the match
    /// </summary>
    public Guid MatchId { get; }

    /// <summary>
    /// Gets the ID of the team that scored
    /// </summary>
    public Guid TeamId { get; }

    /// <summary>
    /// Gets the ID of the player who scored
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
    /// Gets whether the goal was scored in overtime
    /// </summary>
    public bool IsOvertime { get; }

    /// <summary>
    /// Gets whether the goal was scored from a penalty shot
    /// </summary>
    public bool IsPenaltyShot { get; }

    /// <summary>
    /// Gets the ID of the player who assisted (if any)
    /// </summary>
    public Guid? AssisterId { get; }

    /// <summary>
    /// Gets the ID of the second player who assisted (if any)
    /// </summary>
    public Guid? SecondaryAssisterId { get; }

    /// <summary>
    /// Initializes a new instance of the FloorballGoalScoredEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="teamId">The ID of the team that scored</param>
    /// <param name="playerId">The ID of the player who scored</param>
    /// <param name="periodNumber">The period number when the goal was scored</param>
    /// <param name="timeInSeconds">The time in seconds when the goal was scored in the period</param>
    /// <param name="isOvertime">Whether the goal was scored in overtime</param>
    /// <param name="isPenaltyShot">Whether the goal was scored from a penalty shot</param>
    /// <param name="assisterId">The ID of the player who assisted (optional)</param>
    public FloorballGoalScoredEvent(
        Guid matchId,
        Guid teamId,
        Guid? playerId,
        int periodNumber,
        int timeInSeconds,
        bool isOvertime = false,
        bool isPenaltyShot = false,
        Guid? assisterId = null,
        Guid? secondaryAssisterId = null)
    {
        if (periodNumber < 1)
        {
            throw new ArgumentException("Period number must be positive", nameof(periodNumber));
        }

        if (timeInSeconds < 0 || timeInSeconds > 1200) // 20 minutes = 1200 seconds max per period
        {
            throw new ArgumentException("Time must be between 0 and 1200 seconds", nameof(timeInSeconds));
        }

        MatchId = matchId;
        TeamId = teamId;
        PlayerId = playerId;
        PeriodNumber = periodNumber;
        TimeInSeconds = timeInSeconds;
        IsOvertime = isOvertime;
        IsPenaltyShot = isPenaltyShot;
        AssisterId = assisterId;
        SecondaryAssisterId = secondaryAssisterId;
    }

    private FloorballGoalScoredEvent() { }
} 
