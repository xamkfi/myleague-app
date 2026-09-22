using Domain.Entities.Football.Teams;
using Domain.Enums.Football;

namespace DomainTestProject.Football;

public class FootballTeamJerseyTests
{
    [Fact]
    public void UpdateTeamPlayer_InactivePlayerHoldsJersey_RejectsReuse()
    {
        FootballTeam team = FootballTestHelpers.CreateTeam();
        Guid competitionId = Guid.NewGuid();
        FootballPlayer retired = FootballTestHelpers.CreatePlayer();
        FootballPlayer active = FootballTestHelpers.CreatePlayer();
        team.AddPlayer(retired, FootballPosition.Midfielder, 7, competitionId: competitionId);
        team.AddPlayer(active, FootballPosition.Midfielder, 8, competitionId: competitionId);
        team.UpdateTeamPlayer(retired.Id, FootballPosition.Midfielder, 7, isActive: false, competitionId);

        Action act = () => team.UpdateTeamPlayer(
            active.Id, FootballPosition.Midfielder, 7, isActive: true, competitionId);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*7*already used*");
    }
}
