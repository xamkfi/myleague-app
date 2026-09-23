using Domain.Entities.Common;
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;
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

    [Fact]
    public async Task ReplaceContentBlocks_NewBlock_PersistsAndReloads()
    {
        FloorballSeason season = new(
            "League 2026",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc));
        await CompetitionRepository.AddAsync(season);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        FloorballSeason? loaded = await CompetitionRepository.GetSeasonWithContentBlocksAsync(season.Id);
        loaded.Should().NotBeNull();

        List<Guid> existingBlockIds = loaded!.ContentBlocks.Select(block => block.Id).ToList();
        loaded.ReplaceContentBlocks([(null, "Intro", "<p>Hello</p>")]);
        CompetitionRepository.MarkNewContentBlocksAdded(loaded, existingBlockIds);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        FloorballSeason? reloaded = await CompetitionRepository.GetSeasonWithContentBlocksAsync(season.Id);
        reloaded.Should().NotBeNull();
        FloorballSeasonContentBlock block = reloaded!.ContentBlocks.Should().ContainSingle().Subject;
        block.Title.Should().Be("Intro");
        block.ContentHtml.Should().Be("<p>Hello</p>");
        block.SortOrder.Should().Be(0);
        block.Id.Should().NotBe(Guid.Empty);
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

    [Fact]
    public async Task GetLastCompletedForTeamsAsync_ReturnsNewestCompletedMatchesForThoseTeams()
    {
        FloorballSeason season = new(
            "Season",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc));
        FloorballSeason otherSeason = new(
            "Other",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc));
        FloorballTeam home = CreateTeam("Home");
        FloorballTeam away = CreateTeam("Away");
        FloorballPlayer homeGoalie = AddGoalie(home, 1);
        FloorballPlayer awayGoalie = AddGoalie(away, 1);
        season.AddTeam(home);
        season.AddTeam(away);
        otherSeason.AddTeam(home);
        otherSeason.AddTeam(away);

        FloorballMatch older = CreateMatch(season, home, away, new DateTime(2027, 1, 1, 18, 0, 0, DateTimeKind.Utc));
        FloorballMatch newer = CreateMatch(season, home, away, new DateTime(2027, 2, 1, 18, 0, 0, DateTimeKind.Utc));
        FloorballMatch scheduled = CreateMatch(season, home, away, new DateTime(2027, 3, 1, 18, 0, 0, DateTimeKind.Utc));
        FloorballMatch otherCompetition = CreateMatch(otherSeason, home, away, new DateTime(2027, 4, 1, 18, 0, 0, DateTimeKind.Utc));
        Complete(older, home, away, homeGoalie, awayGoalie);
        Complete(newer, home, away, homeGoalie, awayGoalie);
        Complete(otherCompetition, home, away, homeGoalie, awayGoalie);

        await CompetitionRepository.AddAsync(season);
        await CompetitionRepository.AddAsync(otherSeason);
        await MatchRepository.AddAsync(older);
        await MatchRepository.AddAsync(newer);
        await MatchRepository.AddAsync(scheduled);
        await MatchRepository.AddAsync(otherCompetition);
        await DbContext.SaveChangesAsync();

        List<FloorballMatch> result = (await MatchRepository.GetLastCompletedForTeamsAsync(
            season.Id,
            new[] { home.Id, away.Id })).ToList();

        result.Select(match => match.Id).Should().Equal(newer.Id, older.Id);
    }

    [Fact]
    public async Task GetLastCompletedForTeamsAsync_WhenNoTeams_ReturnsEmpty()
    {
        IEnumerable<FloorballMatch> result = await MatchRepository.GetLastCompletedForTeamsAsync(
            Guid.NewGuid(),
            Array.Empty<Guid>());

        result.Should().BeEmpty();
    }

    private FloorballPlayer AddGoalie(FloorballTeam team, int jerseyNumber)
    {
        FloorballPlayer goalie = new(Guid.NewGuid(), new Position(FloorballPosition.Goalkeeper));
        DbContext.FloorballPlayers.Add(goalie);
        team.AddPlayer(goalie, FloorballPosition.Goalkeeper, jerseyNumber);
        return goalie;
    }

    private static FloorballMatch CreateMatch(FloorballSeason season, FloorballTeam home, FloorballTeam away, DateTime start)
    {
        return new FloorballMatch(season, home, away, start, "Arena");
    }

    private static void Complete(
        FloorballMatch match,
        FloorballTeam home,
        FloorballTeam away,
        FloorballPlayer homeGoalie,
        FloorballPlayer awayGoalie)
    {
        FloorballReferee referee = new(
            Guid.NewGuid(),
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 12, 31, 0, 0, 0, DateTimeKind.Utc));
        match.AddOfficial(referee);
        match.SetActiveGoalie(home.Id, homeGoalie.Id);
        match.SetActiveGoalie(away.Id, awayGoalie.Id);
        match.Start();
        match.Complete();
    }
}
