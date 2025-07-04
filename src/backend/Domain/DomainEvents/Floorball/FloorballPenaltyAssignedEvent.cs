using Domain.Entities.Floorball;
using Domain.Enums.Floorball;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a penalty is assigned in a floorball match
/// </summary>
public class FloorballPenaltyAssignedEvent : FloorballDomainEvent
{

    /// <summary>
    /// Gets the ID of the match
    /// </summary>
    public Guid MatchId { get; }

    /// <summary>
    /// Gets the ID of the team that received the penalty
    /// </summary>
    public Guid TeamId { get; }

    /// <summary>
    /// Gets the ID of the player who received the penalty
    /// </summary>
    public Guid? PlayerId { get; }

    /// <summary>
    /// Gets the period number when the penalty was assigned
    /// </summary>
    public int PeriodNumber { get; }
    
    /// <summary>
    /// Gets the time in seconds when the penalty was assigned in the period
    /// </summary>
    public int TimeInSeconds { get; }

    /// <summary>
    /// Gets the type of penalty assigned
    /// </summary>
    public FloorballPenaltyType PenaltyType { get; }

    /// <summary>
    /// Gets the penalty duration in minutes
    /// </summary>
    public int Minutes { get; }

    /// <summary>
    /// Gets the reason for the penalty (optional)
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the FloorballPenaltyAssignedEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="teamId">The ID of the team that received the penalty</param>
    /// <param name="playerId">The ID of the player who received the penalty</param>
    /// <param name="penaltyType">The type of penalty assigned</param>
    /// <param name="minutes">The penalty duration in minutes</param>
    /// <param name="periodNumber">The period number when the penalty was assigned</param>
    /// <param name="timeInSeconds">The time in seconds when the penalty was assigned in the period</param>
    /// <param name="description">The reason for the penalty (optional)</param>
    public FloorballPenaltyAssignedEvent(
        Guid matchId,
        Guid teamId,
        Guid? playerId,
        FloorballPenaltyType penaltyType,
        int minutes,
        int periodNumber,
        int timeInSeconds,
        string? description = null)
    {
        if (periodNumber < 1)
        {
            throw new ArgumentException("Period number must be positive", nameof(periodNumber));
        }

        if (timeInSeconds < 0 || timeInSeconds > 1200) // 20 minutes = 1200 seconds max per period
        {
            throw new ArgumentException("Time must be between 0 and 1200 seconds", nameof(timeInSeconds));
        }

        if (minutes <= 0)
        {
            throw new ArgumentException("Penalty minutes must be positive", nameof(minutes));
        }

        MatchId = matchId;
        TeamId = teamId;
        PlayerId = playerId;
        PeriodNumber = periodNumber;
        TimeInSeconds = timeInSeconds;
        PenaltyType = penaltyType;
        Minutes = minutes;
        Description = description ?? string.Empty;
    }
} 
