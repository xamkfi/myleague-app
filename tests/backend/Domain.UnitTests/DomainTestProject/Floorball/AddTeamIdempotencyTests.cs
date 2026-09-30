using System.Reflection;
using Domain.Entities;
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Teams;

namespace DomainTestProject.Floorball;

public class AddTeamIdempotencyTests
{
    [Fact]
    public void AddTeam_SameIdDifferentInstance_KeepsASingleMembership()
    {
        FloorballSeason season = new(
            "2013",
            new DateTime(2013, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2014, 4, 1, 0, 0, 0, DateTimeKind.Utc));
        FloorballTeam enrolled = FloorballTestHelpers.CreateTeam("KalU");
        FloorballTeam reloaded = FloorballTestHelpers.CreateTeam("KalU");
        CopyId(enrolled, reloaded);

        season.AddTeam(enrolled);
        season.AddTeam(reloaded);

        season.Teams.Should().ContainSingle(team => team.Id == enrolled.Id);
    }

    private static void CopyId(BaseEntity source, BaseEntity target)
    {
        PropertyInfo id = typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!;
        id.SetValue(target, source.Id);
    }
}
