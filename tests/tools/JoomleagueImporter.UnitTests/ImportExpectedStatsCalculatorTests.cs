using Domain.Enums.Football;
using Domain.Enums.Hockey.Teams;
using JoomleagueImporter.Import;
using JoomleagueImporter.Models;

namespace JoomleagueImporter.UnitTests;

public class ImportExpectedStatsCalculatorTests
{
    [Fact]
    public void BuildSeason_WinAndTie_ComputesStandingsAndPlayerStats()
    {
        FloorballImportSet set = TwoTeamSet();
        ImportExpectedReport report = ImportExpectedStatsCalculator.Build("floorball", "fixture.sql", set);

        report.Seasons.Should().HaveCount(1);
        ExpectedSeasonStats season = report.Seasons[0];
        season.PlayedMatchCount.Should().Be(2);
        season.TotalGoals.Should().Be(7);

        ExpectedTeamStats home = season.Teams.Single(t => t.OldTeamId == 10);
        home.GamesPlayed.Should().Be(2);
        home.Wins.Should().Be(1);
        home.Ties.Should().Be(1);
        home.Losses.Should().Be(0);
        home.GoalsFor.Should().Be(5);
        home.GoalsAgainst.Should().Be(2);
        home.Points.Should().Be(4);

        ExpectedTeamStats away = season.Teams.Single(t => t.OldTeamId == 20);
        away.Wins.Should().Be(0);
        away.Ties.Should().Be(1);
        away.Losses.Should().Be(1);
        away.Points.Should().Be(1);

        ExpectedPlayerStats scorer = season.Players.Single(p => p.OldPersonId == 100);
        scorer.GamesPlayed.Should().Be(2);
        scorer.Goals.Should().Be(4);
        scorer.Assists.Should().Be(0);

        ExpectedPlayerStats assister = season.Players.Single(p => p.OldPersonId == 101);
        assister.Assists.Should().Be(1);
        assister.PenaltyMinutes.Should().Be(2);
    }

    [Fact]
    public void ApplyIdMap_FillsNewIds()
    {
        FloorballImportSet set = TwoTeamSet();
        ImportExpectedReport report = ImportExpectedStatsCalculator.Build("floorball", "fixture.sql", set);
        IdMapStore idMap = new();
        Guid seasonId = Guid.NewGuid();
        Guid teamId = Guid.NewGuid();
        Guid playerId = Guid.NewGuid();
        idMap.MapSeason(1, seasonId);
        idMap.MapTeam(10, teamId);
        idMap.MapPerson(100, new IdMapStore.PersonMapping { PersonId = Guid.NewGuid(), PlayerId = playerId });

        ImportExpectedStatsCalculator.ApplyIdMap(report, idMap);

        report.Seasons[0].NewSeasonId.Should().Be(seasonId);
        report.Seasons[0].Teams.Single(t => t.OldTeamId == 10).NewTeamId.Should().Be(teamId);
        report.Seasons[0].Players.Single(p => p.OldPersonId == 100).NewPlayerId.Should().Be(playerId);
    }

    private static FloorballImportSet TwoTeamSet()
    {
        OldPerson homeScorer = new() { Id = 100, FirstName = "Aaro", LastName = "Maalintekija" };
        OldPerson homeAssist = new() { Id = 101, FirstName = "Simo", LastName = "Syottaja" };
        OldPerson awayPlayer = new() { Id = 200, FirstName = "Ville", LastName = "Vieras" };
        OldTeam home = new() { Id = 10, Name = "Koti" };
        OldTeam away = new() { Id = 20, Name = "Vieras" };
        OldProjectTeam homePt = new() { Id = 1, ProjectId = 1, TeamId = 10 };
        OldProjectTeam awayPt = new() { Id = 2, ProjectId = 1, TeamId = 20 };

        ProjectTeamImport homeImport = new() { ProjectTeam = homePt, Team = home };
        homeImport.Roster.Add(Entry(homeScorer, 1001, homePt.Id));
        homeImport.Roster.Add(Entry(homeAssist, 1002, homePt.Id));
        ProjectTeamImport awayImport = new() { ProjectTeam = awayPt, Team = away };
        awayImport.Roster.Add(Entry(awayPlayer, 2001, awayPt.Id));

        ProjectImport project = new()
        {
            Project = new OldProject { Id = 1, Name = "Testikausi", StartDate = new DateTime(2020, 1, 1) },
        };
        project.Teams[1] = homeImport;
        project.Teams[2] = awayImport;

        project.Matches.Add(new MatchImport
        {
            Match = new OldMatch
            {
                Id = 501,
                ProjectTeam1Id = 1,
                ProjectTeam2Id = 2,
                Team1Result = 4,
                Team2Result = 1,
            },
            Events =
            [
                new() { MatchId = 501, ProjectTeamId = 1, TeamPlayerId = 1001, EventTypeId = JoomleagueDatabase.EventGoal, Count = 3 },
                new() { MatchId = 501, ProjectTeamId = 1, TeamPlayerId = 1002, EventTypeId = JoomleagueDatabase.EventAssist },
                new() { MatchId = 501, ProjectTeamId = 1, TeamPlayerId = 1002, EventTypeId = JoomleagueDatabase.EventPenalty, Count = 2 },
                new() { MatchId = 501, ProjectTeamId = 2, TeamPlayerId = 2001, EventTypeId = JoomleagueDatabase.EventGoal, Count = 1 },
            ],
            Players =
            [
                new() { MatchId = 501, TeamPlayerId = 1001 },
                new() { MatchId = 501, TeamPlayerId = 1002 },
                new() { MatchId = 501, TeamPlayerId = 2001 },
            ],
        });
        project.Matches.Add(new MatchImport
        {
            Match = new OldMatch
            {
                Id = 502,
                ProjectTeam1Id = 1,
                ProjectTeam2Id = 2,
                Team1Result = 1,
                Team2Result = 1,
            },
            Events =
            [
                new() { MatchId = 502, ProjectTeamId = 1, TeamPlayerId = 1001, EventTypeId = JoomleagueDatabase.EventGoal, Count = 1 },
                new() { MatchId = 502, ProjectTeamId = 2, TeamPlayerId = 2001, EventTypeId = JoomleagueDatabase.EventGoal, Count = 1 },
            ],
            Players =
            [
                new() { MatchId = 502, TeamPlayerId = 1001 },
                new() { MatchId = 502, TeamPlayerId = 2001 },
            ],
        });

        FloorballImportSet set = new();
        set.Projects.Add(project);
        set.UniqueTeams[10] = home;
        set.UniqueTeams[20] = away;
        set.UniquePersons[100] = homeScorer;
        set.UniquePersons[101] = homeAssist;
        set.UniquePersons[200] = awayPlayer;
        return set;
    }

    private static RosterEntry Entry(OldPerson person, int teamPlayerId, int projectTeamId) =>
        new()
        {
            TeamPlayer = new OldTeamPlayer
            {
                Id = teamPlayerId,
                PersonId = person.Id,
                ProjectTeamId = projectTeamId,
            },
            Person = person,
            FootballPosition = FootballPosition.Forward,
            HockeyPosition = HockeyPosition.Center,
        };
}
