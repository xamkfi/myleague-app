using Domain.Enums.Floorball;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a goal scored during a floorball match
/// </summary>
public class FloorballGoal : FloorballMatchEvent
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
    /// Gets the ID of the second player who assisted the goal
    /// </summary>
    public Guid? SecondaryAssistingPlayerId { get; private set; }
    
    /// <summary>
    /// Gets the type of goal scored
    /// </summary>
    public FloorballGoalType? GoalType { get; private set; }
    
    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballGoal() : base()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the FloorballGoal class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="teamId">The ID of the scoring team</param>
    /// <param name="scoringPlayerId">The ID of the player who scored</param>
    /// <param name="assistingPlayerId">The ID of the player who assisted</param>
    /// <param name="periodNumber">The period number</param>
    /// <param name="timeInSeconds">The time in seconds</param>
    /// <param name="goalType">The type of goal (optional)</param>
    /// <param name="description">The description of the goal</param>
    public FloorballGoal(
        Guid matchId,
        Guid teamId,
        Guid? scoringPlayerId,
        Guid? assistingPlayerId,
        Guid? secondaryAssistingPlayerId,
        int periodNumber,
        int timeInSeconds,
        FloorballGoalType? goalType = null,
        string? description = null) 
        : base(matchId, teamId, periodNumber, timeInSeconds, description)
    {
        ScoringPlayerId = scoringPlayerId;
        AssistingPlayerId = assistingPlayerId;
        SecondaryAssistingPlayerId = secondaryAssistingPlayerId;
        GoalType = goalType;
    }
} 
