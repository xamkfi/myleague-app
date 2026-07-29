using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Teams;

namespace Domain.Entities.Hockey.Matches;

/// <summary>
/// Match-day roster selection for one <see cref="HockeyMatchTeam"/> side.
/// Owns the list of <see cref="HockeyMatchActivePlayer"/> entries used by lines, on-ice tracking and later events.
/// </summary>
public class HockeyMatchPlayerSelection : BaseEntity
{
    public Guid MatchTeamId { get; private set; }
    public HockeyMatchTeam MatchTeam { get; private set; } = null!;

    public HockeyPlayerSelectionSource Source { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public Guid? ConfirmedByUserId { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public bool IsConfirmed { get; private set; }

    public IReadOnlyCollection<HockeyMatchActivePlayer> ActivePlayers => _activePlayers.AsReadOnly();
    private readonly List<HockeyMatchActivePlayer> _activePlayers = new();

    private HockeyMatchPlayerSelection() { }

    internal HockeyMatchPlayerSelection(
        Guid matchTeamId,
        HockeyPlayerSelectionSource source,
        Guid? createdByUserId)
    {
        if (matchTeamId == Guid.Empty)
            throw new ArgumentException("Match team id cannot be empty.", nameof(matchTeamId));

        MatchTeamId = matchTeamId;
        Source = source;
        CreatedByUserId = createdByUserId;
        IsConfirmed = false;
    }

    /// <summary>
    /// Adds a team roster member to the match selection. Snapshots jersey/position/captain from
    /// <paramref name="teamPlayer"/> unless overrides are provided.
    /// </summary>
    public HockeyMatchActivePlayer AddActivePlayer(
        HockeyTeamPlayer teamPlayer,
        int? jerseyNumber = null,
        HockeyPosition? position = null,
        HockeyCaptainRole? captainRole = null,
        bool isStartingPlayer = false,
        bool isGoalie = false,
        bool isEmergencyGoalie = false)
    {
        ArgumentNullException.ThrowIfNull(teamPlayer);
        if (!teamPlayer.IsActive)
            throw new InvalidOperationException("Team player is not active on the roster.");
        if (MatchTeam is null)
            throw new InvalidOperationException("Match team navigation must be set before adding active players.");
        if (teamPlayer.TeamId != MatchTeam.TeamId)
            throw new InvalidOperationException("Team player must belong to the same team as the match side.");

        HockeyMatchActivePlayer? existing = _activePlayers.FirstOrDefault(p => p.TeamPlayerId == teamPlayer.Id);
        if (existing is not null)
        {
            if (existing.IsActive)
                throw new InvalidOperationException("Team player is already in the active match roster.");

            existing.Reactivate();
            existing.UpdateSnapshot(
                jerseyNumber ?? teamPlayer.JerseyNumber ?? 0,
                position ?? teamPlayer.Position,
                captainRole ?? teamPlayer.CaptainRole,
                isStartingPlayer,
                isGoalie,
                isEmergencyGoalie);
            IsConfirmed = false;
            ConfirmedAt = null;
            ConfirmedByUserId = null;
            return existing;
        }

        int resolvedJersey = jerseyNumber ?? teamPlayer.JerseyNumber
            ?? throw new InvalidOperationException("Jersey number is required when the team player has none assigned.");

        HockeyMatchActivePlayer activePlayer = new(
            Id,
            teamPlayer.Id,
            resolvedJersey,
            position ?? teamPlayer.Position,
            captainRole ?? teamPlayer.CaptainRole,
            isStartingPlayer,
            isGoalie,
            isEmergencyGoalie);

        _activePlayers.Add(activePlayer);
        IsConfirmed = false;
        ConfirmedAt = null;
        ConfirmedByUserId = null;
        return activePlayer;
    }

    public void DeactivatePlayer(Guid matchActivePlayerId)
    {
        HockeyMatchActivePlayer player = _activePlayers.FirstOrDefault(p => p.Id == matchActivePlayerId)
            ?? throw new InvalidOperationException("Active player is not part of this selection.");

        player.Deactivate();
        IsConfirmed = false;
        ConfirmedAt = null;
        ConfirmedByUserId = null;
    }

    public void Confirm(Guid? confirmedByUserId = null)
    {
        if (!_activePlayers.Any(p => p.IsActive))
            throw new InvalidOperationException("Cannot confirm an empty match roster.");

        IsConfirmed = true;
        ConfirmedAt = DateTime.UtcNow;
        ConfirmedByUserId = confirmedByUserId;
    }

    public HockeyMatchActivePlayer? FindActivePlayer(Guid matchActivePlayerId) =>
        _activePlayers.FirstOrDefault(p => p.Id == matchActivePlayerId && p.IsActive);

    public bool HasActivePlayer(Guid matchActivePlayerId) =>
        FindActivePlayer(matchActivePlayerId) is not null;

    internal void AttachMatchTeam(HockeyMatchTeam matchTeam)
    {
        ArgumentNullException.ThrowIfNull(matchTeam);
        MatchTeam = matchTeam;
        MatchTeamId = matchTeam.Id;
    }
}
