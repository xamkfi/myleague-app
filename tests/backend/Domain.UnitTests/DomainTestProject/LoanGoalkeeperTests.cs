using Domain.Entities.Common;
using Domain.Entities.Floorball.Teams;
using Domain.Entities.Football.Teams;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Floorball;
using Domain.Enums.Football;
using Domain.Enums.Hockey.Teams;
using Domain.ValueObjects.Floorball;
using Domain.ValueObjects.Football;

namespace DomainTestProject;

public class LoanGoalkeeperTests
{
    [Fact]
    public void MarkAsLoanGoalkeeper_FloorballPlayer_SetsFlag()
    {
        Person person = new("Test", "Player");
        FloorballPlayer player = new(person.Id, new Position(FloorballPosition.Goalkeeper));

        player.IsLoanGoalkeeper.Should().BeFalse();
        player.MarkAsLoanGoalkeeper();
        player.IsLoanGoalkeeper.Should().BeTrue();
    }

    [Fact]
    public void MarkAsLoanGoalkeeper_FootballPlayer_SetsFlag()
    {
        Person person = new("Test", "Player");
        FootballPlayer player = new(person.Id, new FootballPositionPreference(FootballPosition.Goalkeeper));

        player.IsLoanGoalkeeper.Should().BeFalse();
        player.MarkAsLoanGoalkeeper();
        player.IsLoanGoalkeeper.Should().BeTrue();
    }

    [Fact]
    public void MarkAsLoanGoalkeeper_HockeyPlayer_SetsFlag()
    {
        Person person = new("Test", "Player");
        HockeyPlayer player = new(person.Id, HockeyPosition.Goalie);

        player.IsLoanGoalkeeper.Should().BeFalse();
        player.MarkAsLoanGoalkeeper();
        player.IsLoanGoalkeeper.Should().BeTrue();
    }
}
