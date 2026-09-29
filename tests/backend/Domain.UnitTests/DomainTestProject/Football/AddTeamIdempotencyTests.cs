using System.Reflection;
using Domain.Entities;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Teams;

namespace DomainTestProject.Football;

public class AddTeamIdempotencyTests
{
    [Fact]
    public void AddTeam_SameIdDifferentInstance_KeepsASingleMembership()
    {
        FootballSeason season = new(
            "2013",
            new DateTime(2013, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2014, 4, 1, 0, 0, 0, DateTimeKind.Utc));
        FootballTeam enrolled = FootballTestHelpers.CreateTeam("Wilmert");
        FootballTeam reloaded = FootballTestHelpers.CreateTeam("Wilmert");
        PropertyInfo id = typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!;
        id.SetValue(reloaded, enrolled.Id);

        season.AddTeam(enrolled);
        season.AddTeam(reloaded);

        season.Teams.Should().ContainSingle(team => team.Id == enrolled.Id);
    }
}
