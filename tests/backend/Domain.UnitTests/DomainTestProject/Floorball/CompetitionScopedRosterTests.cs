using Domain.Entities.Floorball;
using Domain.Enums.Floorball;

namespace DomainTestProject.Floorball;

public class CompetitionScopedRosterTests
{
    [Fact]
    public void CopyRosterToCompetition_FromBase_CopiesActivePlayers()
    {
        FloorballTeam team = FloorballTestHelpers.CreateTeam();
        FloorballPlayer player = FloorballTestHelpers.CreatePlayer();
        team.AddPlayer(player, FloorballPosition.Forward, 10);
        Guid seasonId = Guid.NewGuid();

        int copied = team.CopyRosterToCompetition(null, seasonId);

        copied.Should().Be(1);
        team.GetActiveRoster(seasonId).Should().ContainSingle(row =>
            row.PlayerId == player.Id && row.JerseyNumber == 10 && row.CompetitionId == seasonId);
        team.GetActiveRoster(null).Should().ContainSingle();
    }

    [Fact]
    public void CopyRosterToCompetition_DropsConflictingJerseyNumbers()
    {
        FloorballTeam team = FloorballTestHelpers.CreateTeam();
        FloorballPlayer sourcePlayer = FloorballTestHelpers.CreatePlayer();
        FloorballPlayer existingPlayer = FloorballTestHelpers.CreatePlayer();
        Guid seasonId = Guid.NewGuid();
        Guid cupId = Guid.NewGuid();

        team.AddPlayer(sourcePlayer, FloorballPosition.Forward, 7, null, seasonId);
        team.AddPlayer(existingPlayer, FloorballPosition.Forward, 7, null, cupId);

        int copied = team.CopyRosterToCompetition(seasonId, cupId);

        copied.Should().Be(1);
        FloorballTeamPlayer copiedRow = team.GetActiveRoster(cupId).Single(row => row.PlayerId == sourcePlayer.Id);
        copiedRow.JerseyNumber.Should().BeNull();
    }

    [Fact]
    public void AddPlayer_SamePlayerInTwoCompetitions_IsAllowed()
    {
        FloorballTeam team = FloorballTestHelpers.CreateTeam();
        FloorballPlayer player = FloorballTestHelpers.CreatePlayer();
        Guid seasonId = Guid.NewGuid();
        Guid cupId = Guid.NewGuid();

        team.AddPlayer(player, FloorballPosition.Forward, 8, null, seasonId);
        team.AddPlayer(player, FloorballPosition.Forward, 9, null, cupId);

        team.GetActiveRoster(seasonId).Should().ContainSingle();
        team.GetActiveRoster(cupId).Should().ContainSingle();
    }

    [Fact]
    public void IsPlayerOnRoster_FallsBackToBase_WhenCompetitionHasNoRows()
    {
        FloorballTeam team = FloorballTestHelpers.CreateTeam();
        FloorballPlayer player = FloorballTestHelpers.CreatePlayer();
        team.AddPlayer(player, FloorballPosition.Forward, 4);

        team.IsPlayerOnRoster(player.Id, Guid.NewGuid()).Should().BeTrue();
    }

    [Fact]
    public void IsPlayerOnRoster_RejectsBasePlayer_WhenCompetitionRosterExists()
    {
        FloorballTeam team = FloorballTestHelpers.CreateTeam();
        FloorballPlayer basePlayer = FloorballTestHelpers.CreatePlayer();
        FloorballPlayer seasonPlayer = FloorballTestHelpers.CreatePlayer();
        Guid seasonId = Guid.NewGuid();

        team.AddPlayer(basePlayer, FloorballPosition.Forward, 4);
        team.AddPlayer(seasonPlayer, FloorballPosition.Forward, 11, null, seasonId);

        team.IsPlayerOnRoster(basePlayer.Id, seasonId).Should().BeFalse();
        team.IsPlayerOnRoster(seasonPlayer.Id, seasonId).Should().BeTrue();
    }

    [Fact]
    public void SetActiveGoalie_RejectsPlayerMissingFromCompetitionRoster()
    {
        FloorballSeason season = FloorballTestHelpers.CreateSeason();
        FloorballTeam home = FloorballTestHelpers.CreateTeam("Home");
        FloorballTeam away = FloorballTestHelpers.CreateTeam("Away");
        FloorballPlayer homeBaseGoalie = FloorballTestHelpers.CreatePlayer(FloorballPosition.Goalkeeper);
        FloorballPlayer homeSeasonForward = FloorballTestHelpers.CreatePlayer(FloorballPosition.Forward);
        FloorballPlayer awayGoalie = FloorballTestHelpers.CreatePlayer(FloorballPosition.Goalkeeper);

        home.AddPlayer(homeBaseGoalie, FloorballPosition.Goalkeeper, 1);
        home.AddPlayer(homeSeasonForward, FloorballPosition.Forward, 9, null, season.Id);
        away.AddPlayer(awayGoalie, FloorballPosition.Goalkeeper, 1, null, season.Id);

        season.AddTeam(home);
        season.AddTeam(away);
        FloorballMatch match = FloorballTestHelpers.CreateMatch(season, home, away);

        Action act = () => match.SetActiveGoalie(home.Id, homeBaseGoalie.Id);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*not on the team's roster*");
    }
}
