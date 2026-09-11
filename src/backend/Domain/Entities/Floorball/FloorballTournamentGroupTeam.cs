namespace Domain.Entities.Floorball;

/// <summary>
/// Join entity linking a team to a tournament group
/// </summary>
public class FloorballTournamentGroupTeam : BaseEntity
{
    /// <summary>
    /// Gets the tournament group ID
    /// </summary>
    public Guid TournamentGroupId { get; private set; }

    /// <summary>
    /// Gets the tournament group
    /// </summary>
    public FloorballTournamentGroup TournamentGroup { get; private set; }

    /// <summary>
    /// Gets the team ID
    /// </summary>
    public Guid TeamId { get; private set; }

    /// <summary>
    /// Gets the team
    /// </summary>
    public FloorballTeam Team { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballTournamentGroupTeam()
    {
        TournamentGroup = null!;
        Team = null!;
    }

    public FloorballTournamentGroupTeam(Guid tournamentGroupId, Guid teamId)
    {
        TournamentGroupId = tournamentGroupId;
        TeamId = teamId;
        TournamentGroup = null!;
        Team = null!;
    }
}
