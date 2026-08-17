using Domain.Entities.Football.Teams;

namespace Domain.Entities.Football.Competitions;

/// <summary>
/// Join entity linking a team to a tournament group.
/// </summary>
public class FootballTournamentGroupTeam : BaseEntity
{
    public Guid TournamentGroupId { get; private set; }
    public FootballTournamentGroup TournamentGroup { get; private set; }
    public Guid TeamId { get; private set; }
    public FootballTeam Team { get; private set; }

    private FootballTournamentGroupTeam()
    {
        TournamentGroup = null!;
        Team = null!;
    }

    public FootballTournamentGroupTeam(Guid tournamentGroupId, Guid teamId)
    {
        TournamentGroupId = tournamentGroupId;
        TeamId = teamId;
        TournamentGroup = null!;
        Team = null!;
    }
}
