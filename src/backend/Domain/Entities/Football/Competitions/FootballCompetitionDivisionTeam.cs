using Domain.Entities.Football.Teams;

namespace Domain.Entities.Football.Competitions;

/// <summary>
/// Links a football team to a competition-division.
/// </summary>
public class FootballCompetitionDivisionTeam : BaseEntity
{
    public Guid CompetitionId { get; private set; }
    public Guid CompetitionDivisionId { get; private set; }
    public FootballCompetitionDivision CompetitionDivision { get; private set; }
    public Guid TeamId { get; private set; }
    public FootballTeam Team { get; private set; }

    private FootballCompetitionDivisionTeam()
    {
        CompetitionDivision = null!;
        Team = null!;
    }

    public FootballCompetitionDivisionTeam(Guid competitionDivisionId, Guid teamId, Guid competitionId)
    {
        CompetitionId = competitionId;
        CompetitionDivisionId = competitionDivisionId;
        TeamId = teamId;
        CompetitionDivision = null!;
        Team = null!;
    }
}
