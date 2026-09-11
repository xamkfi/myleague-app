using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Common;
using Domain.Enums.Floorball;
using Domain.ValueObjects.Floorball;

namespace DomainTestProject.Floorball;

/// <summary>
/// Shared factories for floorball domain unit tests (real entities, no EF).
/// </summary>
internal static class FloorballTestHelpers
{
    public static Club CreateClub(string name = "Test HC") => new(name);

    public static FloorballTeam CreateTeam(string name = "Wolves")
    {
        Club club = CreateClub(name + " Club");
        return new FloorballTeam(
            name,
            divisionId: null,
            club,
            homeArena: "Test Arena",
            primaryJerseyColor: "Blue",
            teamCategory: TeamCategory.Adult);
    }

    public static FloorballPlayer CreatePlayer(
        FloorballPosition position = FloorballPosition.Forward,
        bool canPlayAsGoalkeeper = false)
    {
        Person person = new("Test", "Player");
        return new FloorballPlayer(
            person.Id,
            new Position(position, canPlayAsGoalkeeper: canPlayAsGoalkeeper || position == FloorballPosition.Goalkeeper));
    }

    public static FloorballReferee CreateReferee()
    {
        Person person = new("Test", "Referee");
        DateTime issued = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime expires = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return new FloorballReferee(person.Id, issued, expires);
    }

    public static FloorballSeason CreateSeason(
        string name = "2026-2027",
        FloorballMatchRules? matchRules = null)
    {
        return new FloorballSeason(
            name,
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            matchRules);
    }

    public static FloorballTournament CreateTournament(string name = "Cup")
    {
        return new FloorballTournament(
            name,
            new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 10, 0, 0, 0, DateTimeKind.Utc),
            venue: "Arena");
    }

    public static FloorballTeamPlayer AddRosterPlayer(
        FloorballTeam team,
        FloorballPlayer? player = null,
        FloorballPosition position = FloorballPosition.Forward,
        int? jerseyNumber = 10)
    {
        player ??= CreatePlayer(position);
        team.AddPlayer(player, position, jerseyNumber);
        return team.Roster.Single(r => r.PlayerId == player.Id);
    }

    public static (FloorballTeam Team, List<FloorballPlayer> Players, FloorballPlayer Goalie) CreateTeamWithSquad(
        string name,
        int fieldPlayers = 4)
    {
        FloorballTeam team = CreateTeam(name);
        List<FloorballPlayer> players = new();

        FloorballPlayer goalie = CreatePlayer(FloorballPosition.Goalkeeper);
        team.AddPlayer(goalie, FloorballPosition.Goalkeeper, jerseyNumber: 1);
        players.Add(goalie);

        for (int i = 0; i < fieldPlayers; i++)
        {
            FloorballPlayer player = CreatePlayer(FloorballPosition.Forward);
            team.AddPlayer(player, FloorballPosition.Forward, jerseyNumber: i + 2);
            players.Add(player);
        }

        return (team, players, goalie);
    }

    public static FloorballMatch CreateMatch(
        FloorballCompetition competition,
        FloorballTeam? home,
        FloorballTeam? away,
        DateTime? kickoff = null)
    {
        return new FloorballMatch(
            competition,
            home,
            away,
            kickoff ?? new DateTime(2027, 1, 15, 18, 30, 0, DateTimeKind.Utc),
            "Test Arena");
    }

    /// <summary>
    /// Season with two squads, match created with teams, official, and both goalies set.
    /// Ready for <see cref="FloorballMatch.Start"/>.
    /// </summary>
    public static ReadyFloorballMatch CreateReadyMatch()
    {
        FloorballSeason season = CreateSeason();
        (FloorballTeam home, List<FloorballPlayer> homePlayers, FloorballPlayer homeGoalie) =
            CreateTeamWithSquad("Home");
        (FloorballTeam away, List<FloorballPlayer> awayPlayers, FloorballPlayer awayGoalie) =
            CreateTeamWithSquad("Away");

        season.AddTeam(home);
        season.AddTeam(away);

        FloorballMatch match = CreateMatch(season, home, away);
        match.AddOfficial(CreateReferee());
        match.SetActiveGoalie(home.Id, homeGoalie.Id);
        match.SetActiveGoalie(away.Id, awayGoalie.Id);

        return new ReadyFloorballMatch(match, season, home, away, homePlayers, awayPlayers, homeGoalie, awayGoalie);
    }

    public sealed record ReadyFloorballMatch(
        FloorballMatch Match,
        FloorballSeason Season,
        FloorballTeam Home,
        FloorballTeam Away,
        List<FloorballPlayer> HomePlayers,
        List<FloorballPlayer> AwayPlayers,
        FloorballPlayer HomeGoalie,
        FloorballPlayer AwayGoalie);
}
