using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Teams;

namespace DomainTestProject.Hockey;

/// <summary>
/// Lineup edits keep existing match-player rows so recorded events stay valid.
/// </summary>
public class HockeyLineupSyncTests
{
    [Fact]
    public void SyncPlayerSelection_CreatesSelection_WhenNoneExists()
    {
        HockeyTeam homeTeam = HockeyTestHelpers.CreateTeam("Wolves");
        (HockeyMatch match, HockeyMatchTeam home, _) =
            HockeyTestHelpers.CreateMatchWithSides(homeTeam, HockeyTestHelpers.CreateTeam("Bears"));
        HockeyTeamPlayer skater = HockeyTestHelpers.AddRosterPlayer(homeTeam, jerseyNumber: 10);
        HockeyTeamPlayer goalie = HockeyTestHelpers.AddRosterPlayer(homeTeam, position: HockeyPosition.Goalie, jerseyNumber: 1);

        HockeyMatchPlayerSelection selection = match.SyncPlayerSelection(
            home.Id, [skater, goalie], HockeyPlayerSelectionSource.Manual);

        selection.ActivePlayers.Should().HaveCount(2);
        selection.ActivePlayers.Single(p => p.TeamPlayerId == goalie.Id).IsGoalie.Should().BeTrue();
    }

    [Fact]
    public void SyncPlayerSelection_KeepsExistingRows_AndDeactivatesRemovedPlayers()
    {
        HockeyTeam homeTeam = HockeyTestHelpers.CreateTeam("Wolves");
        (HockeyMatch match, HockeyMatchTeam home, _) =
            HockeyTestHelpers.CreateMatchWithSides(homeTeam, HockeyTestHelpers.CreateTeam("Bears"));
        HockeyTeamPlayer kept = HockeyTestHelpers.AddRosterPlayer(homeTeam, jerseyNumber: 10);
        HockeyTeamPlayer dropped = HockeyTestHelpers.AddRosterPlayer(homeTeam, jerseyNumber: 11);
        HockeyTeamPlayer added = HockeyTestHelpers.AddRosterPlayer(homeTeam, jerseyNumber: 12);
        HockeyMatchActivePlayer keptActive = HockeyTestHelpers.DressPlayer(home, kept, 10);
        HockeyMatchActivePlayer droppedActive = HockeyTestHelpers.DressPlayer(home, dropped, 11);

        match.SyncPlayerSelection(home.Id, [kept, added], HockeyPlayerSelectionSource.Manual);

        HockeyMatchPlayerSelection selection = home.PlayerSelection!;
        selection.FindActivePlayer(keptActive.Id).Should().NotBeNull();
        droppedActive.IsActive.Should().BeFalse();
        selection.ActivePlayers.Should().Contain(p => p.TeamPlayerId == added.Id && p.IsActive);
    }

    [Fact]
    public void SyncPlayerSelection_ReactivatesReturningPlayer()
    {
        HockeyTeam homeTeam = HockeyTestHelpers.CreateTeam("Wolves");
        (HockeyMatch match, HockeyMatchTeam home, _) =
            HockeyTestHelpers.CreateMatchWithSides(homeTeam, HockeyTestHelpers.CreateTeam("Bears"));
        HockeyTeamPlayer player = HockeyTestHelpers.AddRosterPlayer(homeTeam, jerseyNumber: 10);
        HockeyMatchActivePlayer active = HockeyTestHelpers.DressPlayer(home, player, 10);

        match.SyncPlayerSelection(home.Id, [], HockeyPlayerSelectionSource.Manual);
        match.SyncPlayerSelection(home.Id, [player], HockeyPlayerSelectionSource.Manual);

        active.IsActive.Should().BeTrue();
        home.PlayerSelection!.ActivePlayers.Should().ContainSingle();
    }

    [Fact]
    public void SyncPlayerSelection_RejectsRemovingPlayerWithEvents()
    {
        HockeyTeam homeTeam = HockeyTestHelpers.CreateTeam("Wolves");
        (HockeyMatch match, HockeyMatchTeam home, _) =
            HockeyTestHelpers.CreateMatchWithSides(homeTeam, HockeyTestHelpers.CreateTeam("Bears"));
        HockeyTeamPlayer scorer = HockeyTestHelpers.AddRosterPlayer(homeTeam, jerseyNumber: 19);
        HockeyMatchActivePlayer scorerActive = HockeyTestHelpers.DressPlayer(home, scorer, 19);
        match.AddEvent(new HockeyGoal(
            match.Id, home.Id, scorerActive.Id, 1, TimeSpan.FromMinutes(5), HockeyGoalStrength.EvenStrength));

        Action act = () => match.SyncPlayerSelection(home.Id, [], HockeyPlayerSelectionSource.Manual);

        act.Should().Throw<InvalidOperationException>().WithMessage("*#19*");
        scorerActive.IsActive.Should().BeTrue();
    }

    [Fact]
    public void SyncPlayerSelection_ClearsActiveGoalie_WhenGoalieRemoved()
    {
        HockeyTeam homeTeam = HockeyTestHelpers.CreateTeam("Wolves");
        (HockeyMatch match, HockeyMatchTeam home, _) =
            HockeyTestHelpers.CreateMatchWithSides(homeTeam, HockeyTestHelpers.CreateTeam("Bears"));
        HockeyTeamPlayer goalie = HockeyTestHelpers.AddRosterPlayer(homeTeam, position: HockeyPosition.Goalie, jerseyNumber: 1);
        HockeyMatchActivePlayer goalieActive = HockeyTestHelpers.DressPlayer(home, goalie, 1, isGoalie: true);
        home.SetActiveGoalie(goalieActive);

        match.SyncPlayerSelection(home.Id, [], HockeyPlayerSelectionSource.Manual);

        home.ActiveGoalieMatchPlayerId.Should().BeNull();
    }
}
