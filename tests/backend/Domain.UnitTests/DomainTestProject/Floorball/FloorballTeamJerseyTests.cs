using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;
using Domain.Enums.Floorball;

namespace DomainTestProject.Floorball;

public class FloorballTeamJerseyTests
{
    [Fact]
    public void UpdateTeamPlayer_InactivePlayerHoldsJersey_RejectsReuse()
    {
        FloorballTeam team = FloorballTestHelpers.CreateTeam();
        Guid competitionId = Guid.NewGuid();
        FloorballPlayer retired = FloorballTestHelpers.CreatePlayer();
        FloorballPlayer active = FloorballTestHelpers.CreatePlayer();
        team.AddPlayer(retired, FloorballPosition.Forward, 7, competitionId: competitionId);
        team.AddPlayer(active, FloorballPosition.Forward, 8, competitionId: competitionId);
        team.UpdateTeamPlayer(retired.Id, FloorballPosition.Forward, 7, isActive: false, competitionId);

        Action act = () => team.UpdateTeamPlayer(
            active.Id, FloorballPosition.Forward, 7, isActive: true, competitionId);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*7*already used*");
    }

    [Fact]
    public void UpdateTeamPlayer_WithoutJerseyNumber_TogglesActiveStatusOnly()
    {
        FloorballTeam team = FloorballTestHelpers.CreateTeam();
        Guid competitionId = Guid.NewGuid();
        FloorballPlayer player = FloorballTestHelpers.CreatePlayer();
        team.AddPlayer(player, FloorballPosition.Forward, jerseyNumber: null, competitionId: competitionId);

        team.UpdateTeamPlayer(player.Id, FloorballPosition.Forward, jerseyNumber: null, isActive: false, competitionId);

        FloorballTeamPlayer roster = team.Roster.Single();
        roster.IsActive.Should().BeFalse();
        roster.JerseyNumber.Should().BeNull();
        roster.Position.Should().Be(FloorballPosition.Forward);

        team.UpdateTeamPlayer(player.Id, FloorballPosition.Forward, jerseyNumber: null, isActive: true, competitionId);

        roster.IsActive.Should().BeTrue();
        roster.JerseyNumber.Should().BeNull();
        roster.Position.Should().Be(FloorballPosition.Forward);
    }
}
