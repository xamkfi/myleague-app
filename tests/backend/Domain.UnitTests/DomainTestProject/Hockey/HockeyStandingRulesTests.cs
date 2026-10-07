using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Statistics;
using Domain.Enums.Hockey.Statistics;
using Domain.ValueObjects.Hockey.Rules;
using FluentAssertions;

namespace DomainTestProject.Hockey;

public class HockeyStandingRulesTests
{
    [Fact]
    public void RecalculateStandingsMetrics_UsesEachResultTypePointValue()
    {
        HockeyStandingRules rules = new(
            regulationWinPoints: 4,
            overtimeWinPoints: 3,
            shootoutWinPoints: 2,
            overtimeLossPoints: 1,
            shootoutLossPoints: 0,
            tiePoints: 1);

        HockeyTeamCompetitionStatistics stats = new(Guid.NewGuid(), Guid.NewGuid(), HockeyStatisticsScope.Competition);
        stats.UpdateRecord(
            gamesPlayed: 6,
            regulationWins: 1,
            overtimeWins: 1,
            shootoutWins: 1,
            regulationLosses: 1,
            overtimeLosses: 1,
            shootoutLosses: 1,
            ties: 1,
            homeWins: 2,
            homeLosses: 1,
            awayWins: 1,
            awayLosses: 2);
        stats.RecalculateStandingsMetrics(rules);

        stats.Points.Should().Be(4 + 3 + 2 + 1 + 0 + 1);
    }

    [Fact]
    public void Default_MatchesTheThreeTwoOneTable()
    {
        HockeyStandingRules rules = HockeyStandingRules.Default();

        rules.RegulationWinPoints.Should().Be(3);
        rules.OvertimeWinPoints.Should().Be(2);
        rules.ShootoutWinPoints.Should().Be(2);
        rules.OvertimeLossPoints.Should().Be(1);
        rules.ShootoutLossPoints.Should().Be(1);
        rules.TiePoints.Should().Be(1);
    }

    [Fact]
    public void UpdateStandingRules_ReplacesOnlyStandingRules_AndKeepsOtherRuleParts()
    {
        HockeySeason season = HockeyTestHelpers.CreateSeason();
        HockeyCompetitionRules rulesBefore = season.CompetitionRules;
        HockeyMatchRules matchRulesBefore = rulesBefore.MatchRules;
        HockeyRosterRules rosterRulesBefore = rulesBefore.RosterRules;
        HockeyStandingRules updated = new(2, 2, 2, 1, 1, 1);

        season.UpdateStandingRules(updated);

        season.CompetitionRules.Should().BeSameAs(rulesBefore);
        season.CompetitionRules.StandingRules.Should().BeSameAs(updated);
        season.CompetitionRules.MatchRules.Should().BeSameAs(matchRulesBefore);
        season.CompetitionRules.RosterRules.Should().BeSameAs(rosterRulesBefore);
    }

    [Fact]
    public void UpdateStandingRules_WithEqualValues_KeepsCurrentInstance()
    {
        HockeySeason season = HockeyTestHelpers.CreateSeason();
        HockeyStandingRules current = season.CompetitionRules.StandingRules;

        season.UpdateStandingRules(new HockeyStandingRules(
            current.RegulationWinPoints,
            current.OvertimeWinPoints,
            current.ShootoutWinPoints,
            current.OvertimeLossPoints,
            current.ShootoutLossPoints,
            current.TiePoints,
            current.TieBreakers));

        season.CompetitionRules.StandingRules.Should().BeSameAs(current);
    }
}
