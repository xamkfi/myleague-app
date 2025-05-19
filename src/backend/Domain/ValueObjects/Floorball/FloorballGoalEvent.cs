using System;
using Domain2.Enums;

namespace Domain2.ValueObjects.Floorball;

/// <summary>
/// Represents a goal scored during a floorball match
/// </summary>
public class FloorballGoalEvent : FloorballMatchEventBase
{
    /// <summary>
    /// Gets the ID of the player who scored the goal
    /// </summary>
    public Guid? ScoringPlayerId { get; private set; }
    
    /// <summary>
    /// Gets the ID of the player who assisted the goal
    /// </summary>
    public Guid? AssistingPlayerId { get; private set; }
    
    /// <summary>
    /// Gets the goal type ID
    /// </summary>
    public int? GoalTypeId { get; private set; }
    
    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballGoalEvent() : base()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the FloorballGoalEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="teamId">The ID of the scoring team</param>
    /// <param name="scoringPlayerId">The ID of the player who scored</param>
    /// <param name="assistingPlayerId">The ID of the player who assisted</param>
    /// <param name="periodNumber">The period number</param>
    /// <param name="timeInSeconds">The time in seconds</param>
    /// <param name="goalTypeId">The goal type ID</param>
    /// <param name="description">The description of the goal</param>
    public FloorballGoalEvent(
        Guid matchId,
        Guid teamId,
        Guid? scoringPlayerId,
        Guid? assistingPlayerId,
        int periodNumber,
        int timeInSeconds,
        int? goalTypeId = null,
        string description = null) 
        : base(matchId, teamId, periodNumber, timeInSeconds, description)
    {
        ScoringPlayerId = scoringPlayerId;
        AssistingPlayerId = assistingPlayerId;
        GoalTypeId = goalTypeId;
    }
} 