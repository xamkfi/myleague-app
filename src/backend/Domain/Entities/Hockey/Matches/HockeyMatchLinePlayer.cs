using Domain.Enums.Hockey.Teams;

namespace Domain.Entities.Hockey.Matches;

/// <summary>
/// A player slot within an ottelu-specific line. References <see cref="HockeyMatchActivePlayer"/>, not the career player.
/// </summary>
public class HockeyMatchLinePlayer : BaseEntity
{
    public Guid MatchLineId { get; private set; }
    public HockeyMatchLine MatchLine { get; private set; } = null!;

    public Guid MatchActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer MatchActivePlayer { get; private set; } = null!;

    public HockeyLineSlot? Slot { get; private set; }
    public int? Order { get; private set; }

    private HockeyMatchLinePlayer() { }

    internal HockeyMatchLinePlayer(
        Guid matchLineId,
        Guid matchActivePlayerId,
        HockeyLineSlot? slot,
        int? order)
    {
        if (matchLineId == Guid.Empty)
            throw new ArgumentException("Match line id cannot be empty.", nameof(matchLineId));
        if (matchActivePlayerId == Guid.Empty)
            throw new ArgumentException("Match active player id cannot be empty.", nameof(matchActivePlayerId));
        if (order is < 0)
            throw new ArgumentOutOfRangeException(nameof(order), "Order cannot be negative.");

        MatchLineId = matchLineId;
        MatchActivePlayerId = matchActivePlayerId;
        Slot = slot;
        Order = order;
    }

    internal void UpdateSlot(HockeyLineSlot? slot) => Slot = slot;

    internal void UpdateOrder(int? order)
    {
        if (order is < 0)
            throw new ArgumentOutOfRangeException(nameof(order), "Order cannot be negative.");
        Order = order;
    }
}
