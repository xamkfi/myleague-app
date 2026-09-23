using Domain.Common;
using Domain.Entities.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Enums.Common;
using Domain.Enums.Football;
using Domain.Repositories.Common;
using Domain.ValueObjects.Football;
using InfrastructureIntegrationTestProject.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Repositories.Football;

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

    [Fact]
    public async Task GetLastCompletedForTeamsAsync_ReturnsNewestCompletedMatchesForThoseTeams()
    {
        FootballMatchRules rules = new(1, 20, 5, false, 0, false, false, 2, 5, false);
        FootballSeason season = new(
            "Season",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            rules);
        FootballSeason otherSeason = new(
            "Other",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            rules);
        FootballTeam home = CreateTeam("Home");
        FootballTeam away = CreateTeam("Away");
        FootballPlayer[] homePlayers = AddSquad(home);
        FootballPlayer[] awayPlayers = AddSquad(away);
        season.AddTeam(home);
        season.AddTeam(away);
        otherSeason.AddTeam(home);
        otherSeason.AddTeam(away);

        FootballMatch older = CreateMatch(season, home, away, new DateTime(2027, 1, 1, 18, 0, 0, DateTimeKind.Utc));
        FootballMatch newer = CreateMatch(season, home, away, new DateTime(2027, 2, 1, 18, 0, 0, DateTimeKind.Utc));
        FootballMatch scheduled = CreateMatch(season, home, away, new DateTime(2027, 3, 1, 18, 0, 0, DateTimeKind.Utc));
        FootballMatch otherCompetition = CreateMatch(otherSeason, home, away, new DateTime(2027, 4, 1, 18, 0, 0, DateTimeKind.Utc));
        Complete(older, homePlayers, awayPlayers);
        Complete(newer, homePlayers, awayPlayers);
        Complete(otherCompetition, homePlayers, awayPlayers);

        await CompetitionRepository.AddAsync(season);
        await CompetitionRepository.AddAsync(otherSeason);
        await MatchRepository.AddAsync(older);
        await MatchRepository.AddAsync(newer);
        await MatchRepository.AddAsync(scheduled);
        await MatchRepository.AddAsync(otherCompetition);
        await DbContext.SaveChangesAsync();

        List<FootballMatch> result = (await MatchRepository.GetLastCompletedForTeamsAsync(
            season.Id,
            new[] { home.Id, away.Id })).ToList();

        result.Select(match => match.Id).Should().Equal(newer.Id, older.Id);
        result.Should().NotContain(match => match.Id == scheduled.Id || match.Id == otherCompetition.Id);
    }

    [Fact]
    public async Task GetLastCompletedForTeamsAsync_WhenNoTeams_ReturnsEmpty()
    {
        IEnumerable<FootballMatch> result = await MatchRepository.GetLastCompletedForTeamsAsync(
            Guid.NewGuid(),
            Array.Empty<Guid>());

        result.Should().BeEmpty();
    }

    private FootballPlayer[] AddSquad(FootballTeam team)
    {
        FootballPlayer[] players = new FootballPlayer[5];
        for (int index = 0; index < players.Length; index++)
        {
            players[index] = new FootballPlayer(Guid.NewGuid(), new FootballPositionPreference(FootballPosition.Midfielder));
            DbContext.FootballPlayers.Add(players[index]);
            team.AddPlayer(players[index], FootballPosition.Midfielder, jerseyNumber: index + 1);
        }

        return players;
    }

    private static FootballMatch CreateMatch(FootballSeason season, FootballTeam home, FootballTeam away, DateTime kickoff)
    {
        return new FootballMatch(season, home, away, kickoff, "Pitch");
    }

    private static void Complete(FootballMatch match, FootballPlayer[] homePlayers, FootballPlayer[] awayPlayers)
    {
        match.SetLineup(
            match.HomeTeamId!.Value,
            homePlayers.Select(player => new FootballLineupSelection(player.Id, FootballPosition.Midfielder, true)));
        match.SetLineup(
            match.AwayTeamId!.Value,
            awayPlayers.Select(player => new FootballLineupSelection(player.Id, FootballPosition.Midfielder, true)));
        match.Start();
        match.Complete();
    }
}

public class FootballPlayerRepositoryTests : FootballIntegrationTestBase
{
    [Fact]
    public async Task GetByIdsAsync_ReturnsOnlyRequestedPlayers()
    {
        FootballPlayer first = new(Guid.NewGuid(), new FootballPositionPreference(FootballPosition.Midfielder));
        FootballPlayer second = new(Guid.NewGuid(), new FootballPositionPreference(FootballPosition.Forward));
        FootballPlayer other = new(Guid.NewGuid(), new FootballPositionPreference(FootballPosition.Defender));
        DbContext.FootballPlayers.AddRange(first, second, other);
        await DbContext.SaveChangesAsync();

        FootballPlayerRepository repository = new(DbContext, new UnusedPersonRepository());
        Dictionary<Guid, FootballPlayer> result = await repository.GetByIdsAsync(new[] { first.Id, second.Id, Guid.NewGuid() });

        result.Keys.Should().BeEquivalentTo(new[] { first.Id, second.Id });
    }

    [Fact]
    public async Task GetByIdsAsync_WhenEmpty_ReturnsEmptyDictionary()
    {
        FootballPlayerRepository repository = new(DbContext, new UnusedPersonRepository());

        Dictionary<Guid, FootballPlayer> result = await repository.GetByIdsAsync(Array.Empty<Guid>());

        result.Should().BeEmpty();
    }

    private sealed class UnusedPersonRepository : IPersonRepository
    {
        public Task<Person?> GetByIdAsync(Guid id) => Task.FromResult<Person?>(null);
        public Task<IEnumerable<Person>> GetByIdsAsync(IEnumerable<Guid> ids) => Task.FromResult<IEnumerable<Person>>(Array.Empty<Person>());
        public Task<Person?> GetByFullNameAsync(string firstName, string lastName) => Task.FromResult<Person?>(null);
        public Task<Person?> GetByEmailAsync(string email) => Task.FromResult<Person?>(null);
        public Task<IEnumerable<Person>> GetAllAsync(int page, int pageSize, string? firstName, string? lastName, string? birthDate, bool? isRegistered, CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<Person>>(Array.Empty<Person>());
        public Task<int> GetCountAsync(string? firstName, string? lastName, string? birthDate, bool? isRegistered, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<IEnumerable<Person>> GetByFirstNameAsync(string firstName) => Task.FromResult<IEnumerable<Person>>(Array.Empty<Person>());
        public Task<IEnumerable<Person>> GetByLastNameAsync(string lastName) => Task.FromResult<IEnumerable<Person>>(Array.Empty<Person>());
        public Task<IEnumerable<Person>> GetByAgeRangeAsync(int minAge, int maxAge) => Task.FromResult<IEnumerable<Person>>(Array.Empty<Person>());
        public Task AddAsync(Person person) => Task.CompletedTask;
        public Task UpdateAsync(Person person) => Task.CompletedTask;
        public Task DeleteAsync(Guid id) => Task.CompletedTask;
        public Task<PagedResult<Person>> SearchByNameAsync(string searchTerm, int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult(PagedResult.Empty<Person>(page, pageSize));
        public Task<IReadOnlyList<Guid>> GetIdsByNameContainsAsync(string searchTerm, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Guid>>(Array.Empty<Guid>());
        public Task<bool> ExistsAsync(Guid id) => Task.FromResult(false);
        public Task<bool> ExistsByFullNameAsync(string firstName, string lastName) => Task.FromResult(false);
    }
}
