using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Teams;

namespace Domain.Entities.Hockey.Matches;

/// <summary>
/// Current on-ice personnel for one <see cref="HockeyMatchTeam"/>. Can be enabled/disabled per match side.
/// Change log is a light audit trail, not full shift tracking.
/// </summary>
public class HockeyOnIceState : BaseEntity
{
    public Guid MatchTeamId { get; private set; }
    public HockeyMatchTeam MatchTeam { get; private set; } = null!;

    public bool IsEnabled { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }
    public Guid? LastUpdatedByUserId { get; private set; }
    public int Version { get; private set; }

    public IReadOnlyCollection<HockeyOnIcePlayer> PlayersOnIce => _playersOnIce.AsReadOnly();
    private readonly List<HockeyOnIcePlayer> _playersOnIce = new();

    public IReadOnlyCollection<HockeyOnIceChange> ChangeLog => _changeLog.AsReadOnly();
    private readonly List<HockeyOnIceChange> _changeLog = new();

    private HockeyOnIceState() { }

    internal HockeyOnIceState(Guid matchTeamId, bool isEnabled, Guid? createdByUserId = null)
    {
        if (matchTeamId == Guid.Empty)
            throw new ArgumentException("Match team id cannot be empty.", nameof(matchTeamId));

        MatchTeamId = matchTeamId;
        IsEnabled = isEnabled;
        Version = 0;
        LastUpdatedAt = DateTime.UtcNow;
        LastUpdatedByUserId = createdByUserId;
    }

    public void Enable(Guid? userId = null)
    {
        IsEnabled = true;
        Touch(userId);
    }

    public void Disable(Guid? userId = null)
    {
        IsEnabled = false;
        Touch(userId);
    }

    public HockeyOnIcePlayer AddPlayerToIce(
        HockeyMatchActivePlayer activePlayer,
        HockeyIceSlot? slot = null,
        int? order = null,
        bool? isGoalie = null,
        bool isExtraAttacker = false,
        int? periodNumber = null,
        TimeSpan? gameTime = null,
        Guid? createdByUserId = null)
    {
        EnsureTrackingEnabled();
        ArgumentNullException.ThrowIfNull(activePlayer);
        EnsureActiveOnRoster(activePlayer);

        if (_playersOnIce.Any(p => p.MatchActivePlayerId == activePlayer.Id))
            throw new InvalidOperationException("Player is already on the ice.");

        bool goalie = isGoalie ?? activePlayer.IsGoalie;
        HockeyOnIcePlayer onIcePlayer = new(Id, activePlayer.Id, slot, order, goalie, isExtraAttacker);
        _playersOnIce.Add(onIcePlayer);

        AppendChange(
            HockeyOnIceChangeType.PlayerAdded,
            outgoingActivePlayerId: null,
            incomingActivePlayerId: activePlayer.Id,
            appliedLineId: null,
            periodNumber,
            gameTime,
            createdByUserId);

        return onIcePlayer;
    }

    public void RemovePlayerFromIce(
        Guid matchActivePlayerId,
        int? periodNumber = null,
        TimeSpan? gameTime = null,
        Guid? createdByUserId = null)
    {
        EnsureTrackingEnabled();
        HockeyOnIcePlayer? existing = _playersOnIce.FirstOrDefault(p => p.MatchActivePlayerId == matchActivePlayerId)
            ?? throw new InvalidOperationException("Player is not currently on the ice.");

        _playersOnIce.Remove(existing);
        AppendChange(
            HockeyOnIceChangeType.PlayerRemoved,
            outgoingActivePlayerId: matchActivePlayerId,
            incomingActivePlayerId: null,
            appliedLineId: null,
            periodNumber,
            gameTime,
            createdByUserId);
    }

    public void ClearIce(
        int? periodNumber = null,
        TimeSpan? gameTime = null,
        Guid? createdByUserId = null)
    {
        EnsureTrackingEnabled();
        _playersOnIce.Clear();
        AppendChange(
            HockeyOnIceChangeType.IceCleared,
            outgoingActivePlayerId: null,
            incomingActivePlayerId: null,
            appliedLineId: null,
            periodNumber,
            gameTime,
            createdByUserId);
    }

    public void ApplyLine(
        HockeyMatchLine line,
        int? periodNumber = null,
        TimeSpan? gameTime = null,
        Guid? createdByUserId = null)
    {
        EnsureTrackingEnabled();
        ArgumentNullException.ThrowIfNull(line);
        if (line.MatchTeamId != MatchTeamId)
            throw new InvalidOperationException("Line must belong to the same match team.");

        _playersOnIce.Clear();
        foreach (HockeyMatchLinePlayer linePlayer in line.Players)
        {
            HockeyMatchActivePlayer? active = MatchTeam.PlayerSelection?.FindActivePlayer(linePlayer.MatchActivePlayerId)
                ?? throw new InvalidOperationException("Line contains a player that is not on the active match roster.");

            HockeyIceSlot? iceSlot = MapLineSlotToIceSlot(linePlayer.Slot);
            HockeyOnIcePlayer onIcePlayer = new(
                Id,
                active.Id,
                iceSlot,
                linePlayer.Order,
                active.IsGoalie || linePlayer.Slot == HockeyLineSlot.Goalie,
                isExtraAttacker: false);
            _playersOnIce.Add(onIcePlayer);
        }

        AppendChange(
            HockeyOnIceChangeType.LineApplied,
            outgoingActivePlayerId: null,
            incomingActivePlayerId: null,
            appliedLineId: line.Id,
            periodNumber,
            gameTime,
            createdByUserId);
    }

    internal void AttachMatchTeam(HockeyMatchTeam matchTeam)
    {
        ArgumentNullException.ThrowIfNull(matchTeam);
        MatchTeam = matchTeam;
        MatchTeamId = matchTeam.Id;
    }

    private void EnsureTrackingEnabled()
    {
        if (!IsEnabled)
            throw new InvalidOperationException("On-ice tracking is disabled for this match team.");
        if (MatchTeam is not null && !MatchTeam.TracksOnIcePlayers)
            throw new InvalidOperationException("On-ice tracking is not enabled on the match team.");
    }

    private void EnsureActiveOnRoster(HockeyMatchActivePlayer activePlayer)
    {
        if (!activePlayer.IsActive)
            throw new InvalidOperationException("Cannot put an inactive match player on the ice.");
        if (MatchTeam?.PlayerSelection is null || !MatchTeam.PlayerSelection.HasActivePlayer(activePlayer.Id))
            throw new InvalidOperationException("Player must belong to the active match roster before going on the ice.");
    }

    private void AppendChange(
        HockeyOnIceChangeType changeType,
        Guid? outgoingActivePlayerId,
        Guid? incomingActivePlayerId,
        Guid? appliedLineId,
        int? periodNumber,
        TimeSpan? gameTime,
        Guid? createdByUserId)
    {
        _changeLog.Add(new HockeyOnIceChange(
            Id,
            changeType,
            outgoingActivePlayerId,
            incomingActivePlayerId,
            appliedLineId,
            periodNumber,
            gameTime,
            createdByUserId));

        Touch(createdByUserId);
    }

    private void Touch(Guid? userId)
    {
        Version++;
        LastUpdatedAt = DateTime.UtcNow;
        LastUpdatedByUserId = userId;
    }

    private static HockeyIceSlot? MapLineSlotToIceSlot(HockeyLineSlot? slot) =>
        slot switch
        {
            null => null,
            HockeyLineSlot.Goalie => HockeyIceSlot.Goalie,
            HockeyLineSlot.LeftDefense => HockeyIceSlot.LeftDefense,
            HockeyLineSlot.RightDefense => HockeyIceSlot.RightDefense,
            HockeyLineSlot.LeftWing => HockeyIceSlot.LeftWing,
            HockeyLineSlot.Center => HockeyIceSlot.Center,
            HockeyLineSlot.RightWing => HockeyIceSlot.RightWing,
            HockeyLineSlot.Extra => HockeyIceSlot.ExtraAttacker,
            HockeyLineSlot.Any => HockeyIceSlot.Any,
            _ => null
        };
}
