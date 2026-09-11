using Domain.Entities.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Enums.Common;
using Domain.Enums.Football;
using Domain.ValueObjects.Football;

namespace DomainTestProject.Football;

/// <summary>
/// Shared factories for football domain unit tests (real entities, no EF).
/// </summary>
internal static class FootballTestHelpers
{
    public static Club CreateClub(string name = "Test FC") => new(name);

    public static FootballMatchRules FiveASideRules(
        int maxSubstitutions = 0,
        bool requireOfficials = false,
        bool allowExtraTime = false,
        bool allowPenaltyShootout = false) =>
        new(
            numberOfHalves: 2,
            halfDurationMinutes: 20,
            playersOnField: 5,
            requireGoalkeeper: true,
            maxSubstitutions: maxSubstitutions,
            requireOfficialsToStart: requireOfficials,
            allowExtraTime: allowExtraTime,
            extraTimeHalfCount: 2,
            extraTimeHalfDurationMinutes: 5,
            allowPenaltyShootout: allowPenaltyShootout);

    public static FootballTeam CreateTeam(string name = "Wolves")
    {
        Club club = CreateClub(name + " Club");
        return new FootballTeam(
            name,
            divisionId: null,
            club,
            homeArena: "Test Pitch",
            primaryJerseyColor: "Blue",
            teamCategory: TeamCategory.Adult);
    }

    public static FootballPlayer CreatePlayer(FootballPosition position = FootballPosition.Midfielder)
    {
        Person person = new("Test", "Player");
        return new FootballPlayer(person.Id, new FootballPositionPreference(position));
    }

    public static FootballReferee CreateReferee()
    {
        Person person = new("Test", "Referee");
        DateTime issued = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime expires = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return new FootballReferee(person.Id, issued, expires);
    }

    public static FootballSeason CreateSeason(
        string name = "Hobby Season",
        FootballMatchRules? rules = null)
    {
        return new FootballSeason(
            name,
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            rules ?? FiveASideRules());
    }

    public static FootballTournament CreateTournament(string name = "Cup")
    {
        return new FootballTournament(
            name,
            new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 10, 0, 0, 0, DateTimeKind.Utc),
            venue: "Pitch");
    }

    public static FootballTeamPlayer AddRosterPlayer(
        FootballTeam team,
        FootballPlayer? player = null,
        FootballPosition position = FootballPosition.Midfielder,
        int? jerseyNumber = 10)
    {
        player ??= CreatePlayer(position);
        team.AddPlayer(player, position, jerseyNumber);
        return team.Roster.Single(r => r.PlayerId == player.Id);
    }

    public static (FootballTeam Team, List<FootballPlayer> Players) CreateTeamWithSquad(
        string name,
        int fieldPlayers)
    {
        FootballTeam team = CreateTeam(name);
        List<FootballPlayer> players = new();

        FootballPlayer goalkeeper = CreatePlayer(FootballPosition.Goalkeeper);
        team.AddPlayer(goalkeeper, FootballPosition.Goalkeeper, jerseyNumber: 1);
        players.Add(goalkeeper);

        for (int i = 0; i < fieldPlayers; i++)
        {
            FootballPlayer player = CreatePlayer(FootballPosition.Midfielder);
            team.AddPlayer(player, FootballPosition.Midfielder, jerseyNumber: i + 2);
            players.Add(player);
        }

        return (team, players);
    }

    public static void SetStartingLineup(
        FootballMatch match,
        FootballTeam team,
        IReadOnlyList<FootballPlayer> players,
        int onFieldCount)
    {
        List<FootballLineupSelection> selections = new();
        for (int i = 0; i < players.Count; i++)
        {
            FootballPosition position = i == 0 ? FootballPosition.Goalkeeper : FootballPosition.Midfielder;
            selections.Add(new FootballLineupSelection(players[i].Id, position, i < onFieldCount));
        }

        match.SetLineup(team.Id, selections);
    }

    public static FootballMatch CreateMatch(
        FootballCompetition competition,
        FootballTeam? home,
        FootballTeam? away,
        DateTime? kickoff = null)
    {
        return new FootballMatch(
            competition,
            home,
            away,
            kickoff ?? new DateTime(2027, 1, 15, 18, 30, 0, DateTimeKind.Utc),
            "Test Pitch");
    }

    public static ReadyFootballMatch CreateReadyMatch(
        FootballMatchRules? rules = null,
        bool assignLineups = true,
        bool assignTeams = true)
    {
        FootballMatchRules matchRules = rules ?? FiveASideRules();
        FootballSeason season = CreateSeason(rules: matchRules);
        (FootballTeam home, List<FootballPlayer> homePlayers) = CreateTeamWithSquad("Home", fieldPlayers: 6);
        (FootballTeam away, List<FootballPlayer> awayPlayers) = CreateTeamWithSquad("Away", fieldPlayers: 6);
        season.AddTeam(home);
        season.AddTeam(away);

        FootballMatch match = CreateMatch(
            season,
            assignTeams ? home : null,
            assignTeams ? away : null);

        if (assignTeams && assignLineups)
        {
            SetStartingLineup(match, home, homePlayers, matchRules.PlayersOnField);
            SetStartingLineup(match, away, awayPlayers, matchRules.PlayersOnField);
        }

        return new ReadyFootballMatch(match, season, home, away, homePlayers, awayPlayers);
    }

    public sealed record ReadyFootballMatch(
        FootballMatch Match,
        FootballSeason Season,
        FootballTeam Home,
        FootballTeam Away,
        List<FootballPlayer> HomePlayers,
        List<FootballPlayer> AwayPlayers);
}
