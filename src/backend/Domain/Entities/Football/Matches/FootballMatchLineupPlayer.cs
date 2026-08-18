using Domain.Enums.Football;

namespace Domain.Entities.Football.Matches;

/// <summary>
/// A player in a team's match squad. Goalkeeper is a position in this collection,
/// not a separate match-level field. IsOnField tracks who is currently playing.
/// </summary>
public class FootballMatchLineupPlayer : BaseEntity
{
    public Guid MatchId { get; private set; }
    public Guid TeamId { get; private set; }
    public Guid PlayerId { get; private set; }
    public FootballPosition Position { get; private set; }
    public bool IsOnField { get; private set; }
    public bool IsSentOff { get; private set; }

    private FootballMatchLineupPlayer()
    {
    }

    public FootballMatchLineupPlayer(
        Guid matchId,
        Guid teamId,
        Guid playerId,
        FootballPosition position,
        bool isOnField)
    {
        if (matchId == Guid.Empty)
            throw new ArgumentException("Match ID cannot be empty.", nameof(matchId));
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team ID cannot be empty.", nameof(teamId));
        if (playerId == Guid.Empty)
            throw new ArgumentException("Player ID cannot be empty.", nameof(playerId));
        if (position == FootballPosition.None)
            throw new ArgumentException("Lineup player must have a position.", nameof(position));

        MatchId = matchId;
        TeamId = teamId;
        PlayerId = playerId;
        Position = position;
        IsOnField = isOnField;
        IsSentOff = false;
    }

    public void PutOnField()
    {
        if (IsSentOff)
            throw new InvalidOperationException("A sent-off player cannot return to the field.");
        IsOnField = true;
    }

    public void TakeOffField() => IsOnField = false;

    public void SendOff()
    {
        IsOnField = false;
        IsSentOff = true;
    }

    /// <summary>
    /// Clears a sending-off after the card event that caused it is deleted.
    /// The player returns to the bench, not the field.
    /// </summary>
    public void ClearSendingOff()
    {
        IsSentOff = false;
        IsOnField = false;
    }
}
