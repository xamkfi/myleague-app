namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a group within a tournament (e.g., "A-Lohko", "B-Lohko")
/// </summary>
public class FloorballTournamentGroup : BaseEntity
{
    /// <summary>
    /// Gets the tournament this group belongs to
    /// </summary>
    public Guid TournamentId { get; private set; }

    /// <summary>
    /// Gets the name of the group (e.g., "A-Lohko")
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the display order of this group within the tournament
    /// </summary>
    public int Order { get; private set; }

    /// <summary>
    /// Gets the teams in this group
    /// </summary>
    public IReadOnlyCollection<FloorballTournamentGroupTeam> Teams => _teams.AsReadOnly();
    private readonly List<FloorballTournamentGroupTeam> _teams = new();

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballTournamentGroup()
    {
        Name = string.Empty;
        _teams = new List<FloorballTournamentGroupTeam>();
    }

    public FloorballTournamentGroup(Guid tournamentId, string name, int order)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name cannot be empty.", nameof(name));
        if (order < 0)
            throw new ArgumentOutOfRangeException(nameof(order), "Order must be non-negative.");

        TournamentId = tournamentId;
        Name = name;
        Order = order;
        _teams = new List<FloorballTournamentGroupTeam>();
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name cannot be empty.", nameof(name));
        Name = name;
    }

    public void UpdateOrder(int order)
    {
        if (order < 0)
            throw new ArgumentOutOfRangeException(nameof(order), "Order must be non-negative.");
        Order = order;
    }

    public void AddTeam(FloorballTeam team)
    {
        ArgumentNullException.ThrowIfNull(team);
        if (_teams.Any(t => t.TeamId == team.Id))
            return;
        _teams.Add(new FloorballTournamentGroupTeam(Id, team.Id));
    }

    public void RemoveTeam(Guid teamId)
    {
        FloorballTournamentGroupTeam? existing = _teams.FirstOrDefault(t => t.TeamId == teamId);
        if (existing != null)
            _teams.Remove(existing);
    }
}
