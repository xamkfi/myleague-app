using Domain.Enums.Hockey.Matches;

namespace Domain.Entities.Hockey.Matches.Events;

public class HockeyTimeout : HockeyMatchEvent
{
    private HockeyTimeout() { }

    public HockeyTimeout(
        Guid matchId,
        Guid matchTeamId,
        int periodNumber,
        TimeSpan gameTime,
        string? description = null)
        : base(
            matchId,
            HockeyMatchEventType.Timeout,
            periodNumber,
            gameTime,
            matchTeamId: matchTeamId,
            description: description)
    {
        if (matchTeamId == Guid.Empty)
            throw new ArgumentException("Match team id cannot be empty.", nameof(matchTeamId));
    }
}
