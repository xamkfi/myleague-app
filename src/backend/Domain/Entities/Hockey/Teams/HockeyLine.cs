using Domain.Enums.Hockey.Teams;

namespace Domain.Entities.Hockey.Teams;

/// <summary>
/// Default line or special unit configuration for a team. Optional <see cref="CompetitionId"/>
/// scopes lines to a specific competition.
/// </summary>
public class HockeyLine : BaseEntity
{
    public Guid TeamId { get; private set; }
    public HockeyTeam Team { get; private set; } = null!;
    public Guid? CompetitionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int LineNumber { get; private set; }
    public HockeyLineType LineType { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<HockeyLinePlayer> Players => _players.AsReadOnly();
    private readonly List<HockeyLinePlayer> _players = new();

    private HockeyLine() { }

    internal HockeyLine(
        Guid teamId,
        string name,
        int lineNumber,
        HockeyLineType lineType,
        Guid? competitionId = null)
    {
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team id cannot be empty.", nameof(teamId));
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Line name cannot be null or empty.", nameof(name));
        if (lineNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(lineNumber), "Line number cannot be negative.");

        TeamId = teamId;
        Name = name;
        LineNumber = lineNumber;
        LineType = lineType;
        CompetitionId = competitionId;
        IsActive = true;
    }

    internal void UpdateName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Line name cannot be null or empty.", nameof(name));
        Name = name;
    }

    internal void Deactivate() => IsActive = false;

    internal HockeyLinePlayer AddPlayer(Guid teamPlayerId, HockeyLineSlot slot, int order)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot add players to an inactive line.");
        if (teamPlayerId == Guid.Empty)
            throw new ArgumentException("Team player  id cannot be empty.", nameof(teamPlayerId));

        HockeyLinePlayer? existing = _players.FirstOrDefault(p => p.TeamPlayerId == teamPlayerId);
        if (existing is not null)
        {
            existing.UpdateSlot(slot);
            existing.UpdateOrder(order);
            return existing;
        }

        HockeyLinePlayer linePlayer = new(Id, teamPlayerId, slot, order);
        _players.Add(linePlayer);
        return linePlayer;
    }

    internal void RemovePlayer(Guid teamPlayerId)
    {
        HockeyLinePlayer? existing = _players.FirstOrDefault(p => p.TeamPlayerId == teamPlayerId)
            ?? throw new InvalidOperationException("Team player is not part of this line.");

        _players.Remove(existing);
    }
}
