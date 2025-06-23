using Domain.Enums.Floorball;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a penalty given during a floorball match
/// </summary>
public class FloorballPenalty : FloorballMatchEvent
{
    /// <summary>
    /// Gets the ID of the player who received the penalty
    /// </summary>
    public Guid? PlayerId { get; private set; }
    
    /// <summary>
    /// Gets the type of penalty
    /// </summary>
    public FloorballPenaltyType PenaltyType { get; private set; }
    
    /// <summary>
    /// Gets the duration of the penalty in minutes
    /// </summary>
    public int DurationInMinutes { get; private set; }
    
    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballPenalty() : base()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the FloorballPenalty class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="teamId">The ID of the penalized team</param>
    /// <param name="playerId">The ID of the player who received the penalty</param>
    /// <param name="penaltyType">The type of penalty</param>
    /// <param name="durationInMinutes">The duration of the penalty in minutes</param>
    /// <param name="periodNumber">The period number</param>
    /// <param name="timeInSeconds">The time in seconds</param>
    /// <param name="description">The description of the penalty</param>
    public FloorballPenalty(
        Guid matchId,
        Guid teamId,
        Guid? playerId,
        FloorballPenaltyType penaltyType,
        int durationInMinutes,
        int periodNumber,
        int timeInSeconds,
        string? description = null) 
        : base(matchId, teamId, periodNumber, timeInSeconds, description)
    {
        if (durationInMinutes <= 0)
            throw new ArgumentException("Penalty duration must be positive.", nameof(durationInMinutes));
        
        PlayerId = playerId;
        PenaltyType = penaltyType;
        DurationInMinutes = durationInMinutes;
    }
} 