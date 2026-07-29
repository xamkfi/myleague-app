using Domain.Enums.Hockey.Matches;

namespace Domain.Entities.Hockey.Matches.Events;

public class HockeyPenalty : HockeyMatchEvent
{
    public Guid PenaltyMatchTeamId { get; private set; }
    public HockeyMatchTeam PenaltyMatchTeam { get; private set; } = null!;

    public Guid? PenalizedActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer? PenalizedPlayer { get; private set; }

    public Guid? ServedByActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer? ServedByPlayer { get; private set; }

    public HockeyPenaltySeverity Severity { get; private set; }
    public HockeyPenaltyOffence Offence { get; private set; }
    public int PenaltyMinutes { get; private set; }
    public TimeSpan? PenaltyStartTime { get; private set; }
    public TimeSpan? PenaltyEndTime { get; private set; }
    public bool IsBenchPenalty { get; private set; }
    public bool IsDelayedPenalty { get; private set; }
    public bool WasServed { get; private set; }

    private HockeyPenalty() { }

    public HockeyPenalty(
        Guid matchId,
        Guid penaltyMatchTeamId,
        int periodNumber,
        TimeSpan gameTime,
        HockeyPenaltySeverity severity,
        HockeyPenaltyOffence offence,
        int penaltyMinutes,
        Guid? penalizedActivePlayerId = null,
        Guid? servedByActivePlayerId = null,
        TimeSpan? penaltyStartTime = null,
        TimeSpan? penaltyEndTime = null,
        bool isBenchPenalty = false,
        bool isDelayedPenalty = false,
        bool wasServed = false,
        string? description = null)
        : base(
            matchId,
            HockeyMatchEventType.Penalty,
            periodNumber,
            gameTime,
            matchTeamId: penaltyMatchTeamId,
            matchActivePlayerId: penalizedActivePlayerId,
            description: description)
    {
        if (penaltyMatchTeamId == Guid.Empty)
            throw new ArgumentException("Penalty match team id cannot be empty.", nameof(penaltyMatchTeamId));
        if (penaltyMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(penaltyMinutes), "Penalty minutes cannot be negative.");
        if (penalizedActivePlayerId == Guid.Empty)
            throw new ArgumentException("Penalized active player id cannot be empty.", nameof(penalizedActivePlayerId));
        if (servedByActivePlayerId == Guid.Empty)
            throw new ArgumentException("Served-by active player id cannot be empty.", nameof(servedByActivePlayerId));
        if (penaltyStartTime < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(penaltyStartTime), "Penalty start time cannot be negative.");
        if (penaltyEndTime < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(penaltyEndTime), "Penalty end time cannot be negative.");

        PenaltyMatchTeamId = penaltyMatchTeamId;
        PenalizedActivePlayerId = penalizedActivePlayerId;
        ServedByActivePlayerId = servedByActivePlayerId;
        Severity = severity;
        Offence = offence;
        PenaltyMinutes = penaltyMinutes;
        PenaltyStartTime = penaltyStartTime;
        PenaltyEndTime = penaltyEndTime;
        IsBenchPenalty = isBenchPenalty;
        IsDelayedPenalty = isDelayedPenalty;
        WasServed = wasServed;
    }

    public void MarkServed() => WasServed = true;
}
