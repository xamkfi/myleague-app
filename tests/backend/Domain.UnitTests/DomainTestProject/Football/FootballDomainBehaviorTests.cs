using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Teams;
using Domain.Enums.Football;

namespace DomainTestProject.Football;

/// <summary>
/// Domain behavior tests for football competitions and teams.
/// Match rules/lifecycle are covered by <see cref="FootballMatchTests"/>.
/// </summary>
public class FootballDomainBehaviorTests
{
    [Fact]
    public void Season_And_Tournament_Inherit_FootballCompetition()
    {
        FootballSeason season = FootballTestHelpers.CreateSeason();
        FootballTournament tournament = FootballTestHelpers.CreateTournament();

        season.Should().BeAssignableTo<FootballCompetition>();
        tournament.Should().BeAssignableTo<FootballCompetition>();
        season.IsActive.Should().BeFalse();
        tournament.TournamentStatus.Should().Be(FootballTournamentStatus.Draft);
    }

    [Fact]
    public void CannotAddTeam_ToCompletedCompetition()
    {
        FootballSeason season = FootballTestHelpers.CreateSeason();
        season.Activate();
        season.Complete();

        Action act = () => season.AddTeam(FootballTestHelpers.CreateTeam());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*completed*");
    }

    [Fact]
    public void CannotUpdateDetails_WhenCompleted()
    {
        FootballSeason season = FootballTestHelpers.CreateSeason();
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
        FootballTournament tournament = FootballTestHelpers.CreateTournament();
        FootballTeam team1 = FootballTestHelpers.CreateTeam("Team1");
        FootballTeam team2 = FootballTestHelpers.CreateTeam("Team2");
        tournament.AddTeam(team1);
        tournament.AddTeam(team2);
        FootballTournamentGroup group = tournament.AddGroup("Group A");
        group.AddTeam(team1);
        group.AddTeam(team2);

        tournament.StartGroupStage();

        tournament.TournamentStatus.Should().Be(FootballTournamentStatus.GroupStage);
        tournament.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Tournament_AddGroup_And_Cancel()
    {
        FootballTournament tournament = FootballTestHelpers.CreateTournament();
        FootballTournamentGroup group = tournament.AddGroup("Group A");

        tournament.Groups.Should().ContainSingle(g => g.Id == group.Id && g.Name == "Group A");

        tournament.CancelTournament();

        tournament.TournamentStatus.Should().Be(FootballTournamentStatus.Cancelled);
        tournament.IsActive.Should().BeFalse();
    }

    [Fact]
    public void CannotAddSamePlayerTwiceToRoster()
    {
        FootballTeam team = FootballTestHelpers.CreateTeam();
        FootballPlayer player = FootballTestHelpers.CreatePlayer();

        team.AddPlayer(player, FootballPosition.Midfielder, jerseyNumber: 10);
        Action act = () => team.AddPlayer(player, FootballPosition.Forward, jerseyNumber: 11);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemovePlayer_RemovesFromRoster()
    {
        FootballTeam team = FootballTestHelpers.CreateTeam();
        FootballPlayer player = FootballTestHelpers.CreatePlayer();
        team.AddPlayer(player, FootballPosition.Midfielder, jerseyNumber: 7);

        team.RemovePlayer(player.Id);

        team.Roster.Should().BeEmpty();
    }

    [Fact]
    public void Activate_Then_Deactivate_Season()
    {
        FootballSeason season = FootballTestHelpers.CreateSeason();

        season.Activate();
        season.IsActive.Should().BeTrue();

        season.Deactivate();
        season.IsActive.Should().BeFalse();
        season.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void AddTeam_ToSeason_AppearsInTeams()
    {
        FootballSeason season = FootballTestHelpers.CreateSeason();
        FootballTeam team = FootballTestHelpers.CreateTeam("United");

        season.AddTeam(team);

        season.Teams.Should().ContainSingle(t => t.Id == team.Id);
    }

    [Fact]
    public void RemoveTeam_WithScheduledMatch_Throws()
    {
        FootballTestHelpers.ReadyFootballMatch ready = FootballTestHelpers.CreateReadyMatch(assignLineups: false);
        ready.Season.AddMatch(ready.Match);

        Action act = () => ready.Season.RemoveTeam(ready.Home);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*scheduled matches*");
    }
}
