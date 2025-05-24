using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.ValueObjects.Hockey;

namespace Domain.ValueObjects.Hockey;
/// <summary>
/// Represents a goal scored during a hockey match
/// </summary>
public class GoalEventValue : MatchEventBaseValue
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
    /// Private constructor for EF Core
    /// </summary>
    private GoalEventValue() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the GoalEventValue class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="teamId">The ID of the scoring team</param>
    /// <param name="scoringPlayerId">The ID of the player who scored</param>
    /// <param name="assistingPlayerId">The ID of the player who assisted</param>
    /// <param name="periodNumber">The period number</param>
    /// <param name="timeInSeconds">The time in seconds</param>
    /// <param name="goalTypeId">The goal type ID</param>
    /// <param name="description">The description of the goal</param>
    public GoalEventValue(
        Guid matchId,
        Guid teamId,
        Guid? scoringPlayerId,
        Guid? assistingPlayerId,
        int periodNumber,
        int timeInSeconds,
        string? description = null)
        : base(matchId, teamId, periodNumber, timeInSeconds, description)
    {
        ScoringPlayerId = scoringPlayerId;
        AssistingPlayerId = assistingPlayerId;
    }
}
