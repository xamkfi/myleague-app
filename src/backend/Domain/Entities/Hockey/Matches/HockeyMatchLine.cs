using Domain.Enums.Hockey.Teams;

namespace Domain.Entities.Hockey.Matches;

/// <summary>
/// Match-specific line or deployment unit for one <see cref="HockeyMatchTeam"/>.
/// Players are always <see cref="HockeyMatchActivePlayer"/> entries from the match roster.
/// </summary>
public class HockeyMatchLine : BaseEntity
{
    public Guid MatchTeamId { get; private set; }
    public HockeyMatchTeam MatchTeam { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;
    public int? LineNumber { get; private set; }
    public HockeyLineType LineType { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsLocked { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyCollection<HockeyMatchLinePlayer> Players => _players.AsReadOnly();
    private readonly List<HockeyMatchLinePlayer> _players = new();

    private HockeyMatchLine() { }

    internal HockeyMatchLine(
        Guid matchTeamId,
        string name,
        HockeyLineType lineType,
        int? lineNumber = null,
        string? notes = null)
    {
        if (matchTeamId == Guid.Empty)
            throw new ArgumentException("Match team id cannot be empty.", nameof(matchTeamId));
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Line name cannot be null or empty.", nameof(name));
        if (lineNumber is < 0)
            throw new ArgumentOutOfRangeException(nameof(lineNumber), "Line number cannot be negative.");

        MatchTeamId = matchTeamId;
        Name = name;
        LineType = lineType;
        LineNumber = lineNumber;
        Notes = notes;
        IsActive = true;
        IsLocked = false;
    }

    public HockeyMatchLinePlayer AddPlayer(HockeyMatchActivePlayer activePlayer, HockeyLineSlot? slot = null, int? order = null)
    {
        EnsureEditable();
        ArgumentNullException.ThrowIfNull(activePlayer);
        if (!activePlayer.IsActive)
            throw new InvalidOperationException("Cannot add an inactive match player to a line.");
        if (MatchTeam?.PlayerSelection is null || !MatchTeam.PlayerSelection.HasActivePlayer(activePlayer.Id))
            throw new InvalidOperationException("Match active player must belong to this match team's active roster.");

        HockeyMatchLinePlayer? existing = _players.FirstOrDefault(p => p.MatchActivePlayerId == activePlayer.Id);
        if (existing is not null)
        {
            existing.UpdateSlot(slot);
            existing.UpdateOrder(order);
            return existing;
        }

        HockeyMatchLinePlayer linePlayer = new(Id, activePlayer.Id, slot, order);
        _players.Add(linePlayer);
        return linePlayer;
    }

    public void RemovePlayer(Guid matchActivePlayerId)
    {
        EnsureEditable();
        HockeyMatchLinePlayer? existing = _players.FirstOrDefault(p => p.MatchActivePlayerId == matchActivePlayerId)
            ?? throw new InvalidOperationException("Match active player is not part of this line.");
        _players.Remove(existing);
    }

    public void UpdateName(string name)
    {
        EnsureEditable();
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Line name cannot be null or empty.", nameof(name));
        Name = name;
    }

    public void UpdateNotes(string? notes)
    {
        EnsureEditable();
        Notes = notes;
    }

    public void Lock() => IsLocked = true;

    public void Unlock() => IsLocked = false;

    public void Deactivate() => IsActive = false;

    internal void AttachMatchTeam(HockeyMatchTeam matchTeam)
    {
        ArgumentNullException.ThrowIfNull(matchTeam);
        MatchTeam = matchTeam;
        MatchTeamId = matchTeam.Id;
    }

    private void EnsureEditable()
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot modify an inactive match line.");
        if (IsLocked)
            throw new InvalidOperationException("Cannot modify a locked match line.");
    }
}
