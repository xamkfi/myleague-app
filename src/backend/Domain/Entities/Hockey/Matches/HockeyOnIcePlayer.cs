using Domain.Enums.Hockey.Matches;

namespace Domain.Entities.Hockey.Matches;

/// <summary>
/// A player currently on the ice for a match side. Always references a <see cref="HockeyMatchActivePlayer"/>.
/// </summary>
public class HockeyOnIcePlayer : BaseEntity
{
    public Guid OnIceStateId { get; private set; }
    public HockeyOnIceState OnIceState { get; private set; } = null!;

    public Guid MatchActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer MatchActivePlayer { get; private set; } = null!;

    public HockeyIceSlot? Slot { get; private set; }
    public int? Order { get; private set; }
    public bool IsGoalie { get; private set; }
    public bool IsExtraAttacker { get; private set; }
    public DateTime AddedAt { get; private set; }

    private HockeyOnIcePlayer() { }

    internal HockeyOnIcePlayer(
        Guid onIceStateId,
        Guid matchActivePlayerId,
        HockeyIceSlot? slot,
        int? order,
        bool isGoalie,
        bool isExtraAttacker)
    {
        if (onIceStateId == Guid.Empty)
            throw new ArgumentException("On-ice state id cannot be empty.", nameof(onIceStateId));
        if (matchActivePlayerId == Guid.Empty)
            throw new ArgumentException("Match active player id cannot be empty.", nameof(matchActivePlayerId));
        if (order is < 0)
            throw new ArgumentOutOfRangeException(nameof(order), "Order cannot be negative.");

        OnIceStateId = onIceStateId;
        MatchActivePlayerId = matchActivePlayerId;
        Slot = slot;
        Order = order;
        IsGoalie = isGoalie;
        IsExtraAttacker = isExtraAttacker;
        AddedAt = DateTime.UtcNow;
    }

    internal void UpdatePlacement(HockeyIceSlot? slot, int? order, bool isGoalie, bool isExtraAttacker)
    {
        if (order is < 0)
            throw new ArgumentOutOfRangeException(nameof(order), "Order cannot be negative.");

        Slot = slot;
        Order = order;
        IsGoalie = isGoalie;
        IsExtraAttacker = isExtraAttacker;
    }
}
