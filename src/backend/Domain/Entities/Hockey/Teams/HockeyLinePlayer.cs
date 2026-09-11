using Domain.Enums.Hockey.Teams;

namespace Domain.Entities.Hockey.Teams;

/// <summary>
/// Places a <see cref="HockeyTeamPlayer"/> into a slot on a <see cref="HockeyLine"/>.
/// </summary>
public class HockeyLinePlayer : BaseEntity
{
    public Guid LineId { get; private set; }
    public HockeyLine Line { get; private set; } = null!;
    public Guid TeamPlayerId { get; private set; }
    public HockeyTeamPlayer TeamPlayer { get; private set; } = null!;
    public HockeyLineSlot Slot { get; private set; }
    public int Order { get; private set; }

    private HockeyLinePlayer() { }

    internal HockeyLinePlayer(Guid lineId, Guid teamPlayerId, HockeyLineSlot slot, int order)
    {
        if (lineId == Guid.Empty)
            throw new ArgumentException("Line id cannot be empty.", nameof(lineId));
        if (teamPlayerId == Guid.Empty)
            throw new ArgumentException("Team player id cannot be empty.", nameof(teamPlayerId));
        if (order < 0)
            throw new ArgumentOutOfRangeException(nameof(order), "Order cannot be negative.");

        LineId = lineId;
        TeamPlayerId = teamPlayerId;
        Slot = slot;
        Order = order;
    }

    internal void UpdateSlot(HockeyLineSlot slot) => Slot = slot;

    internal void UpdateOrder(int order)
    {
        if (order < 0)
            throw new ArgumentOutOfRangeException(nameof(order), "Order cannot be negative.");
        Order = order;
    }
}
