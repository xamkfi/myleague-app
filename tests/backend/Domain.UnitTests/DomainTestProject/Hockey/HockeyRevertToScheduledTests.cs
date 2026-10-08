using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Matches;

namespace DomainTestProject.Hockey;

public class HockeyRevertToScheduledTests
{
    private static (HockeyMatch Match, HockeyMatchTeam Home, HockeyTeam HomeTeam) StartMatch()
    {
        HockeyTeam homeTeam = HockeyTestHelpers.CreateTeam("Wolves");
        (HockeyMatch match, HockeyMatchTeam home, _) =
            HockeyTestHelpers.CreateMatchWithSides(homeTeam, HockeyTestHelpers.CreateTeam("Bears"));
        match.MarkStarted();
        match.AddPeriodScore(1, HockeyPeriodType.RegularPeriod);
        match.AddEvent(new HockeyPeriodEvent(match.Id, 1, TimeSpan.Zero, HockeyPeriodAction.PeriodStarted));
        return (match, home, homeTeam);
    }

    [Fact]
    public void RevertToScheduled_WithOnlyPeriodMarkers_ClearsStartState()
    {
        (HockeyMatch match, _, _) = StartMatch();

        IReadOnlyList<HockeyMatchEvent> removed = match.RevertToScheduled();

        removed.Should().ContainSingle().Which.Should().BeOfType<HockeyPeriodEvent>();
        match.Status.Should().Be(HockeyMatchStatus.Scheduled);
        match.Events.Should().BeEmpty();
        match.PeriodScores.Should().BeEmpty();
        match.ActualStartTime.Should().BeNull();
        match.CurrentPeriodNumber.Should().Be(0);
    }

    [Fact]
    public void RevertToScheduled_DuringIntermission_IsAllowed()
    {
        (HockeyMatch match, _, _) = StartMatch();
        match.SetStatus(HockeyMatchStatus.Intermission);

        match.RevertToScheduled();

        match.Status.Should().Be(HockeyMatchStatus.Scheduled);
    }

    [Fact]
    public void RevertToScheduled_WithGoal_Throws()
    {
        (HockeyMatch match, HockeyMatchTeam home, HockeyTeam homeTeam) = StartMatch();
        HockeyTeamPlayer scorer = HockeyTestHelpers.AddRosterPlayer(homeTeam, jerseyNumber: 19);
        HockeyMatchActivePlayer scorerActive = HockeyTestHelpers.DressPlayer(home, scorer, 19);
        match.AddEvent(new HockeyGoal(
            match.Id, home.Id, scorerActive.Id, 1, TimeSpan.FromMinutes(5), HockeyGoalStrength.EvenStrength));

        Action act = () => match.RevertToScheduled();

        act.Should().Throw<InvalidOperationException>();
        match.Status.Should().Be(HockeyMatchStatus.InProgress);
    }

    [Fact]
    public void RevertToScheduled_FinishedMatch_Throws()
    {
        (HockeyMatch match, _, _) = StartMatch();
        match.MarkFinished();

        Action act = () => match.RevertToScheduled();

        act.Should().Throw<InvalidOperationException>().WithMessage("*live*");
    }
}
