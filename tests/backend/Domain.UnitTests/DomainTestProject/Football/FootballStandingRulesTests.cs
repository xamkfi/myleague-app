using Domain.Entities.Football.Statistics;
using Domain.Enums.Football;
using Domain.ValueObjects.Football;
using FluentAssertions;

namespace DomainTestProject.Football;

public class FootballStandingRulesTests
{
    [Fact]
    public void PointsFor_UsesOvertimeValuesWhenTheMatchWentBeyondRegulation()
    {
        FootballStandingRules rules = new(5, 2, 0, overtimeWinPoints: 4, overtimeLossPoints: 1);

        rules.PointsFor(FootballGameResult.Win).Should().Be(5);
        rules.PointsFor(FootballGameResult.Draw).Should().Be(2);
        rules.PointsFor(FootballGameResult.Loss).Should().Be(0);
        rules.PointsFor(FootballGameResult.Win, decidedAfterRegulation: true).Should().Be(4);
        rules.PointsFor(FootballGameResult.Loss, decidedAfterRegulation: true).Should().Be(1);
        rules.PointsFor(FootballGameResult.Draw, decidedAfterRegulation: true).Should().Be(2);
    }

    [Fact]
    public void Default_KeepsExtraTimeAsAFullWinAndAZeroLoss()
    {
        FootballStandingRules rules = FootballStandingRules.Default();

        rules.PointsFor(FootballGameResult.Win, decidedAfterRegulation: true).Should().Be(3);
        rules.PointsFor(FootballGameResult.Loss, decidedAfterRegulation: true).Should().Be(0);
    }

    [Fact]
    public void UpdateAfterMatch_AwardsOvertimePointsAndUndoRemovesThem()
    {
        FootballStandingRules rules = new(3, 1, 0, overtimeWinPoints: 2, overtimeLossPoints: 1);
        FootballTeamSeasonStatistics winner = new(Guid.NewGuid(), Guid.NewGuid());
        FootballTeamSeasonStatistics loser = new(Guid.NewGuid(), Guid.NewGuid());

        winner.UpdateAfterMatch(FootballGameResult.Win, isHomeGame: true, goalsFor: 2, goalsAgainst: 1, rules, decidedAfterRegulation: true);
        loser.UpdateAfterMatch(FootballGameResult.Loss, isHomeGame: false, goalsFor: 1, goalsAgainst: 2, rules, decidedAfterRegulation: true);

        winner.Points.Should().Be(2);
        loser.Points.Should().Be(1);

        winner.RevertAfterMatch(FootballGameResult.Win, isHomeGame: true, goalsFor: 2, goalsAgainst: 1, rules, decidedAfterRegulation: true);
        loser.RevertAfterMatch(FootballGameResult.Loss, isHomeGame: false, goalsFor: 1, goalsAgainst: 2, rules, decidedAfterRegulation: true);

        winner.Points.Should().Be(0);
        loser.Points.Should().Be(0);
        winner.GamesPlayed.Should().Be(0);
    }

    [Fact]
    public void Constructor_RejectsNegativePoints()
    {
        Action act = () => new FootballStandingRules(3, 1, 0, overtimeWinPoints: -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
