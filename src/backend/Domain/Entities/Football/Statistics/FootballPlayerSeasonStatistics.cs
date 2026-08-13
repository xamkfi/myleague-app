using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Teams;

namespace Domain.Entities.Football.Statistics;

/// <summary>
/// Player statistics for a football competition.
/// </summary>
public class FootballPlayerSeasonStatistics : BaseEntity
{
    public Guid PlayerId { get; private set; }
    public FootballPlayer Player { get; private set; }
    public Guid TeamId { get; private set; }
    public FootballTeam Team { get; private set; }
    public Guid CompetitionId { get; private set; }
    public FootballCompetition Competition { get; private set; }
    public int GamesPlayed { get; private set; }
    public int Goals { get; private set; }
    public int Assists { get; private set; }
    public int Points { get; private set; }
    public int YellowCards { get; private set; }
    public int RedCards { get; private set; }

    private FootballPlayerSeasonStatistics()
    {
        Player = null!;
        Team = null!;
        Competition = null!;
    }

    public FootballPlayerSeasonStatistics(Guid playerId, Guid teamId, Guid competitionId)
    {
        if (playerId == Guid.Empty)
            throw new ArgumentException("Player ID cannot be empty.", nameof(playerId));
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team ID cannot be empty.", nameof(teamId));
        if (competitionId == Guid.Empty)
            throw new ArgumentException("Competition ID cannot be empty.", nameof(competitionId));

        PlayerId = playerId;
        TeamId = teamId;
        CompetitionId = competitionId;
        Player = null!;
        Team = null!;
        Competition = null!;
    }

    public void RecordGamePlayed() => GamesPlayed++;

    public void RecordGoal()
    {
        Goals++;
        RecalculatePoints();
    }

    public void RecordAssist()
    {
        Assists++;
        RecalculatePoints();
    }

    public void RecordYellowCard() => YellowCards++;
    public void RecordRedCard() => RedCards++;

    public void RemoveGoal()
    {
        if (Goals > 0)
            Goals--;
        RecalculatePoints();
    }

    public void RemoveAssist()
    {
        if (Assists > 0)
            Assists--;
        RecalculatePoints();
    }

    public void RemoveGamePlayed()
    {
        if (GamesPlayed > 0)
            GamesPlayed--;
    }

    public void RemoveYellowCard()
    {
        if (YellowCards > 0)
            YellowCards--;
    }

    public void RemoveRedCard()
    {
        if (RedCards > 0)
            RedCards--;
    }

    private void RecalculatePoints() => Points = Goals + Assists;
}
