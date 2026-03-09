namespace Domain.Entities.Floorball.Tournament;

/// <summary>
/// Links a floorball team to a tournament group.
/// TournamentId is denormalized for convenient querying.
/// </summary>
public class FloorballTournamentGroupTeam : BaseEntity
{
    /// <summary>
    /// Gets the group ID this membership belongs to
    /// </summary>
    public Guid GroupId { get; private set; }

    /// <summary>
    /// Gets the group this membership belongs to
    /// </summary>
    public FloorballTournamentGroup Group { get; private set; }

    /// <summary>
    /// Gets the team ID
    /// </summary>
    public Guid TeamId { get; private set; }

    /// <summary>
    /// Gets the team (from the main FloorballTeam table)
    /// </summary>
    public FloorballTeam Team { get; private set; }

    /// <summary>
    /// Gets the tournament ID (denormalized from Group.TournamentId for indexing)
    /// </summary>
    public Guid TournamentId { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballTournamentGroupTeam()
    {
        Group = null!;
        Team = null!;
    }

    /// <summary>
    /// Initializes a new instance linking a team to a tournament group
    /// </summary>
    public FloorballTournamentGroupTeam(Guid groupId, Guid teamId, Guid tournamentId)
    {
        Id = Guid.NewGuid();
        GroupId = groupId;
        TeamId = teamId;
        TournamentId = tournamentId;
        Group = null!;
        Team = null!;
    }
}
