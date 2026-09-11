using Domain.Enums.Hockey.Matches;

namespace Domain.Entities.Hockey.Matches.Events;

public class HockeyGoalieChange : HockeyMatchEvent
{
    public Guid? OutgoingGoalieActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer? OutgoingGoalie { get; private set; }

    public Guid? IncomingGoalieActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer? IncomingGoalie { get; private set; }

    public string? Reason { get; private set; }

    private HockeyGoalieChange() { }

    public HockeyGoalieChange(
        Guid matchId,
        Guid matchTeamId,
        int periodNumber,
        TimeSpan gameTime,
        Guid? outgoingGoalieActivePlayerId = null,
        Guid? incomingGoalieActivePlayerId = null,
        string? reason = null,
        string? description = null)
        : base(
            matchId,
            HockeyMatchEventType.GoalieChange,
            periodNumber,
            gameTime,
            matchTeamId: matchTeamId,
            matchActivePlayerId: incomingGoalieActivePlayerId ?? outgoingGoalieActivePlayerId,
            description: description)
    {
        if (matchTeamId == Guid.Empty)
            throw new ArgumentException("Match team id cannot be empty.", nameof(matchTeamId));
        if (outgoingGoalieActivePlayerId == Guid.Empty)
            throw new ArgumentException("Outgoing goalie id cannot be empty.", nameof(outgoingGoalieActivePlayerId));
        if (incomingGoalieActivePlayerId == Guid.Empty)
            throw new ArgumentException("Incoming goalie id cannot be empty.", nameof(incomingGoalieActivePlayerId));

        OutgoingGoalieActivePlayerId = outgoingGoalieActivePlayerId;
        IncomingGoalieActivePlayerId = incomingGoalieActivePlayerId;
        Reason = reason;
    }
}
