using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Teams;
using Domain.Enums.Football;
using Domain.ValueObjects.Football;

namespace Domain.Entities.Football.Statistics;

/// <summary>
/// Team statistics for a football competition. Points come from standing rules, not hardcoded values.
/// </summary>
public class FootballTeamSeasonStatistics : BaseEntity
{
    public Guid TeamId { get; private set; }
    public FootballTeam Team { get; private set; }
    public Guid CompetitionId { get; private set; }
    public FootballCompetition Competition { get; private set; }
    public int GamesPlayed { get; private set; }
    public int Wins { get; private set; }
    public int Losses { get; private set; }
    public int Draws { get; private set; }
    public int Points { get; private set; }
    public int GoalsFor { get; private set; }
    public int GoalsAgainst { get; private set; }
    public int GoalDifference { get; private set; }
    public int HomeWins { get; private set; }
    public int AwayWins { get; private set; }
    public int HomeLosses { get; private set; }
    public int AwayLosses { get; private set; }
    public int CleanSheets { get; private set; }
    public int YellowCards { get; private set; }
    public int RedCards { get; private set; }

    private FootballTeamSeasonStatistics()
    {
        Team = null!;
        Competition = null!;
    }

    public FootballTeamSeasonStatistics(Guid teamId, Guid competitionId)
    {
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team ID cannot be empty.", nameof(teamId));
        if (competitionId == Guid.Empty)
            throw new ArgumentException("Competition ID cannot be empty.", nameof(competitionId));

        TeamId = teamId;
        CompetitionId = competitionId;
        Team = null!;
        Competition = null!;
    }

    public void UpdateAfterMatch(
        FootballGameResult gameResult,
        bool isHomeGame,
        int goalsFor,
        int goalsAgainst,
        FootballStandingRules standingRules,
        int yellowCards = 0,
        int redCards = 0)
    {
        ArgumentNullException.ThrowIfNull(standingRules);
        if (goalsFor < 0 || goalsAgainst < 0)
            throw new ArgumentException("Goals cannot be negative.");

        GamesPlayed++;
        switch (gameResult)
        {
            case FootballGameResult.Win:
                Wins++;
                Points += standingRules.WinPoints;
                if (isHomeGame) HomeWins++; else AwayWins++;
                break;
            case FootballGameResult.Loss:
                Losses++;
                Points += standingRules.LossPoints;
                if (isHomeGame) HomeLosses++; else AwayLosses++;
                break;
            case FootballGameResult.Draw:
                Draws++;
                Points += standingRules.DrawPoints;
                break;
            default:
                throw new ArgumentException($"Invalid game result: {gameResult}", nameof(gameResult));
        }

        GoalsFor += goalsFor;
        GoalsAgainst += goalsAgainst;
        GoalDifference = GoalsFor - GoalsAgainst;
        if (goalsAgainst == 0)
            CleanSheets++;
        YellowCards += yellowCards;
        RedCards += redCards;
    }

    public void RevertAfterMatch(
        FootballGameResult gameResult,
        bool isHomeGame,
        int goalsFor,
        int goalsAgainst,
        FootballStandingRules standingRules,
        int yellowCards = 0,
        int redCards = 0)
    {
        ArgumentNullException.ThrowIfNull(standingRules);
        if (GamesPlayed <= 0)
            return;

        GamesPlayed--;
        switch (gameResult)
        {
            case FootballGameResult.Win:
                Wins = Math.Max(0, Wins - 1);
                Points = Math.Max(0, Points - standingRules.WinPoints);
                if (isHomeGame) HomeWins = Math.Max(0, HomeWins - 1); else AwayWins = Math.Max(0, AwayWins - 1);
                break;
            case FootballGameResult.Loss:
                Losses = Math.Max(0, Losses - 1);
                Points = Math.Max(0, Points - standingRules.LossPoints);
                if (isHomeGame) HomeLosses = Math.Max(0, HomeLosses - 1); else AwayLosses = Math.Max(0, AwayLosses - 1);
                break;
            case FootballGameResult.Draw:
                Draws = Math.Max(0, Draws - 1);
                Points = Math.Max(0, Points - standingRules.DrawPoints);
                break;
        }

        GoalsFor = Math.Max(0, GoalsFor - goalsFor);
        GoalsAgainst = Math.Max(0, GoalsAgainst - goalsAgainst);
        GoalDifference = GoalsFor - GoalsAgainst;
        if (goalsAgainst == 0)
            CleanSheets = Math.Max(0, CleanSheets - 1);
        YellowCards = Math.Max(0, YellowCards - yellowCards);
        RedCards = Math.Max(0, RedCards - redCards);
    }
}
