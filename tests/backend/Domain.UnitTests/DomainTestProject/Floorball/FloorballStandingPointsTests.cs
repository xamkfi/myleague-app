using Domain.Entities.Floorball.Statistics;
using Domain.Enums.Floorball;
using Domain.Services.Floorball;

namespace DomainTestProject.Floorball;

public class FloorballStandingPointsTests
{
    [Theory]
    [InlineData(3, 1, false, 3)]
    [InlineData(2, 1, true, 2)]
    [InlineData(1, 2, true, 1)]
    [InlineData(2, 2, false, 1)]
    [InlineData(1, 4, false, 0)]
    public void ForScore_AwardsRegulationShootoutAndDrawPoints(
        int goalsFor,
        int goalsAgainst,
        bool wentToShootout,
        int expectedPoints)
    {
        FloorballStandingPoints.ForScore(goalsFor, goalsAgainst, wentToShootout).Should().Be(expectedPoints);
    }

    [Fact]
    public void UpdateAfterMatch_ShootoutWinAndLoss_AwardTwoAndOnePoints()
    {
        FloorballTeamSeasonStatistics winner = new(Guid.NewGuid(), Guid.NewGuid());
        FloorballTeamSeasonStatistics loser = new(Guid.NewGuid(), Guid.NewGuid());

        winner.UpdateAfterMatch(FloorballGameResult.Win, isHomeGame: true, goalsFor: 0, goalsAgainst: 0, wentToShootout: true);
        loser.UpdateAfterMatch(FloorballGameResult.Loss, isHomeGame: false, goalsFor: 0, goalsAgainst: 0, wentToShootout: true);

        winner.Points.Should().Be(FloorballStandingPoints.ShootoutWinPoints);
        winner.Wins.Should().Be(1);
        loser.Points.Should().Be(FloorballStandingPoints.ShootoutLossPoints);
        loser.Losses.Should().Be(1);
    }

    [Fact]
    public void UndoMatchResult_Shootout_RemovesTheSamePoints()
    {
        FloorballTeamSeasonStatistics winner = new(Guid.NewGuid(), Guid.NewGuid());
        winner.UpdateAfterMatch(FloorballGameResult.Win, isHomeGame: true, goalsFor: 0, goalsAgainst: 0, wentToShootout: true);

        winner.UndoMatchResult(FloorballGameResult.Win, isHomeGame: true, wentToShootout: true);

        winner.Points.Should().Be(0);
        winner.Wins.Should().Be(0);
        winner.GamesPlayed.Should().Be(0);
    }

    [Fact]
    public void UpdateAfterMatch_RegulationWin_AwardsThreePoints()
    {
        FloorballTeamSeasonStatistics winner = new(Guid.NewGuid(), Guid.NewGuid());

        winner.UpdateAfterMatch(FloorballGameResult.Win, isHomeGame: false, goalsFor: 0, goalsAgainst: 0);

        winner.Points.Should().Be(FloorballStandingPoints.RegulationWinPoints);
    }

    [Fact]
    public void UpdateAfterMatch_Draw_AwardsOnePoint()
    {
        FloorballTeamSeasonStatistics team = new(Guid.NewGuid(), Guid.NewGuid());

        team.UpdateAfterMatch(FloorballGameResult.Tie, isHomeGame: true, goalsFor: 0, goalsAgainst: 0);

        team.Points.Should().Be(FloorballStandingPoints.TiePoints);
        team.Ties.Should().Be(1);
    }
}
