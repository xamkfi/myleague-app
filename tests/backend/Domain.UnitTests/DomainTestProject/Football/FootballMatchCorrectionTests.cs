using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Enums.Common;
using Domain.Enums.Football;
using Domain.ValueObjects.Football;

namespace DomainTestProject.Football;

/// <summary>
/// Lineups without a size limit, missed penalties, clean sheets and correcting how a match was decided.
/// </summary>
public class FootballMatchCorrectionTests
{
    private static readonly FootballMatchRules ShootoutRules = new(
        numberOfHalves: 2,
        halfDurationMinutes: 20,
        playersOnField: 5,
        requireGoalkeeper: true,
        maxSubstitutions: 0,
        requireOfficialsToStart: false,
        allowExtraTime: true,
        extraTimeHalfCount: 2,
        extraTimeHalfDurationMinutes: 5,
        allowPenaltyShootout: true);

    [Fact]
    public void SetLineup_MorePlayersThanPlayersOnField_IsAllowedAndMatchStarts()
    {
        FootballTestHelpers.ReadyFootballMatch ready = FootballTestHelpers.CreateReadyMatch();
        FootballTestHelpers.SetStartingLineup(ready.Match, ready.Home, ready.HomePlayers, ready.HomePlayers.Count);

        ready.Match.Start();

        ready.Match.Status.Should().Be(FootballMatchStatus.InProgress);
        ready.Match.HomeOnFieldPlayers.Count.Should().BeGreaterThan(ready.Match.MatchRules.PlayersOnField);
    }

    [Fact]
    public void Start_LineupSmallerThanPlayersOnField_IsAllowed()
    {
        FootballTestHelpers.ReadyFootballMatch ready = FootballTestHelpers.CreateReadyMatch();
        FootballTestHelpers.SetStartingLineup(ready.Match, ready.Home, ready.HomePlayers.Take(3).ToList(), 3);

        ready.Match.Start();

        ready.Match.Status.Should().Be(FootballMatchStatus.InProgress);
    }

    [Fact]
    public void Start_LineupWithoutGoalkeeper_Throws()
    {
        FootballTestHelpers.ReadyFootballMatch ready = FootballTestHelpers.CreateReadyMatch();
        ready.Match.SetLineup(ready.Home.Id, ready.HomePlayers
            .Select(p => new FootballLineupSelection(p.Id, FootballPosition.Midfielder, true)));

        Action act = () => ready.Match.Start();

        act.Should().Throw<InvalidOperationException>().WithMessage("*goalkeeper*");
    }

    [Fact]
    public void RecordMissedPenalty_DoesNotChangeScore()
    {
        FootballTestHelpers.ReadyFootballMatch ready = FootballTestHelpers.CreateReadyMatch();
        ready.Match.Start();

        FootballMissedPenalty missed = ready.Match.RecordMissedPenalty(ready.Home, ready.HomePlayers[1], 1, 90);

        ready.Match.HomeScore.Should().Be(0);
        ready.Match.Events.Should().ContainSingle(e => e.Id == missed.Id);

        ready.Match.DeleteMissedPenaltyEvent(missed.Id);
        ready.Match.Events.Should().BeEmpty();
    }

    [Fact]
    public void SetLineup_DroppingMissedPenaltyShooter_Throws()
    {
        FootballTestHelpers.ReadyFootballMatch ready = FootballTestHelpers.CreateReadyMatch();
        ready.Match.Start();
        ready.Match.RecordMissedPenalty(ready.Home, ready.HomePlayers[1], 1, 90);

        Action act = () => ready.Match.SetLineup(ready.Home.Id, []);

        act.Should().Throw<InvalidOperationException>().WithMessage("*recorded events*");
    }

    [Fact]
    public void HasCleanSheet_ShootoutGoals_DoNotCount()
    {
        FootballTestHelpers.ReadyFootballMatch ready = FootballTestHelpers.CreateReadyMatch(ShootoutRules);
        FootballMatch match = ready.Match;
        match.Start();
        RecordGoal(ready, ready.Away, 60);
        match.RecordPenaltyShootout();
        RecordGoal(ready, ready.Home, 2400, match.MatchRules.PenaltyShootoutPeriodNumber);

        match.HasCleanSheet(ready.Away.Id).Should().BeTrue();
        match.HasCleanSheet(ready.Home.Id).Should().BeFalse();
    }

    [Fact]
    public void SetDecidedIn_Shootout_MovesLastGoal()
    {
        FootballTestHelpers.ReadyFootballMatch ready = FootballTestHelpers.CreateReadyMatch(ShootoutRules);
        FootballMatch match = ready.Match;
        match.Start();
        RecordGoal(ready, ready.Home, 60);
        RecordGoal(ready, ready.Away, 120);
        FootballGoal winner = RecordGoal(ready, ready.Away, 2400, 2);

        match.SetDecidedIn(MatchDecision.Shootout);

        winner.PeriodNumber.Should().Be(match.MatchRules.PenaltyShootoutPeriodNumber);
        winner.GoalType.Should().Be(FootballGoalType.PenaltyShootout);
        match.WentToPenaltyShootout.Should().BeTrue();
        (match.HomeScore, match.AwayScore).Should().Be((1, 2));
    }

    [Fact]
    public void SetDecidedIn_Overtime_MovesLastGoalToExtraTime()
    {
        FootballTestHelpers.ReadyFootballMatch ready = FootballTestHelpers.CreateReadyMatch(ShootoutRules);
        FootballMatch match = ready.Match;
        match.Start();
        FootballGoal winner = RecordGoal(ready, ready.Home, 2400, 2);

        match.SetDecidedIn(MatchDecision.Overtime);

        winner.PeriodNumber.Should().Be(match.MatchRules.ExtraTimeStartPeriodNumber);
        match.WentToExtraTime.Should().BeTrue();
        match.WentToPenaltyShootout.Should().BeFalse();
    }

    [Fact]
    public void SetDecidedIn_ShootoutNotInRules_Throws()
    {
        FootballTestHelpers.ReadyFootballMatch ready = FootballTestHelpers.CreateReadyMatch();
        ready.Match.Start();

        Action act = () => ready.Match.SetDecidedIn(MatchDecision.Shootout);

        act.Should().Throw<InvalidOperationException>().WithMessage("*not allowed*");
    }

    [Fact]
    public void SetDecidedIn_Regulation_ClearsEmptyExtraPeriods()
    {
        FootballTestHelpers.ReadyFootballMatch ready = FootballTestHelpers.CreateReadyMatch(ShootoutRules);
        FootballMatch match = ready.Match;
        match.Start();
        match.RecordExtraTime();
        match.RecordPenaltyShootout();

        match.SetDecidedIn(MatchDecision.Regulation);

        match.WentToExtraTime.Should().BeFalse();
        match.WentToPenaltyShootout.Should().BeFalse();
        match.PeriodScores.Should().HaveCount(2);
    }

    private static FootballGoal RecordGoal(
        FootballTestHelpers.ReadyFootballMatch ready,
        FootballTeam team,
        int timeInSeconds,
        int periodNumber = 1)
    {
        FootballPlayer scorer = team.Id == ready.Home.Id ? ready.HomePlayers[1] : ready.AwayPlayers[1];
        return ready.Match.RecordGoal(team, scorer, null, periodNumber, timeInSeconds);
    }
}
