using Domain.Entities.Football.Teams;

namespace Domain.Entities.Football.Competitions;

/// <summary>
/// A group within a football tournament.
/// </summary>
public class FootballTournamentGroup : BaseEntity
{
    public Guid TournamentId { get; private set; }
    public string Name { get; private set; }
    public int Order { get; private set; }
    public IReadOnlyCollection<FootballTournamentGroupTeam> Teams => _teams.AsReadOnly();
    private readonly List<FootballTournamentGroupTeam> _teams = new();

    private FootballTournamentGroup()
    {
        Name = string.Empty;
    }

    public FootballTournamentGroup(Guid tournamentId, string name, int order)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name cannot be empty.", nameof(name));
        if (order < 0)
            throw new ArgumentOutOfRangeException(nameof(order), "Order must be non-negative.");

        TournamentId = tournamentId;
        Name = name;
        Order = order;
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

    public void AddTeam(FootballTeam team)
    {
        ArgumentNullException.ThrowIfNull(team);
        if (_teams.Any(t => t.TeamId == team.Id))
            return;
        _teams.Add(new FootballTournamentGroupTeam(Id, team.Id));
    }

    public void RemoveTeam(Guid teamId)
    {
        FootballTournamentGroupTeam? existing = _teams.FirstOrDefault(t => t.TeamId == teamId);
        if (existing != null)
            _teams.Remove(existing);
    }
}
