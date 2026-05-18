namespace Domain.Entities.Floorball;

/// <summary>
/// Links a floorball team to a specific competition-division
/// </summary>
public class FloorballCompetitionDivisionTeam : BaseEntity
{
    /// <summary>
    /// Gets the competition ID for convenience and indexing
    /// </summary>
    public Guid CompetitionId { get; private set; }

    /// <summary>
    /// Gets the competition-division link ID this membership belongs to
    /// </summary>
    public Guid CompetitionDivisionId { get; private set; }

    /// <summary>
    /// Gets the competition-division link this membership belongs to
    /// </summary>
    public FloorballCompetitionDivision CompetitionDivision { get; private set; }

    /// <summary>
    /// Gets the team ID that participates in the competition-division
    /// </summary>
    public Guid TeamId { get; private set; }

    /// <summary>
    /// Gets the team that participates in the competition-division
    /// </summary>
    public FloorballTeam Team { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballCompetitionDivisionTeam()
    {
        CompetitionDivision = null!;
        Team = null!;
    }

    /// <summary>
    /// Initializes a new instance linking a team to a competition-division
    /// </summary>
    /// <param name="competitionDivisionId">Competition-division identifier</param>
    /// <param name="teamId">Team identifier</param>
    /// <param name="competitionId">Competition identifier for indexing</param>
    public FloorballCompetitionDivisionTeam(Guid competitionDivisionId, Guid teamId, Guid competitionId)
    {
        CompetitionId = competitionId;
        CompetitionDivisionId = competitionDivisionId;
        TeamId = teamId;
        CompetitionDivision = null!;
        Team = null!;
    }
}
