using Domain.Entities.Floorball;
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
}
