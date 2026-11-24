using Domain.Enums.Floorball;

using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;

namespace Domain.Entities.Floorball.Matches.Events;

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
    /// Gets the ID of the goalie who conceded the goal. Null when no goalie is charged with it:
    /// own goals, empty-net goals, shootout goals, or no goalie assigned.
    /// </summary>
    public Guid? GoalieId { get; private set; }

    /// <summary>
    /// Gets whether the goal was scored while a penalty against the opponent was delayed.
    /// Combines freely with <see cref="GoalType"/>.
    /// </summary>
    public bool WasDelayedPenalty { get; private set; }

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
    /// <param name="goalieId">The goalie charged with the goal (optional)</param>
    /// <param name="wasDelayedPenalty">Whether the goal was scored during a delayed penalty</param>
    public FloorballGoal(
        Guid matchId,
        Guid teamId,
        Guid? scoringPlayerId,
        Guid? assistingPlayerId,
        Guid? secondaryAssistingPlayerId,
        int periodNumber,
        int timeInSeconds,
        FloorballGoalType? goalType = null,
        string? description = null,
        Guid? goalieId = null,
        bool wasDelayedPenalty = false)
        : base(matchId, teamId, periodNumber, timeInSeconds, description)
    {
        ScoringPlayerId = scoringPlayerId;
        AssistingPlayerId = assistingPlayerId;
        SecondaryAssistingPlayerId = secondaryAssistingPlayerId;
        GoalType = goalType;
        GoalieId = goalieId;
        WasDelayedPenalty = wasDelayedPenalty;
    }

    /// <summary>
    /// Charges the goal to another goalie, or to none.
    /// </summary>
    internal void ChargeToGoalie(Guid? goalieId) => GoalieId = goalieId;

    /// <summary>
    /// Moves the goal to another period and goal type when the way the match was decided is corrected.
    /// </summary>
    internal void MoveToPeriod(int periodNumber, FloorballGoalType goalType)
    {
        PeriodNumber = periodNumber;
        GoalType = goalType;
    }
} 
