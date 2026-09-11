using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;

namespace Domain.Entities.Football.Statistics;

/// <summary>
/// Per-match team statistics for football.
/// </summary>
public class FootballMatchTeamStatistics : BaseEntity
{
    public Guid MatchId { get; private set; }
    public FootballMatch Match { get; private set; }
    public Guid TeamId { get; private set; }
    public FootballTeam Team { get; private set; }
    public int Goals { get; private set; }
    public int YellowCards { get; private set; }
    public int RedCards { get; private set; }
    public int Substitutions { get; private set; }
    public bool CleanSheet { get; private set; }

    private FootballMatchTeamStatistics()
    {
        Match = null!;
        Team = null!;
    }

    public FootballMatchTeamStatistics(Guid matchId, Guid teamId)
    {
        if (matchId == Guid.Empty)
            throw new ArgumentException("Match ID cannot be empty.", nameof(matchId));
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team ID cannot be empty.", nameof(teamId));

        MatchId = matchId;
        TeamId = teamId;
        Match = null!;
        Team = null!;
    }

    public void SetTotals(int goals, int yellowCards, int redCards, int substitutions, bool cleanSheet)
    {
        if (goals < 0 || yellowCards < 0 || redCards < 0 || substitutions < 0)
            throw new ArgumentException("Statistics values cannot be negative.");

        Goals = goals;
        YellowCards = yellowCards;
        RedCards = redCards;
        Substitutions = substitutions;
        CleanSheet = cleanSheet;
    }
}
