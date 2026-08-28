using Domain.Enums.Hockey.Matches;

namespace Domain.Entities.Hockey.Matches.Events;

public class HockeyPeriodEvent : HockeyMatchEvent
{
    public HockeyPeriodAction Action { get; private set; }

    private HockeyPeriodEvent() { }

    public HockeyPeriodEvent(
        Guid matchId,
        int periodNumber,
        TimeSpan gameTime,
        HockeyPeriodAction action,
        string? description = null)
        : base(matchId, HockeyMatchEventType.Period, periodNumber, gameTime, description: description)
    {
        Action = action;
    }
}
