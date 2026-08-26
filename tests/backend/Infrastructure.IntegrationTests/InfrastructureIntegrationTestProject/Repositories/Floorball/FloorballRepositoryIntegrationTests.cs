using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Common;
using Domain.Enums.Floorball;
using Domain.ValueObjects.Floorball;
using InfrastructureIntegrationTestProject.Common;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureIntegrationTestProject.Repositories.Floorball;

public class FloorballCompetitionRepositoryTests : FloorballIntegrationTestBase
{
    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        FloorballCompetition? result = await CompetitionRepository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddSeasonAndTournament_RoundTripsWithTph()
    {
        FloorballSeason season = new(
            "League 2026",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc));
        FloorballTournament tournament = new(
            "Cup 2026",
            new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 10, 0, 0, 0, DateTimeKind.Utc),
            venue: "Arena");

        await CompetitionRepository.AddAsync(season);
        await CompetitionRepository.AddAsync(tournament);
        await DbContext.SaveChangesAsync();

        FloorballCompetition? loadedSeason = await CompetitionRepository.GetByIdAsync(season.Id);
        FloorballCompetition? loadedTournament = await CompetitionRepository.GetByIdAsync(tournament.Id);

        loadedSeason.Should().BeOfType<FloorballSeason>();
        loadedTournament.Should().BeOfType<FloorballTournament>();
        loadedSeason!.Name.Should().Be("League 2026");
        ((FloorballTournament)loadedTournament!).Venue.Should().Be("Arena");

        int seasonCount = await DbContext.FloorballSeasons.CountAsync();
        int tournamentCount = await DbContext.FloorballTournaments.CountAsync();
        seasonCount.Should().Be(1);
        tournamentCount.Should().Be(1);
    }
}

public class FloorballTeamAndMatchRepositoryTests : FloorballIntegrationTestBase
{
    private static FloorballTeam CreateTeam(string name)
    {
        Club club = new(name + " Club");
        return new FloorballTeam(
            name,
            divisionId: null,
            club,
            homeArena: "Arena",
            primaryJerseyColor: "Blue",
            teamCategory: TeamCategory.Adult);
    }

    [Fact]
    public async Task Team_WithRoster_PersistsAndReloads()
    {
        FloorballTeam team = CreateTeam("Wolves");
        FloorballPlayer player = new(Guid.NewGuid(), new Position(FloorballPosition.Forward));
        DbContext.FloorballPlayers.Add(player);
        team.AddPlayer(player, FloorballPosition.Forward, jerseyNumber: 10);

        await TeamRepository.AddAsync(team);
        await DbContext.SaveChangesAsync();

        FloorballTeam? loaded = await TeamRepository.GetByIdAsync(team.Id);

        loaded.Should().NotBeNull();
        loaded!.ClubId.Should().NotBe(Guid.Empty);
        loaded.Roster.Should().ContainSingle(r => r.PlayerId == player.Id && r.JerseyNumber == 10);
    }

    [Fact]
    public async Task Match_WithTeams_PersistsAndReloads()
    {
        FloorballSeason season = new(
            "Season",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc));
        FloorballTeam home = CreateTeam("Home");
        FloorballTeam away = CreateTeam("Away");
        season.AddTeam(home);
        season.AddTeam(away);

        FloorballMatch match = new(
            season,
            home,
            away,
            new DateTime(2027, 1, 15, 18, 0, 0, DateTimeKind.Utc),
            "Arena");

        await CompetitionRepository.AddAsync(season);
        await MatchRepository.AddAsync(match);
        await DbContext.SaveChangesAsync();

        FloorballMatch? loaded = await MatchRepository.GetByIdAsync(match.Id);

        loaded.Should().NotBeNull();
        loaded!.HomeTeamId.Should().Be(home.Id);
        loaded.AwayTeamId.Should().Be(away.Id);
        loaded.CompetitionId.Should().Be(season.Id);
        loaded.Status.Should().Be(FloorballMatchStatus.Scheduled);
    }
}
