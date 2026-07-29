using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Teams;

namespace Domain.Entities.Hockey.Matches;

/// <summary>
/// A player dressed for a specific match. Always references a <see cref="HockeyTeamPlayer"/>;
/// match events later refer to this entity, never directly to the career player.
/// </summary>
public class HockeyMatchActivePlayer : BaseEntity
{
    public Guid MatchPlayerSelectionId { get; private set; }
    public HockeyMatchPlayerSelection MatchPlayerSelection { get; private set; } = null!;

    public Guid TeamPlayerId { get; private set; }
    public HockeyTeamPlayer? TeamPlayer { get; private set; }

    public int JerseyNumber { get; private set; }
    public HockeyPosition Position { get; private set; }
    public HockeyCaptainRole CaptainRole { get; private set; }
    public bool IsStartingPlayer { get; private set; }
    public bool IsGoalie { get; private set; }
    public bool IsEmergencyGoalie { get; private set; }
    public bool IsActive { get; private set; }

    private HockeyMatchActivePlayer() { }

    internal HockeyMatchActivePlayer(
        Guid matchPlayerSelectionId,
        Guid teamPlayerId,
        int jerseyNumber,
        HockeyPosition position,
        HockeyCaptainRole captainRole,
        bool isStartingPlayer,
        bool isGoalie,
        bool isEmergencyGoalie)
    {
        if (matchPlayerSelectionId == Guid.Empty)
            throw new ArgumentException("Match player selection id cannot be empty.", nameof(matchPlayerSelectionId));
        if (teamPlayerId == Guid.Empty)
            throw new ArgumentException("Team player id cannot be empty.", nameof(teamPlayerId));
        if (jerseyNumber < 0 || jerseyNumber > 99)
            throw new ArgumentOutOfRangeException(nameof(jerseyNumber), "Jersey number must be between 0 and 99.");

        MatchPlayerSelectionId = matchPlayerSelectionId;
        TeamPlayerId = teamPlayerId;
        JerseyNumber = jerseyNumber;
        Position = position;
        CaptainRole = captainRole;
        IsStartingPlayer = isStartingPlayer;
        IsGoalie = isGoalie;
        IsEmergencyGoalie = isEmergencyGoalie;
        IsActive = true;
    }

    internal void Deactivate() => IsActive = false;

    internal void Reactivate() => IsActive = true;

    internal void UpdateSnapshot(
        int jerseyNumber,
        HockeyPosition position,
        HockeyCaptainRole captainRole,
        bool isStartingPlayer,
        bool isGoalie,
        bool isEmergencyGoalie)
    {
        if (jerseyNumber < 0 || jerseyNumber > 99)
            throw new ArgumentOutOfRangeException(nameof(jerseyNumber), "Jersey number must be between 0 and 99.");

        JerseyNumber = jerseyNumber;
        Position = position;
        CaptainRole = captainRole;
        IsStartingPlayer = isStartingPlayer;
        IsGoalie = isGoalie;
        IsEmergencyGoalie = isEmergencyGoalie;
    }
}
