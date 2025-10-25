using Domain.Entities;

namespace Domain.Entities.Floorball;

/// <summary>
/// Links a floorball team to a specific season-division
/// </summary>
public class FloorballSeasonDivisionTeam : BaseEntity
{
    /// <summary>
    /// Gets the season ID for convenience and indexing (should match SeasonDivision.SeasonId)
    /// </summary>
    public Guid SeasonId { get; private set; }

    /// <summary>
    /// Gets the season-division link ID this membership belongs to
    /// </summary>
    public Guid SeasonDivisionId { get; private set; }

    /// <summary>
    /// Gets the season-division link this membership belongs to
    /// </summary>
    public FloorballSeasonDivision SeasonDivision { get; private set; }

    /// <summary>
    /// Gets the team ID that participates in the season-division
    /// </summary>
    public Guid TeamId { get; private set; }

    /// <summary>
    /// Gets the team that participates in the season-division
    /// </summary>
    public FloorballTeam Team { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballSeasonDivisionTeam()
    {
        SeasonDivision = null!;
        Team = null!;
    }

    /// <summary>
    /// Initializes a new instance linking a team to a season-division
    /// </summary>
    /// <param name="seasonDivisionId">Season-division identifier</param>
    /// <param name="teamId">Team identifier</param>
    public FloorballSeasonDivisionTeam(Guid seasonDivisionId, Guid teamId, Guid seasonId)
    {
        SeasonId = seasonId;
        SeasonDivisionId = seasonDivisionId;
        TeamId = teamId;
        SeasonDivision = null!;
        Team = null!;
    }
}


