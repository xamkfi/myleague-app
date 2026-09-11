using Domain.Entities.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Enums.Common;
using Domain.Enums.Football;
using Domain.ValueObjects.Football;
using InfrastructureIntegrationTestProject.Common;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureIntegrationTestProject.Repositories.Football;

public class FootballCompetitionRepositoryTests : FootballIntegrationTestBase
{
    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        FootballCompetition? result = await CompetitionRepository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddSeasonAndTournament_RoundTripsWithTph()
    {
        FootballSeason season = new(
            "League 2026",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            new FootballMatchRules(2, 20, 5, true, 0, false, false, 2, 5, false));
        FootballTournament tournament = new(
            "Cup 2026",
            new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 10, 0, 0, 0, DateTimeKind.Utc),
            venue: "Pitch");

        await CompetitionRepository.AddAsync(season);
        await CompetitionRepository.AddAsync(tournament);
        await DbContext.SaveChangesAsync();

        FootballCompetition? loadedSeason = await CompetitionRepository.GetByIdAsync(season.Id);
        FootballCompetition? loadedTournament = await CompetitionRepository.GetByIdAsync(tournament.Id);

        loadedSeason.Should().BeOfType<FootballSeason>();
        loadedTournament.Should().BeOfType<FootballTournament>();
        loadedSeason!.Name.Should().Be("League 2026");

        int seasonCount = await DbContext.FootballSeasons.CountAsync();
        int tournamentCount = await DbContext.FootballTournaments.CountAsync();
        seasonCount.Should().Be(1);
        tournamentCount.Should().Be(1);
    }
}

public class FootballTeamAndMatchRepositoryTests : FootballIntegrationTestBase
{
    private static FootballTeam CreateTeam(string name)
    {
        Club club = new(name + " Club");
        return new FootballTeam(
            name,
            divisionId: null,
            club,
            homeArena: "Pitch",
            primaryJerseyColor: "Red",
            teamCategory: TeamCategory.Adult);
    }

    [Fact]
    public async Task Team_WithRoster_PersistsAndReloads()
    {
        FootballTeam team = CreateTeam("United");
        FootballPlayer player = new(Guid.NewGuid(), new FootballPositionPreference(FootballPosition.Midfielder));
        DbContext.FootballPlayers.Add(player);
        team.AddPlayer(player, FootballPosition.Midfielder, jerseyNumber: 8);

        await TeamRepository.AddAsync(team);
        await DbContext.SaveChangesAsync();

        FootballTeam? loaded = await TeamRepository.GetByIdAsync(team.Id);

        loaded.Should().NotBeNull();
        loaded!.Roster.Should().ContainSingle(r => r.PlayerId == player.Id && r.JerseyNumber == 8);
    }

    [Fact]
    public async Task Match_WithPlaceholderTeams_Persists()
    {
        FootballSeason season = new(
            "Season",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            new FootballMatchRules(2, 20, 5, true, 0, false, false, 2, 5, false));
        FootballMatch match = new(
            season,
            homeTeam: null,
            awayTeam: null,
            new DateTime(2027, 1, 15, 18, 0, 0, DateTimeKind.Utc),
            "Pitch");

        await CompetitionRepository.AddAsync(season);
        await MatchRepository.AddAsync(match);
        await DbContext.SaveChangesAsync();

        FootballMatch? loaded = await MatchRepository.GetByIdAsync(match.Id);

        loaded.Should().NotBeNull();
        loaded!.HomeTeamId.Should().BeNull();
        loaded.AwayTeamId.Should().BeNull();
        loaded.CompetitionId.Should().Be(season.Id);
        loaded.Status.Should().Be(FootballMatchStatus.Scheduled);
    }
}
