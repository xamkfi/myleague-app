namespace Domain.Entities.Floorball;

/// <summary>
/// Links a floorball competition to a specific division (from Common context via DivisionId)
/// </summary>
public class FloorballCompetitionDivision : BaseEntity
{
    /// <summary>
    /// Gets the competition ID this link belongs to
    /// </summary>
    public Guid CompetitionId { get; private set; }

    /// <summary>
    /// Gets the competition this link belongs to
    /// </summary>
    public FloorballCompetition Competition { get; private set; }

    /// <summary>
    /// Gets the division ID (Common.Division) that is part of the competition.
    /// Kept as FK only to avoid cross-context navigation.
    /// </summary>
    public Guid DivisionId { get; private set; }

    /// <summary>
    /// Gets the collection of team memberships for this competition-division
    /// </summary>
    public IReadOnlyCollection<FloorballCompetitionDivisionTeam> Teams => _teams.AsReadOnly();
    private readonly List<FloorballCompetitionDivisionTeam> _teams = new();

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballCompetitionDivision()
    {
        Competition = null!;
    }

    /// <summary>
    /// Initializes a new instance linking a competition to a division
    /// </summary>
    /// <param name="competitionId">Competition identifier</param>
    /// <param name="divisionId">Division identifier from Common context</param>
    public FloorballCompetitionDivision(Guid competitionId, Guid divisionId)
    {
        CompetitionId = competitionId;
        DivisionId = divisionId;
        Competition = null!;
    }
}
