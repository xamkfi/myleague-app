using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Teams;

namespace Domain.Entities.Hockey.Matches;

/// <summary>
/// One side of a hockey match (home or away). Owns match roster selection, match lines
/// and optional on-ice tracking for that side.
/// </summary>
public class HockeyMatchTeam : BaseEntity
{
    public Guid MatchId { get; private set; }
    public HockeyMatch Match { get; private set; } = null!;

    public Guid TeamId { get; private set; }
    public HockeyTeam? Team { get; private set; }

    public Guid? CompetitionTeamId { get; private set; }
    public HockeyCompetitionTeam? CompetitionTeam { get; private set; }

    public HockeyTeamSlot TeamSlot { get; private set; }
    public int Goals { get; private set; }
    public bool IsGoaliePulled { get; private set; }
    public Guid? ActiveGoalieMatchPlayerId { get; private set; }
    public HockeyMatchActivePlayer? ActiveGoalie { get; private set; }
    public bool TracksOnIcePlayers { get; private set; }

    public HockeyMatchPlayerSelection? PlayerSelection { get; private set; }

    public IReadOnlyCollection<HockeyMatchLine> Lines => _lines.AsReadOnly();
    private readonly List<HockeyMatchLine> _lines = new();

    public HockeyOnIceState? OnIceState { get; private set; }

    private HockeyMatchTeam() { }

    internal HockeyMatchTeam(
        Guid matchId,
        Guid teamId,
        HockeyTeamSlot teamSlot,
        Guid? competitionTeamId,
        bool tracksOnIcePlayers)
    {
        if (matchId == Guid.Empty)
            throw new ArgumentException("Match id cannot be empty.", nameof(matchId));
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team id cannot be empty.", nameof(teamId));
        if (teamSlot is not (HockeyTeamSlot.Home or HockeyTeamSlot.Away))
            throw new ArgumentException("Match team slot must be Home or Away.", nameof(teamSlot));

        MatchId = matchId;
        TeamId = teamId;
        TeamSlot = teamSlot;
        CompetitionTeamId = competitionTeamId;
        TracksOnIcePlayers = tracksOnIcePlayers;
        Goals = 0;
        IsGoaliePulled = false;

        if (tracksOnIcePlayers)
        {
            OnIceState = new HockeyOnIceState(Id, isEnabled: true);
            OnIceState.AttachMatchTeam(this);
        }
    }

    internal void SetGoals(int goals)
    {
        if (goals < 0)
            throw new ArgumentOutOfRangeException(nameof(goals), "Goals cannot be negative.");
        Goals = goals;
    }

    internal void IncrementGoals() => Goals += 1;

    internal void SetGoaliePulled(bool isGoaliePulled) => IsGoaliePulled = isGoaliePulled;

    /// <summary>
    /// Creates a new player selection for this match side, replacing any previous selection.
    /// </summary>
    public HockeyMatchPlayerSelection CreateOrReplacePlayerSelection(
        HockeyPlayerSelectionSource source,
        Guid? createdByUserId = null)
    {
        HockeyMatchPlayerSelection selection = new(Id, source, createdByUserId);
        selection.AttachMatchTeam(this);
        PlayerSelection = selection;
        ActiveGoalie = null;
        ActiveGoalieMatchPlayerId = null;
        return selection;
    }

    public HockeyMatchLine AddMatchLine(
        string name,
        HockeyLineType lineType,
        int? lineNumber = null,
        string? notes = null)
    {
        HockeyMatchLine line = new(Id, name, lineType, lineNumber, notes);
        line.AttachMatchTeam(this);
        _lines.Add(line);
        return line;
    }

    public void RemoveMatchLine(Guid matchLineId)
    {
        HockeyMatchLine line = _lines.FirstOrDefault(l => l.Id == matchLineId)
            ?? throw new InvalidOperationException("Match line is not part of this match team.");
        _lines.Remove(line);
    }

    public void EnableOnIceTracking(Guid? userId = null)
    {
        TracksOnIcePlayers = true;
        if (OnIceState is null)
        {
            OnIceState = new HockeyOnIceState(Id, isEnabled: true, userId);
            OnIceState.AttachMatchTeam(this);
        }
        else
        {
            OnIceState.Enable(userId);
        }
    }

    public void DisableOnIceTracking(Guid? userId = null)
    {
        TracksOnIcePlayers = false;
        OnIceState?.Disable(userId);
    }

    public void SetActiveGoalie(HockeyMatchActivePlayer activeGoalie)
    {
        ArgumentNullException.ThrowIfNull(activeGoalie);
        if (!activeGoalie.IsActive)
            throw new InvalidOperationException("Active goalie must be an active match player.");
        if (!activeGoalie.IsGoalie && !activeGoalie.IsEmergencyGoalie)
            throw new InvalidOperationException("Selected player is not marked as a goalie.");
        if (PlayerSelection is null || !PlayerSelection.HasActivePlayer(activeGoalie.Id))
            throw new InvalidOperationException("Active goalie must belong to this match team's roster.");

        ActiveGoalie = activeGoalie;
        ActiveGoalieMatchPlayerId = activeGoalie.Id;
    }

    public void ClearActiveGoalie()
    {
        ActiveGoalie = null;
        ActiveGoalieMatchPlayerId = null;
    }

    internal void SetTracksOnIcePlayers(bool tracksOnIcePlayers) => TracksOnIcePlayers = tracksOnIcePlayers;
}
