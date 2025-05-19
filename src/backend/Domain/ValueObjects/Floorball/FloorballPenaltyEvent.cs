using System;
using Domain2.Enums;
using Domain2.Enums.Floorball;

namespace Domain2.ValueObjects.Floorball;

/// <summary>
/// Represents a penalty given during a floorball match
/// </summary>
public class FloorballPenaltyEvent : FloorballMatchEventBase
{
    /// <summary>
    /// Gets the ID of the player who received the penalty
    /// </summary>
    public Guid? PlayerId { get; private set; }
    
    /// <summary>
    /// Gets the penalty type ID
    /// </summary>
    public int PenaltyTypeId { get; private set; }
    
    /// <summary>
    /// Gets the penalty minutes
    /// </summary>
    public int PenaltyMinutes { get; private set; }
    
    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballPenaltyEvent() : base()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the FloorballPenaltyEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="teamId">The ID of the penalized team</param>
    /// <param name="playerId">The ID of the player who received the penalty</param>
    /// <param name="penaltyType">The type of penalty</param>
    /// <param name="penaltyMinutes">The penalty minutes</param>
    /// <param name="periodNumber">The period number</param>
    /// <param name="timeInSeconds">The time in seconds</param>
    /// <param name="description">The description of the penalty</param>
    public FloorballPenaltyEvent(
        Guid matchId,
        Guid teamId,
        Guid? playerId,
        FloorballPenaltyType penaltyType,
        int penaltyMinutes,
        int periodNumber,
        int timeInSeconds,
        string description = null) 
        : base(matchId, teamId, periodNumber, timeInSeconds, description)
    {
        PlayerId = playerId;
        PenaltyTypeId = (int)penaltyType;
        PenaltyMinutes = penaltyMinutes;
    }
} 