using Domain.Enums.Football;

namespace Domain.Entities.Football.Matches;

/// <summary>
/// A goal scored during a football match. Own goals credit the opposing team
/// while still recording the player who put the ball in.
/// </summary>
public class FootballGoal : FootballMatchEvent
{
    public Guid? ScoringPlayerId { get; private set; }
    public Guid? AssistingPlayerId { get; private set; }
    public FootballGoalType? GoalType { get; private set; }

    private FootballGoal()
    {
    }

    public FootballGoal(
        Guid matchId,
        Guid teamId,
        Guid? scoringPlayerId,
        Guid? assistingPlayerId,
        int periodNumber,
        int timeInSeconds,
        FootballGoalType? goalType = null,
        string? description = null)
        : base(matchId, teamId, periodNumber, timeInSeconds, description)
    {
        ScoringPlayerId = scoringPlayerId;
        AssistingPlayerId = assistingPlayerId;
        GoalType = goalType;
    }

    public bool IsOwnGoal => GoalType == FootballGoalType.OwnGoal;
}
