using Domain.Entities.Floorball;
using Domain.Enums.Floorball;

namespace DomainTestProject.Floorball;

/// <summary>
/// Domain behavior tests for floorball competitions, teams, and match lifecycle/events.
/// </summary>
public class FloorballDomainBehaviorTests
{
    [Fact]
    public void Season_And_Tournament_Inherit_FloorballCompetition()
    {
        FloorballSeason season = FloorballTestHelpers.CreateSeason();
        FloorballTournament tournament = FloorballTestHelpers.CreateTournament();

        season.Should().BeAssignableTo<FloorballCompetition>();
        tournament.Should().BeAssignableTo<FloorballCompetition>();
        season.IsActive.Should().BeFalse();
        tournament.TournamentStatus.Should().Be(FloorballTournamentStatus.Draft);
    }

    [Fact]
    public void CannotAddTeam_ToCompletedCompetition()
    {
        FloorballSeason season = FloorballTestHelpers.CreateSeason();
        season.Activate();
        season.Complete();

        Action act = () => season.AddTeam(FloorballTestHelpers.CreateTeam());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*completed*");
    }

    [Fact]
    public void CannotUpdateDetails_WhenCompleted()
    {
        FloorballSeason season = FloorballTestHelpers.CreateSeason();
        season.Complete();

        Action act = () => season.UpdateDetails(
            "New Name",
            season.StartDate,
            season.EndDate);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*completed*");
    }

    [Fact]
    public void Tournament_StartGroupStage_ActivatesCompetition()
    {
        FloorballTournament tournament = FloorballTestHelpers.CreateTournament();
        FloorballTeam team1 = FloorballTestHelpers.CreateTeam("Team1");
        FloorballTeam team2 = FloorballTestHelpers.CreateTeam("Team2");
        tournament.AddTeam(team1);
        tournament.AddTeam(team2);
        FloorballTournamentGroup group = tournament.AddGroup("Group A");
        group.AddTeam(team1);
        group.AddTeam(team2);

        tournament.StartGroupStage();

        tournament.TournamentStatus.Should().Be(FloorballTournamentStatus.GroupStage);
        tournament.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Tournament_AddGroup_And_Cancel()
    {
        FloorballTournament tournament = FloorballTestHelpers.CreateTournament();
        FloorballTournamentGroup group = tournament.AddGroup("Group A");

        tournament.Groups.Should().ContainSingle(g => g.Id == group.Id && g.Name == "Group A");

        tournament.CancelTournament();

        tournament.TournamentStatus.Should().Be(FloorballTournamentStatus.Cancelled);
        tournament.IsActive.Should().BeFalse();
    }

    [Fact]
    public void CannotAddSamePlayerTwiceToRoster()
    {
        FloorballTeam team = FloorballTestHelpers.CreateTeam();
        FloorballPlayer player = FloorballTestHelpers.CreatePlayer();

        team.AddPlayer(player, FloorballPosition.Forward, jerseyNumber: 10);
        Action act = () => team.AddPlayer(player, FloorballPosition.Defender, jerseyNumber: 11);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemovePlayer_RemovesFromRoster()
    {
        FloorballTeam team = FloorballTestHelpers.CreateTeam();
        FloorballPlayer player = FloorballTestHelpers.CreatePlayer();
        team.AddPlayer(player, FloorballPosition.Forward, jerseyNumber: 7);

        team.RemovePlayer(player.Id);

        team.Roster.Should().BeEmpty();
    }

    [Fact]
    public void Start_WithoutOfficials_Throws()
    {
        FloorballSeason season = FloorballTestHelpers.CreateSeason();
        (FloorballTeam home, _, FloorballPlayer homeGoalie) = FloorballTestHelpers.CreateTeamWithSquad("Home");
        (FloorballTeam away, _, FloorballPlayer awayGoalie) = FloorballTestHelpers.CreateTeamWithSquad("Away");
        season.AddTeam(home);
        season.AddTeam(away);
        FloorballMatch match = FloorballTestHelpers.CreateMatch(season, home, away);
        match.SetActiveGoalie(home.Id, homeGoalie.Id);
        match.SetActiveGoalie(away.Id, awayGoalie.Id);

        Action act = () => match.Start();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*officials*");
    }

    [Fact]
    public void Start_WithoutGoalies_Throws()
    {
        FloorballSeason season = FloorballTestHelpers.CreateSeason();
        (FloorballTeam home, _, _) = FloorballTestHelpers.CreateTeamWithSquad("Home");
        (FloorballTeam away, _, _) = FloorballTestHelpers.CreateTeamWithSquad("Away");
        season.AddTeam(home);
        season.AddTeam(away);
        FloorballMatch match = FloorballTestHelpers.CreateMatch(season, home, away);
        match.AddOfficial(FloorballTestHelpers.CreateReferee());

        Action act = () => match.Start();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*goalie*");
    }

    [Fact]
    public void ReadyMatch_CanStart_RecordGoalPenaltySave_AndComplete()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        FloorballMatch match = ready.Match;
        FloorballPlayer scorer = ready.HomePlayers.First(p => p.Id != ready.HomeGoalie.Id);

        match.Start();
        match.Status.Should().Be(FloorballMatchStatus.InProgress);

        FloorballGoal goal = match.RecordGoal(
            ready.Home,
            scorer,
            assistingPlayer: null,
            secondaryAssistingPlayer: null,
            periodNumber: 1,
            timeInSeconds: 120);
        match.HomeScore.Should().Be(1);
        match.AwayScore.Should().Be(0);
        goal.ScoringPlayerId.Should().Be(scorer.Id);

        FloorballPenalty penalty = match.RecordPenalty(
            ready.Away,
            ready.AwayPlayers.First(p => p.Id != ready.AwayGoalie.Id),
            FloorballPenaltyType.Minor,
            minutes: 2,
            periodNumber: 1,
            timeInSeconds: 200);
        match.Events.OfType<FloorballPenalty>().Should().ContainSingle(p => p.Id == penalty.Id);

        FloorballSave save = match.RecordSave(
            ready.Away,
            ready.AwayGoalie,
            periodNumber: 1,
            timeInSeconds: 250);
        match.Events.OfType<FloorballSave>().Should().ContainSingle(s => s.Id == save.Id);

        match.Complete();
        match.Status.Should().Be(FloorballMatchStatus.Completed);
    }

    [Fact]
    public void RecordGoal_WhenNotInProgress_Throws()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        FloorballPlayer scorer = ready.HomePlayers.First(p => p.Id != ready.HomeGoalie.Id);

        Action act = () => ready.Match.RecordGoal(
            ready.Home,
            scorer,
            null,
            null,
            periodNumber: 1,
            timeInSeconds: 10);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SetActiveRoster_RejectsGoalieAlsoAsFieldPlayer()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        FloorballPlayer field = ready.HomePlayers.First(p => p.Id != ready.HomeGoalie.Id);

        Action act = () => ready.Match.SetActiveRoster(
            ready.Home.Id,
            [new ActivePlayerSelection(ready.HomeGoalie.Id, FloorballPosition.Forward),
             new ActivePlayerSelection(field.Id, FloorballPosition.Forward)],
            goalieId: ready.HomeGoalie.Id);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*goalie*");
    }

    [Fact]
    public void Postpone_Then_Reschedule_AllowsStart()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();

        ready.Match.Postpone();
        ready.Match.Status.Should().Be(FloorballMatchStatus.Postponed);

        ready.Match.Reschedule(new DateTime(2027, 2, 1, 18, 0, 0, DateTimeKind.Utc));
        ready.Match.Status.Should().Be(FloorballMatchStatus.Scheduled);

        ready.Match.Start();
        ready.Match.Status.Should().Be(FloorballMatchStatus.InProgress);
    }

    [Fact]
    public void Cancel_Then_Reactivate_AllowsStart()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();

        ready.Match.Cancel();
        ready.Match.Reactivate();
        ready.Match.Status.Should().Be(FloorballMatchStatus.Scheduled);

        ready.Match.Start();
        ready.Match.Status.Should().Be(FloorballMatchStatus.InProgress);
    }

    [Fact]
    public void Cancel_PreventsRecordingEvents()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        ready.Match.Cancel();

        Action act = () => ready.Match.Start();

        act.Should().Throw<InvalidOperationException>();
        ready.Match.Status.Should().Be(FloorballMatchStatus.Cancelled);
    }
}
