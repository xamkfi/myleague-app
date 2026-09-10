using Domain.Enums.Football;
using Domain.Enums.Hockey.Teams;
using JoomleagueImporter.Import;
using JoomleagueImporter.Models;

namespace JoomleagueImporter.UnitTests;

public class PlayerLatestTeamResolverTests
{
    [Fact]
    public void LatestOldTeamIdByPerson_UsesLatestProjectDate()
    {
        FloorballImportSet set = new();
        set.Projects.Add(ProjectWithPlayer(1, new DateTime(2018, 1, 1), teamId: 10, personId: 1316));
        set.Projects.Add(ProjectWithPlayer(2, new DateTime(2024, 1, 1), teamId: 20, personId: 1316));

        Dictionary<int, int> latest = PlayerLatestTeamResolver.LatestOldTeamIdByPerson(set);

        latest.Should().ContainKey(1316);
        latest[1316].Should().Be(20);
    }

    [Fact]
    public void LatestOldTeamIdByPerson_SameDate_UsesHigherProjectId()
    {
        FloorballImportSet set = new();
        DateTime date = new(2022, 5, 1);
        set.Projects.Add(ProjectWithPlayer(5, date, teamId: 10, personId: 7));
        set.Projects.Add(ProjectWithPlayer(9, date, teamId: 30, personId: 7));

        Dictionary<int, int> latest = PlayerLatestTeamResolver.LatestOldTeamIdByPerson(set);

        latest[7].Should().Be(30);
    }

    private static ProjectImport ProjectWithPlayer(int projectId, DateTime start, int teamId, int personId)
    {
        OldPerson person = new() { Id = personId, FirstName = "Tuomas", LastName = "Reijonen" };
        OldTeam team = new() { Id = teamId, Name = $"Team {teamId}" };
        OldProjectTeam projectTeam = new() { Id = projectId * 100, ProjectId = projectId, TeamId = teamId };
        ProjectTeamImport pti = new() { ProjectTeam = projectTeam, Team = team };
        pti.Roster.Add(new RosterEntry
        {
            TeamPlayer = new OldTeamPlayer { Id = projectId * 1000, PersonId = personId, ProjectTeamId = projectTeam.Id },
            Person = person,
            FootballPosition = FootballPosition.Forward,
            HockeyPosition = HockeyPosition.Center,
        });

        ProjectImport pi = new()
        {
            Project = new OldProject { Id = projectId, Name = $"Season {projectId}", StartDate = start },
        };
        pi.Teams[projectTeam.Id] = pti;
        return pi;
    }
}
