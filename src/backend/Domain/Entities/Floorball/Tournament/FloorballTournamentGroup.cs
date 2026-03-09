using Domain.Enums.Floorball.Tournament;

namespace Domain.Entities.Floorball.Tournament;

/// <summary>
/// Represents a group (lohko) within a floorball tournament.
/// Groups can be in the initial group stage (e.g. A-lohko, B-lohko)
/// or in the playoff phase (final group).
/// </summary>
public class FloorballTournamentGroup : BaseEntity
{
    /// <summary>
    /// Gets the tournament ID this group belongs to
    /// </summary>
    public Guid TournamentId { get; private set; }

    /// <summary>
    /// Gets the tournament this group belongs to
    /// </summary>
    public FloorballTournament Tournament { get; private set; }

    /// <summary>
    /// Gets the name of the group (e.g. "A-lohko", "B-lohko", "Finaali-lohko")
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the phase this group belongs to (GroupStage or Playoff)
    /// </summary>
    public FloorballTournamentGroupPhase Phase { get; private set; }

    /// <summary>
    /// Gets the sort order for display purposes
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Gets the team memberships in this group
    /// </summary>
    public IReadOnlyCollection<FloorballTournamentGroupTeam> Teams => _teams.AsReadOnly();
    private readonly List<FloorballTournamentGroupTeam> _teams = new();

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballTournamentGroup()
    {
        Name = string.Empty;
        Tournament = null!;
        _teams = new List<FloorballTournamentGroupTeam>();
    }

    /// <summary>
    /// Initializes a new instance linking a group to a tournament
    /// </summary>
    public FloorballTournamentGroup(Guid tournamentId, string name, FloorballTournamentGroupPhase phase, int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name cannot be empty.", nameof(name));

        Id = Guid.NewGuid();
        TournamentId = tournamentId;
        Name = name;
        Phase = phase;
        SortOrder = sortOrder;
        Tournament = null!;
        _teams = new List<FloorballTournamentGroupTeam>();
    }

    /// <summary>
    /// Updates the group's details
    /// </summary>
    public void UpdateDetails(string name, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name cannot be empty.", nameof(name));

        Name = name;
        SortOrder = sortOrder;
    }

    /// <summary>
    /// Adds a team to this group
    /// </summary>
    public FloorballTournamentGroupTeam AddTeam(Guid teamId)
    {
        if (_teams.Any(t => t.TeamId == teamId))
            throw new InvalidOperationException("Team is already in this group.");

        FloorballTournamentGroupTeam membership = new(Id, teamId, TournamentId);
        _teams.Add(membership);
        return membership;
    }

    /// <summary>
    /// Removes a team from this group
    /// </summary>
    public void RemoveTeam(Guid teamId)
    {
        FloorballTournamentGroupTeam? membership = _teams.FirstOrDefault(t => t.TeamId == teamId);
        if (membership == null)
            throw new ArgumentException($"Team with ID {teamId} is not in this group.", nameof(teamId));

        _teams.Remove(membership);
    }
}
