namespace Domain.Entities.Football.Competitions;

/// <summary>
/// Links a football competition to a Common.Division.
/// </summary>
public class FootballCompetitionDivision : BaseEntity
{
    public Guid CompetitionId { get; private set; }
    public FootballCompetition Competition { get; private set; }
    public Guid DivisionId { get; private set; }
    public IReadOnlyCollection<FootballCompetitionDivisionTeam> Teams => _teams.AsReadOnly();
    private readonly List<FootballCompetitionDivisionTeam> _teams = new();

    private FootballCompetitionDivision()
    {
        Competition = null!;
    }

    public FootballCompetitionDivision(Guid competitionId, Guid divisionId)
    {
        CompetitionId = competitionId;
        DivisionId = divisionId;
        Competition = null!;
    }
}
