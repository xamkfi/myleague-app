using Application.Features.Common.Shared;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Common;
using Domain.Enums.Floorball;
using Domain.ValueObjects.Floorball;

namespace ApplicationTestProject.Features.Common;

public class RosterEnrollmentTests
{
    [Fact]
    public void Apply_CopyLatest_CopiesBaseRosterIntoCompetition()
    {
        FloorballTeam team = CreateTeam();
        FloorballPlayer player = CreatePlayer();
        team.AddPlayer(player, FloorballPosition.Forward, 12);
        Guid competitionId = Guid.NewGuid();

        int copied = RosterEnrollment.Apply(team, competitionId, RosterEnrollmentMode.CopyLatest);

        copied.Should().Be(1);
        team.GetActiveRoster(competitionId).Should().ContainSingle(row => row.PlayerId == player.Id);
    }

    [Fact]
    public void Apply_Empty_DoesNotCopyPlayers()
    {
        FloorballTeam team = CreateTeam();
        FloorballPlayer player = CreatePlayer();
        team.AddPlayer(player, FloorballPosition.Forward, 12);
        Guid competitionId = Guid.NewGuid();

        int copied = RosterEnrollment.Apply(team, competitionId, RosterEnrollmentMode.Empty);

        copied.Should().Be(0);
        team.GetActiveRoster(competitionId).Should().BeEmpty();
        team.GetActiveRoster(null).Should().ContainSingle();
    }

    private static FloorballTeam CreateTeam()
    {
        Club club = new("Roster Club");
        return new FloorballTeam(
            "Wolves",
            divisionId: null,
            club,
            homeArena: "Arena",
            primaryJerseyColor: "Blue",
            teamCategory: TeamCategory.Adult);
    }

    private static FloorballPlayer CreatePlayer()
    {
        Person person = new("Test", "Player");
        return new FloorballPlayer(person.Id, new Position(FloorballPosition.Forward, canPlayAsGoalkeeper: false));
    }
}
