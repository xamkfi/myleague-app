using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Matches;

namespace DomainTestProject.Hockey;

/// <summary>
/// Period score rows follow the goal events.
/// </summary>
public class HockeyPeriodScoreTests
{
    private static HockeyGoal Goal(HockeyMatch match, HockeyMatchTeam side, HockeyMatchActivePlayer scorer, int period) =>
        new(match.Id, side.Id, scorer.Id, period, TimeSpan.FromMinutes(5), HockeyGoalStrength.EvenStrength);

    private static (HockeyMatch Match, HockeyMatchTeam Home, HockeyMatchTeam Away, HockeyMatchActivePlayer HomeScorer, HockeyMatchActivePlayer AwayScorer) Setup()
    {
        HockeyTeam homeTeam = HockeyTestHelpers.CreateTeam("Wolves");
        HockeyTeam awayTeam = HockeyTestHelpers.CreateTeam("Bears");
        (HockeyMatch match, HockeyMatchTeam home, HockeyMatchTeam away) =
            HockeyTestHelpers.CreateMatchWithSides(homeTeam, awayTeam);
        HockeyMatchActivePlayer homeScorer = HockeyTestHelpers.DressPlayer(
            home, HockeyTestHelpers.AddRosterPlayer(homeTeam, jerseyNumber: 19), 19);
        HockeyMatchActivePlayer awayScorer = HockeyTestHelpers.DressPlayer(
            away, HockeyTestHelpers.AddRosterPlayer(awayTeam, jerseyNumber: 7), 7);
        return (match, home, away, homeScorer, awayScorer);
    }

    [Fact]
    public void AddEvent_Goal_CountsIntoItsPeriodRow()
    {
        (HockeyMatch match, HockeyMatchTeam home, HockeyMatchTeam away, HockeyMatchActivePlayer homeScorer, HockeyMatchActivePlayer awayScorer) = Setup();
        match.AddPeriodScore(1, HockeyPeriodType.RegularPeriod);
        match.AddPeriodScore(2, HockeyPeriodType.RegularPeriod);

        match.AddEvent(Goal(match, home, homeScorer, 1));
        match.AddEvent(Goal(match, home, homeScorer, 1));
        match.AddEvent(Goal(match, away, awayScorer, 2));

        HockeyPeriodScore first = match.PeriodScores.Single(p => p.PeriodNumber == 1);
        HockeyPeriodScore second = match.PeriodScores.Single(p => p.PeriodNumber == 2);
        first.HomeGoals.Should().Be(2);
        first.AwayGoals.Should().Be(0);
        second.HomeGoals.Should().Be(0);
        second.AwayGoals.Should().Be(1);
    }

    [Fact]
    public void AddPeriodScore_AfterGoals_CountsExistingGoals()
    {
        (HockeyMatch match, HockeyMatchTeam home, _, HockeyMatchActivePlayer homeScorer, _) = Setup();
        match.AddEvent(Goal(match, home, homeScorer, 1));

        HockeyPeriodScore first = match.AddPeriodScore(1, HockeyPeriodType.RegularPeriod);

        first.HomeGoals.Should().Be(1);
    }

    [Fact]
    public void DeleteGoalEvent_RemovesGoalFromPeriodRow()
    {
        (HockeyMatch match, HockeyMatchTeam home, _, HockeyMatchActivePlayer homeScorer, _) = Setup();
        match.AddPeriodScore(1, HockeyPeriodType.RegularPeriod);
        HockeyGoal goal = Goal(match, home, homeScorer, 1);
        match.AddEvent(goal);

        match.DeleteGoalEvent(goal.Id);

        match.PeriodScores.Single().HomeGoals.Should().Be(0);
        home.Goals.Should().Be(0);
    }

    [Fact]
    public void UpdateGoalEvent_MovesGoalToOtherPeriodAndTeam()
    {
        (HockeyMatch match, HockeyMatchTeam home, HockeyMatchTeam away, HockeyMatchActivePlayer homeScorer, HockeyMatchActivePlayer awayScorer) = Setup();
        match.AddPeriodScore(1, HockeyPeriodType.RegularPeriod);
        match.AddPeriodScore(2, HockeyPeriodType.RegularPeriod);
        HockeyGoal goal = Goal(match, home, homeScorer, 1);
        match.AddEvent(goal);

        match.UpdateGoalEvent(goal.Id, away.Id, awayScorer.Id, 2, TimeSpan.FromMinutes(3), HockeyGoalStrength.EvenStrength);

        HockeyPeriodScore first = match.PeriodScores.Single(p => p.PeriodNumber == 1);
        HockeyPeriodScore second = match.PeriodScores.Single(p => p.PeriodNumber == 2);
        first.HomeGoals.Should().Be(0);
        second.AwayGoals.Should().Be(1);
        match.HomeScore.Should().Be(0);
        match.AwayScore.Should().Be(1);
    }
}
