using Domain.Enums.Hockey.Matches;

namespace Domain.Entities.Hockey.Matches;

/// <summary>
/// Lightweight on-ice change log entry. Not a full shift-tracking system.
/// </summary>
public class HockeyOnIceChange : BaseEntity
{
    public Guid OnIceStateId { get; private set; }
    public HockeyOnIceState OnIceState { get; private set; } = null!;

    public HockeyOnIceChangeType ChangeType { get; private set; }
    public Guid? OutgoingActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer? OutgoingPlayer { get; private set; }
    public Guid? IncomingActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer? IncomingPlayer { get; private set; }
    public Guid? AppliedLineId { get; private set; }
    public HockeyMatchLine? AppliedLine { get; private set; }
    public int? PeriodNumber { get; private set; }
    public TimeSpan? GameTime { get; private set; }
    public Guid? CreatedByUserId { get; private set; }

    private HockeyOnIceChange() { }

    internal HockeyOnIceChange(
        Guid onIceStateId,
        HockeyOnIceChangeType changeType,
        Guid? outgoingActivePlayerId,
        Guid? incomingActivePlayerId,
        Guid? appliedLineId,
        int? periodNumber,
        TimeSpan? gameTime,
        Guid? createdByUserId)
    {
        if (onIceStateId == Guid.Empty)
            throw new ArgumentException("On-ice state id cannot be empty.", nameof(onIceStateId));
        if (periodNumber is < 0)
            throw new ArgumentOutOfRangeException(nameof(periodNumber), "Period number cannot be negative.");

        OnIceStateId = onIceStateId;
        ChangeType = changeType;
        OutgoingActivePlayerId = outgoingActivePlayerId;
        IncomingActivePlayerId = incomingActivePlayerId;
        AppliedLineId = appliedLineId;
        PeriodNumber = periodNumber;
        GameTime = gameTime;
        CreatedByUserId = createdByUserId;
    }
}
