using Domain.Entities;

namespace Domain.Entities.Floorball;

/// <summary>
/// Links a floorball season to a specific division (from Common context via DivisionId)
/// </summary>
public class FloorballSeasonDivision : BaseEntity
{
    /// <summary>
    /// Gets the season ID this link belongs to
    /// </summary>
    public Guid SeasonId { get; private set; }

    /// <summary>
    /// Gets the season this link belongs to
    /// </summary>
    public FloorballSeason Season { get; private set; }

    /// <summary>
    /// Gets the division ID (Common.Division) that is part of the season
    /// Kept as FK only to avoid cross-context navigation
    /// </summary>
    public Guid DivisionId { get; private set; }

    /// <summary>
    /// Gets the collection of team memberships for this season-division
    /// </summary>
    public IReadOnlyCollection<FloorballSeasonDivisionTeam> Teams => _teams.AsReadOnly();
    private readonly List<FloorballSeasonDivisionTeam> _teams = new();

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballSeasonDivision()
    {
        Season = null!;
    }

    /// <summary>
    /// Initializes a new instance linking a season to a division
    /// </summary>
    /// <param name="seasonId">Season identifier</param>
    /// <param name="divisionId">Division identifier from Common context</param>
    public FloorballSeasonDivision(Guid seasonId, Guid divisionId)
    {
        SeasonId = seasonId;
        DivisionId = divisionId;
        Season = null!;
    }
}


