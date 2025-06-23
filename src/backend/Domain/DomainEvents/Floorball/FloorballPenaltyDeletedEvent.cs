using Domain.Entities.Floorball;
using Domain.Enums.Floorball;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a penalty is deleted from a floorball match
/// </summary>
public class FloorballPenaltyDeletedEvent : FloorballDomainEvent
{
    /// <summary>
    /// Gets the ID of the match
    /// </summary>
    public Guid MatchId { get; }

    /// <summary>
    /// Gets the ID of the team that received the deleted penalty
    /// </summary>
    public Guid TeamId { get; }

    /// <summary>
    /// Gets the ID of the player who received the deleted penalty
    /// </summary>
    public Guid? PlayerId { get; }

    /// <summary>
    /// Gets the type of penalty that was deleted
    /// </summary>
    public FloorballPenaltyType PenaltyType { get; }

    /// <summary>
    /// Gets the duration of the deleted penalty in minutes
    /// </summary>
    public int Minutes { get; }

    /// <summary>
    /// Gets the period number when the penalty was assigned
    /// </summary>
    public int PeriodNumber { get; }
    
    /// <summary>
    /// Gets the time in seconds when the penalty was assigned in the period
    /// </summary>
    public int TimeInSeconds { get; }

    /// <summary>
    /// Gets the description of the deleted penalty
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the FloorballPenaltyDeletedEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="teamId">The ID of the team that received the penalty</param>
    /// <param name="playerId">The ID of the player who received the penalty</param>
    /// <param name="penaltyType">The type of penalty</param>
    /// <param name="minutes">The duration of the penalty in minutes</param>
    /// <param name="periodNumber">The period number when the penalty was assigned</param>
    /// <param name="timeInSeconds">The time in seconds when the penalty was assigned</param>
    /// <param name="description">The description of the penalty</param>
    public FloorballPenaltyDeletedEvent(
        Guid matchId,
        Guid teamId,
        Guid? playerId,
        FloorballPenaltyType penaltyType,
        int minutes,
        int periodNumber,
        int timeInSeconds,
        string description)
    {
        MatchId = matchId;
        TeamId = teamId;
        PlayerId = playerId;
        PenaltyType = penaltyType;
        Minutes = minutes;
        PeriodNumber = periodNumber;
        TimeInSeconds = timeInSeconds;
        Description = description;
    }
} 