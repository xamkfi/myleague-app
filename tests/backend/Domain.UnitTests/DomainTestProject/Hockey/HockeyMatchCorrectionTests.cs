using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Entities.Hockey.Statistics;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Competitions;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Statistics;
using Domain.Enums.Hockey.Teams;
using Domain.Services.Hockey;

namespace DomainTestProject.Hockey;

/// <summary>
/// Reopening, missed penalty shots, correcting how a match was decided, and goalie attribution.
/// </summary>
public class HockeyMatchCorrectionTests
{
    private readonly HockeyMatch _match;
    private readonly HockeyMatchTeam _home;
    private readonly HockeyMatchTeam _away;
    private readonly HockeyMatchActivePlayer _homeSkater;
    private readonly HockeyMatchActivePlayer _awaySkater;
    private readonly HockeyMatchActivePlayer _awayGoalie;
    private readonly HockeyMatchActivePlayer _awayBackup;

    public HockeyMatchCorrectionTests()
    {
        HockeyTeam homeTeam = HockeyTestHelpers.CreateTeam("Home");
        HockeyTeam awayTeam = HockeyTestHelpers.CreateTeam("Away");
        (_match, _home, _away) = HockeyTestHelpers.CreateMatchWithSides(homeTeam, awayTeam);
        _homeSkater = Dress(_home, homeTeam, HockeyPosition.Center, 9);
        _awaySkater = Dress(_away, awayTeam, HockeyPosition.Center, 19);
        _awayGoalie = Dress(_away, awayTeam, HockeyPosition.Goalie, 30);
        _awayBackup = Dress(_away, awayTeam, HockeyPosition.Goalie, 31);
        _match.MarkStarted();
    }

    [Fact]
    public void Reopen_ThenFinishWithAnotherWinner_InfersResultAgain()
    {
        AddGoal(_home, _homeSkater, 60);
        _match.MarkFinished();
        _match.ResultType.Should().Be(HockeyMatchResultType.HomeWin);

        _match.SetStatus(HockeyMatchStatus.InProgress);
        _match.ResultType.Should().BeNull();
        _match.ActualEndTime.Should().BeNull();
        _match.StandingResultType.Should().BeNull();

        AddGoal(_away, _awaySkater, 120);
        AddGoal(_away, _awaySkater, 180);
        _match.MarkFinished();

        _match.ResultType.Should().Be(HockeyMatchResultType.AwayWin);
    }

    [Fact]
    public void SetDecidedIn_Shootout_MovesLastGoalAndChangesResult()
    {
        AddGoal(_home, _homeSkater, 60);
        AddGoal(_away, _awaySkater, 120);
        HockeyGoal winner = AddGoal(_away, _awaySkater, 3600, 3);

        _match.SetDecidedIn(MatchDecision.Shootout);
        _match.MarkFinished();

        winner.PeriodNumber.Should().Be(_match.ShootoutPeriodNumber);
        _match.ResultType.Should().Be(HockeyMatchResultType.ShootoutAwayWin);
        _match.PeriodScores.Single(p => p.PeriodNumber == _match.ShootoutPeriodNumber).AwayGoals.Should().Be(1);
    }

    [Fact]
    public void SetDecidedIn_Regulation_RemovesExtraPeriodMarkers()
    {
        HockeyPeriodEvent marker = new(
            _match.Id, _match.OvertimePeriodNumber, TimeSpan.FromSeconds(3600), HockeyPeriodAction.OvertimeStarted);
        _match.AddEvent(marker);
        _match.AddPeriodScore(_match.OvertimePeriodNumber, HockeyPeriodType.Overtime);
        _match.SetWentToOvertime(true);

        IReadOnlyList<HockeyMatchEvent> removed = _match.SetDecidedIn(MatchDecision.Regulation);

        removed.Should().ContainSingle().Which.Should().BeSameAs(marker);
        _match.WentToOvertime.Should().BeFalse();
        _match.PeriodScores.Should().BeEmpty();
        _match.Events.Should().BeEmpty();
    }

    [Fact]
    public void SetDecidedIn_TwoGoalLead_Throws()
    {
        AddGoal(_home, _homeSkater, 60);
        AddGoal(_home, _homeSkater, 120);

        Action act = () => _match.SetDecidedIn(MatchDecision.Overtime);

        act.Should().Throw<InvalidOperationException>().WithMessage("*level*");
    }

    [Fact]
    public void SetDecidedIn_FinishedMatch_Throws()
    {
        _match.MarkFinished();

        Action act = () => _match.SetDecidedIn(MatchDecision.Overtime);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MissedPenaltyShot_IsValidEventAndDoesNotChangeScore()
    {
        HockeyMissedPenaltyShot missed = new(_match.Id, _home.Id, _homeSkater.Id, 2, TimeSpan.FromSeconds(1500));

        _match.AddEvent(missed);

        _match.HomeScore.Should().Be(0);
        _match.HasEventsForActivePlayer(_homeSkater.Id).Should().BeTrue();
        HockeyMatchValidationService.ValidateEventPlayerReferences(_match).IsValid.Should().BeTrue();
        _match.DeleteMissedPenaltyShotEvent(missed.Id).Should().BeSameAs(missed);
    }

    [Fact]
    public void BuildGoalieMatchStatistics_OnlyOwnGoals_AndDressedBackupDidNotPlay()
    {
        _away.SetActiveGoalie(_awayGoalie);
        AddGoal(_home, _homeSkater, 60, goalie: _awayGoalie);
        AddGoal(_home, _homeSkater, 120, goalie: _awayGoalie, wasEmptyNet: true);
        AddGoal(_home, _homeSkater, 3700, _match.ShootoutPeriodNumber, _awayGoalie);
        _match.SetWentToShootout(true);

        IReadOnlyList<HockeyGoalieMatchStatistics> stats =
            HockeyStatisticsCalculationService.BuildGoalieMatchStatistics(_match, _away).ToList();

        HockeyGoalieMatchStatistics starter = stats.Single(s => s.MatchActivePlayerId == _awayGoalie.Id);
        HockeyGoalieMatchStatistics backup = stats.Single(s => s.MatchActivePlayerId == _awayBackup.Id);
        (starter.GamesPlayed, starter.GoalsAgainst, starter.GoalsAgainstAverage).Should().Be((1, 1, 1m));
        backup.GamesPlayed.Should().Be(0);
    }

    private HockeyGoal AddGoal(
        HockeyMatchTeam team,
        HockeyMatchActivePlayer scorer,
        int seconds,
        int period = 1,
        HockeyMatchActivePlayer? goalie = null,
        bool wasEmptyNet = false)
    {
        HockeyGoal goal = new(
            _match.Id, team.Id, scorer.Id, period, TimeSpan.FromSeconds(seconds), HockeyGoalStrength.EvenStrength,
            goalieActivePlayerId: goalie?.Id, wasEmptyNet: wasEmptyNet);
        _match.AddEvent(goal);
        return goal;
    }

    private static HockeyMatchActivePlayer Dress(HockeyMatchTeam side, HockeyTeam team, HockeyPosition position, int jersey)
    {
        HockeyTeamPlayer teamPlayer = HockeyTestHelpers.AddRosterPlayer(team, position: position, jerseyNumber: jersey);
        return HockeyTestHelpers.DressPlayer(side, teamPlayer, jersey, isGoalie: position == HockeyPosition.Goalie);
    }
}
