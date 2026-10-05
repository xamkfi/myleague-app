using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Teams;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Enums.Floorball;
using Domain.Enums.Football;
using DomainTestProject.Floorball;
using DomainTestProject.Football;

namespace DomainTestProject;

public class MatchRosterLicenceTests
{
    [Fact]
    public void FloorballIsPlayerOnRoster_UnpaidLicence_ReturnsTrue()
    {
        FloorballTeam team = FloorballTestHelpers.CreateTeam();
        FloorballPlayer player = FloorballTestHelpers.CreatePlayer();
        Guid seasonId = Guid.NewGuid();
        team.AddPlayer(player, FloorballPosition.Forward, 4, null, seasonId);
        team.UpdateTeamPlayer(player.Id, FloorballPosition.Forward, 4, isActive: false, seasonId);

        team.IsPlayerOnRoster(player.Id, seasonId).Should().BeTrue();
    }

    [Fact]
    public void FloorballSetActiveRoster_UnpaidLicence_AddsPlayer()
    {
        FloorballTestHelpers.ReadyFloorballMatch ready = FloorballTestHelpers.CreateReadyMatch();
        FloorballPlayer fieldPlayer = ready.HomePlayers.First(player => player.Id != ready.HomeGoalie.Id);
        ready.Home.UpdateTeamPlayer(fieldPlayer.Id, FloorballPosition.Forward, null, isActive: false);

        ready.Match.SetActiveRoster(
            ready.Home.Id,
            new[] { new ActivePlayerSelection(fieldPlayer.Id, FloorballPosition.Forward) },
            ready.HomeGoalie.Id);

        ready.Match.ActivePlayers.Should().ContainSingle(row => row.PlayerId == fieldPlayer.Id);
    }

    [Fact]
    public void FootballSetLineup_UnpaidLicence_AddsPlayer()
    {
        FootballTestHelpers.ReadyFootballMatch ready = FootballTestHelpers.CreateReadyMatch(assignLineups: false);
        FootballPlayer unpaid = ready.HomePlayers[1];
        ready.Home.UpdateTeamPlayer(unpaid.Id, FootballPosition.Midfielder, null, isActive: false);

        FootballTestHelpers.SetStartingLineup(ready.Match, ready.Home, ready.HomePlayers, onFieldCount: 5);

        ready.Match.Lineup.Should().Contain(row => row.PlayerId == unpaid.Id);
    }
}
