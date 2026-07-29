using Domain.Enums.Hockey.Matches;

namespace Domain.Entities.Hockey.Matches.Events;

public class HockeyFaceoff : HockeyMatchEvent
{
    public Guid WinningMatchTeamId { get; private set; }
    public HockeyMatchTeam WinningMatchTeam { get; private set; } = null!;

    public Guid LosingMatchTeamId { get; private set; }
    public HockeyMatchTeam LosingMatchTeam { get; private set; } = null!;

    public Guid? WinningActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer? WinningPlayer { get; private set; }

    public Guid? LosingActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer? LosingPlayer { get; private set; }

    public HockeyFaceoffZone Zone { get; private set; }
    public HockeyFaceoffSpot Spot { get; private set; }

    private HockeyFaceoff() { }

    public HockeyFaceoff(
        Guid matchId,
        Guid winningMatchTeamId,
        Guid losingMatchTeamId,
        int periodNumber,
        TimeSpan gameTime,
        HockeyFaceoffZone zone,
        HockeyFaceoffSpot spot,
        Guid? winningActivePlayerId = null,
        Guid? losingActivePlayerId = null,
        string? description = null)
        : base(
            matchId,
            HockeyMatchEventType.Faceoff,
            periodNumber,
            gameTime,
            matchTeamId: winningMatchTeamId,
            matchActivePlayerId: winningActivePlayerId,
            description: description)
    {
        if (winningMatchTeamId == Guid.Empty)
            throw new ArgumentException("Winning match team id cannot be empty.", nameof(winningMatchTeamId));
        if (losingMatchTeamId == Guid.Empty)
            throw new ArgumentException("Losing match team id cannot be empty.", nameof(losingMatchTeamId));
        if (winningMatchTeamId == losingMatchTeamId)
            throw new ArgumentException("Winning and losing teams must be different.", nameof(losingMatchTeamId));
        if (winningActivePlayerId == Guid.Empty)
            throw new ArgumentException("Winning active player id cannot be empty.", nameof(winningActivePlayerId));
        if (losingActivePlayerId == Guid.Empty)
            throw new ArgumentException("Losing active player id cannot be empty.", nameof(losingActivePlayerId));

        WinningMatchTeamId = winningMatchTeamId;
        LosingMatchTeamId = losingMatchTeamId;
        WinningActivePlayerId = winningActivePlayerId;
        LosingActivePlayerId = losingActivePlayerId;
        Zone = zone;
        Spot = spot;
    }
}
