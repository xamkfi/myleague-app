using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;
using Domain.Enums.Common;
using Domain.Enums.Floorball;

namespace DomainTestProject.Floorball;

/// <summary>
/// Shots counted from events, goalie attribution, missed penalties and correcting how a match was decided.
/// </summary>
public class FloorballMatchCorrectionTests
{
    [Fact]
    public void CalculateTeamShots_SavesMovedToAnotherGoalie_CountsEachSaveOnce()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        FloorballMatch match = ready.Match;
        FloorballPlayer secondGoalie = ready.AwayPlayers[1];
        match.Start();
        RecordGoal(ready, ready.Home, timeInSeconds: 60);
        List<FloorballSave> wrongSaves = Enumerable.Range(0, 3)
            .Select(_ => match.RecordSave(ready.Away, ready.AwayGoalie, 1, 100))
            .ToList();
        match.RecordSave(ready.Home, ready.HomeGoalie, 1, 120);

        foreach (FloorballSave save in wrongSaves)
        {
            match.DeleteSaveEvent(save.Id);
            match.RecordSave(ready.Away, secondGoalie, 1, 100);
        }

        match.CalculateTeamShots(ready.Home.Id).Should().Be(new FloorballTeamShots(OnGoal: 1, Total: 4));
        match.CalculateTeamShots(ready.Away.Id).Should().Be(new FloorballTeamShots(OnGoal: 0, Total: 1));
    }

    [Fact]
    public void RecordGoal_ChargesGoalieInGoalWhenScored()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        FloorballMatch match = ready.Match;
        FloorballPlayer secondGoalie = ready.AwayPlayers[1];
        match.Start();

        FloorballGoal first = RecordGoal(ready, ready.Home, timeInSeconds: 60);
        match.SetActiveGoalie(ready.Away.Id, secondGoalie.Id);
        FloorballGoal second = RecordGoal(ready, ready.Home, timeInSeconds: 120);

        first.GoalieId.Should().Be(ready.AwayGoalie.Id);
        second.GoalieId.Should().Be(secondGoalie.Id);
        match.GetGoaliesInGoal(ready.Away.Id).Should().BeEquivalentTo([ready.AwayGoalie.Id, secondGoalie.Id]);
    }

    [Theory]
    [InlineData(FloorballGoalType.OwnGoal)]
    [InlineData(FloorballGoalType.EmptyNet)]
    [InlineData(FloorballGoalType.Shootout)]
    public void RecordGoal_TypeNotChargedToGoalie_LeavesGoalieEmpty(FloorballGoalType goalType)
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        ready.Match.Start();

        FloorballGoal goal = RecordGoal(ready, ready.Home, timeInSeconds: 60, goalType: goalType);

        goal.GoalieId.Should().BeNull();
    }

    [Fact]
    public void RecordGoal_DelayedPenalty_CombinesWithGoalType()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        ready.Match.Start();

        FloorballGoal goal = ready.Match.RecordGoal(
            ready.Home, ready.HomePlayers[1], null, null, 1, 60,
            goalType: (int)FloorballGoalType.EmptyNet, wasDelayedPenalty: true);

        goal.WasDelayedPenalty.Should().BeTrue();
        goal.GoalType.Should().Be(FloorballGoalType.EmptyNet);
    }

    [Fact]
    public void MoveGoalieEvents_MovesSavesAndConcededGoalsAndReportsWhatMoved()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        FloorballMatch match = ready.Match;
        FloorballPlayer secondGoalie = ready.AwayPlayers[1];
        match.Start();
        FloorballGoal goal = RecordGoal(ready, ready.Home, timeInSeconds: 60);
        match.RecordSave(ready.Away, ready.AwayGoalie, 1, 100);
        match.RecordSave(ready.Away, ready.AwayGoalie, 1, 110);
        FloorballSave homeSave = match.RecordSave(ready.Home, ready.HomeGoalie, 1, 120);

        IReadOnlyCollection<FloorballGoalieEventTransfer> transfers =
            match.MoveGoalieEvents(ready.Away.Id, secondGoalie.Id);

        transfers.Should().ContainSingle()
            .Which.Should().Be(new FloorballGoalieEventTransfer(ready.AwayGoalie.Id, Saves: 2, GoalsAgainst: 1));
        goal.GoalieId.Should().Be(secondGoalie.Id);
        match.SaveEvents.Where(s => s.TeamId == ready.Away.Id).Should().OnlyContain(s => s.GoalieId == secondGoalie.Id);
        homeSave.GoalieId.Should().Be(ready.HomeGoalie.Id);
    }

    [Fact]
    public void RecordMissedPenalty_DoesNotChangeScoreOrShots()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        FloorballMatch match = ready.Match;
        match.Start();

        FloorballMissedPenalty missed = match.RecordMissedPenalty(ready.Home, ready.HomePlayers[1], 1, 90);

        match.MissedPenaltyEvents.Should().ContainSingle(e => e.Id == missed.Id && e.ShooterPlayerId == ready.HomePlayers[1].Id);
        match.HomeScore.Should().Be(0);
        match.CalculateTeamShots(ready.Home.Id).Total.Should().Be(0);

        match.DeleteMissedPenaltyEvent(missed.Id);
        match.MissedPenaltyEvents.Should().BeEmpty();
    }

    [Fact]
    public void RecordMissedPenalty_ShooterNotOnRoster_Throws()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        ready.Match.Start();

        Action act = () => ready.Match.RecordMissedPenalty(ready.Home, ready.AwayPlayers[1], 1, 90);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetActiveRoster_DroppingMissedPenaltyShooter_Throws()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        FloorballMatch match = ready.Match;
        FloorballPlayer shooter = ready.HomePlayers[1];
        match.SetActiveRoster(ready.Home.Id, [new ActivePlayerSelection(shooter.Id, FloorballPosition.Forward)], null);
        match.Start();
        match.RecordMissedPenalty(ready.Home, shooter, 1, 90);

        Action act = () => match.SetActiveRoster(ready.Home.Id, [], null);

        act.Should().Throw<InvalidOperationException>().WithMessage("*recorded events*");
    }

    [Fact]
    public void SetDecidedIn_Shootout_MovesShootoutTypedGoalOutOfRegulation()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        FloorballMatch match = ready.Match;
        int lastPeriod = match.MatchRules.NumberOfPeriods;
        match.Start();
        RecordGoal(ready, ready.Home, timeInSeconds: 60);
        RecordGoal(ready, ready.Away, timeInSeconds: 120);
        FloorballGoal winner = RecordGoal(
            ready, ready.Away, timeInSeconds: 1800, periodNumber: lastPeriod, goalType: FloorballGoalType.Shootout);

        FloorballMovedGoal? moved = match.SetDecidedIn(MatchDecision.Shootout);

        moved!.Goal.Should().BeSameAs(winner);
        winner.PeriodNumber.Should().Be(match.ShootoutPeriodNumber);
        match.WentToShootout.Should().BeTrue();
        match.WentToOvertime.Should().BeFalse();
        (match.HomeScore, match.AwayScore).Should().Be((1, 2));
        match.PeriodScores.Single(ps => ps.PeriodNumber == match.ShootoutPeriodNumber).AwayScore.Should().Be(1);
        match.PeriodScores.Where(ps => ps.PeriodNumber == lastPeriod).Should().OnlyContain(ps => ps.AwayScore == 0);
    }

    [Fact]
    public void SetDecidedIn_Shootout_UntypedLastGoal_IsMovedAndNoLongerCharged()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        FloorballMatch match = ready.Match;
        match.Start();
        FloorballGoal winner = RecordGoal(ready, ready.Home, timeInSeconds: 1800);

        FloorballMovedGoal? moved = match.SetDecidedIn(MatchDecision.Shootout);

        moved!.PreviousGoalieId.Should().Be(ready.AwayGoalie.Id);
        winner.GoalieId.Should().BeNull();
        winner.GoalType.Should().Be(FloorballGoalType.Shootout);
    }

    [Fact]
    public void SetDecidedIn_LevelScore_OnlySetsFlag()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        ready.Match.Start();

        ready.Match.SetDecidedIn(MatchDecision.Overtime).Should().BeNull();

        ready.Match.WentToOvertime.Should().BeTrue();
    }

    [Fact]
    public void SetDecidedIn_TwoGoalLead_Throws()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        ready.Match.Start();
        RecordGoal(ready, ready.Home, timeInSeconds: 60);
        RecordGoal(ready, ready.Home, timeInSeconds: 120);

        Action act = () => ready.Match.SetDecidedIn(MatchDecision.Shootout);

        act.Should().Throw<InvalidOperationException>().WithMessage("*level*");
    }

    [Fact]
    public void SetDecidedIn_Regulation_ClearsEmptyExtraPeriods()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        FloorballMatch match = ready.Match;
        match.Start();
        match.RecordOvertime();
        match.RecordShootout();

        match.SetDecidedIn(MatchDecision.Regulation);

        match.WentToOvertime.Should().BeFalse();
        match.WentToShootout.Should().BeFalse();
        match.PeriodScores.Should().OnlyContain(ps => ps.PeriodNumber <= match.MatchRules.NumberOfPeriods);
    }

    [Fact]
    public void SetDecidedIn_Regulation_WithShootoutGoal_Throws()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        FloorballMatch match = ready.Match;
        match.Start();
        match.RecordShootout();
        RecordGoal(ready, ready.Home, timeInSeconds: 1800, periodNumber: match.ShootoutPeriodNumber);

        Action act = () => match.SetDecidedIn(MatchDecision.Regulation);

        act.Should().Throw<InvalidOperationException>().WithMessage("*later periods*");
    }

    [Fact]
    public void SetDecidedIn_CompletedMatch_Throws()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        ready.Match.Start();
        ready.Match.Complete();

        Action act = () => ready.Match.SetDecidedIn(MatchDecision.Shootout);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GoalsAgainstAverage_IsOwnGoalsPerGame_RegardlessOfMinutes()
    {
        FloorballGoalieSeasonStatistics stats = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        stats.RecordSaves(saves: 10, shotsAgainst: 13, goalsAllowed: 3);

        stats.RecordGamePlayed(wasStarter: true, FloorballGameResult.Win, minutesPlayed: 30);
        stats.GoalsAgainstAverage.Should().Be(3m);

        stats.RecordAppearance();
        stats.GoalsAgainstAverage.Should().Be(1.5m);

        stats.UndoAppearance();
        stats.GoalsAgainstAverage.Should().Be(3m);
    }

    private static FloorballGoal RecordGoal(
        FloorballTestHelpers.ReadyFloorballMatch ready,
        FloorballTeam team,
        int timeInSeconds,
        int periodNumber = 1,
        FloorballGoalType? goalType = null)
    {
        FloorballPlayer scorer = team.Id == ready.Home.Id ? ready.HomePlayers[1] : ready.AwayPlayers[1];
        return ready.Match.RecordGoal(
            team, scorer, null, null, periodNumber, timeInSeconds, goalType: (int?)goalType);
    }
}
