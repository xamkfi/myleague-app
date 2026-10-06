using Application.Common;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Statistics.Handlers;
using Application.Features.Floorball.Statistics.Queries;
using Application.Features.Floorball.Statistics.Validators;
using Application.Features.Football.Statistics.DTOs;
using Application.Features.Football.Statistics.Handlers;
using Application.Features.Football.Statistics.Queries;
using Application.Features.Football.Statistics.Validators;
using Application.Features.Hockey.Statistics.DTOs;
using Application.Features.Hockey.Statistics.Handlers;
using Application.Features.Hockey.Statistics.Queries;
using Application.Features.Hockey.Statistics.Validators;
using Domain.Common;
using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Football;
using Domain.Repositories.Hockey;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Statistics;

public class AllTimePlayerStatisticsHandlerTests
{
    [Fact]
    public async Task Floorball_Handle_MapsRepositoryPage_AndPassesRequest()
    {
        Person aino = new("Aino", "Aalto");
        Person bea = new("Bea", "Berg");
        Guid teamId = Guid.NewGuid();
        Guid ainoPlayerId = Guid.NewGuid();
        Guid beaPlayerId = Guid.NewGuid();
        AllTimePlayerPageRequest? captured = null;
        Mock<IFloorballStatisticsRepository> repository = new();
        repository
            .Setup(repo => repo.GetAllTimePlayerPageAsync(
                TeamCategory.Women,
                AllTimeCompetitionFilter.Tournament,
                It.IsAny<AllTimePlayerPageRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback((TeamCategory _, AllTimeCompetitionFilter _, AllTimePlayerPageRequest pageRequest, CancellationToken _) => captured = pageRequest)
            .ReturnsAsync(Page(
                26,
                Totals(26, ainoPlayerId, aino.Id, "New Club", games: 18, goals: 9, assists: 5, points: 14, penalties: 6),
                Totals(27, beaPlayerId, bea.Id, "Solo", games: 6, goals: 2, assists: 20, points: 22, penalties: 1)));

        GetFloorballAllTimePlayerStatisticsHandler handler = new(
            repository.Object,
            Persons(aino, bea),
            Mock.Of<ILogger<GetFloorballAllTimePlayerStatisticsHandler>>());

        Result<PagedResult<FloorballAllTimePlayerStatisticsDto>> result = await handler.Handle(
            new GetFloorballAllTimePlayerStatisticsQuery(
                Page: 2,
                TeamCategory: TeamCategory.Women,
                CompetitionType: AllTimeCompetitionFilter.Tournament,
                Sort: AllTimeStatSort.Goals,
                Direction: AllTimeSortDirection.Asc,
                TeamId: teamId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured.Should().Be(new AllTimePlayerPageRequest(2, 25, AllTimeStatSort.Goals, AllTimeSortDirection.Asc, teamId, null));
        result.Data!.TotalCount.Should().Be(26);
        List<FloorballAllTimePlayerStatisticsDto> players = result.Data.Items.ToList();
        players.Select(player => player.Rank).Should().Equal(26, 27);
        players[0].PlayerId.Should().Be(ainoPlayerId);
        players[0].PlayerName.Should().Be("Aino Aalto");
        players[0].TeamName.Should().Be("New Club");
        players[0].GamesPlayed.Should().Be(18);
        players[0].Goals.Should().Be(9);
        players[0].Assists.Should().Be(5);
        players[0].Points.Should().Be(14);
        players[0].PenaltyMinutes.Should().Be(6);
        players[1].PlayerName.Should().Be("Bea Berg");
    }

    [Fact]
    public async Task Football_Handle_MapsCards()
    {
        Person aino = new("Aino", "Aalto");
        Guid ainoPlayerId = Guid.NewGuid();
        Mock<IFootballStatisticsRepository> repository = new();
        repository
            .Setup(repo => repo.GetAllTimePlayerPageAsync(
                It.IsAny<TeamCategory>(),
                It.IsAny<AllTimeCompetitionFilter>(),
                It.IsAny<AllTimePlayerPageRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(1, Totals(1, ainoPlayerId, aino.Id, "New Club", games: 10, goals: 4, assists: 2, points: 6, yellow: 3, red: 1)));

        GetFootballAllTimePlayerStatisticsHandler handler = new(
            repository.Object,
            Persons(aino),
            Mock.Of<ILogger<GetFootballAllTimePlayerStatisticsHandler>>());

        Result<PagedResult<FootballAllTimePlayerStatisticsDto>> result = await handler.Handle(
            new GetFootballAllTimePlayerStatisticsQuery(Sort: AllTimeStatSort.YellowCards),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        FootballAllTimePlayerStatisticsDto player = result.Data!.Items.Single();
        player.PlayerId.Should().Be(ainoPlayerId);
        player.PlayerName.Should().Be("Aino Aalto");
        player.TeamName.Should().Be("New Club");
        player.GamesPlayed.Should().Be(10);
        player.Points.Should().Be(6);
        player.YellowCards.Should().Be(3);
        player.RedCards.Should().Be(1);
    }

    [Fact]
    public async Task Hockey_Handle_MapsPenalties()
    {
        Person aino = new("Aino", "Aalto");
        Guid ainoPlayerId = Guid.NewGuid();
        Mock<IHockeyStatisticsRepository> repository = new();
        repository
            .Setup(repo => repo.GetAllTimePlayerPageAsync(
                It.IsAny<TeamCategory>(),
                It.IsAny<AllTimeCompetitionFilter>(),
                It.IsAny<AllTimePlayerPageRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(1, Totals(1, ainoPlayerId, aino.Id, "New Club", games: 22, goals: 5, assists: 3, points: 8, penalties: 14)));

        GetHockeyAllTimePlayerStatisticsHandler handler = new(
            repository.Object,
            Persons(aino),
            Mock.Of<ILogger<GetHockeyAllTimePlayerStatisticsHandler>>());

        Result<PagedResult<HockeyAllTimePlayerStatisticsDto>> result = await handler.Handle(
            new GetHockeyAllTimePlayerStatisticsQuery(Sort: AllTimeStatSort.Penalties),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        HockeyAllTimePlayerStatisticsDto player = result.Data!.Items.Single();
        player.PlayerName.Should().Be("Aino Aalto");
        player.GamesPlayed.Should().Be(22);
        player.Goals.Should().Be(5);
        player.Assists.Should().Be(3);
        player.Points.Should().Be(8);
        player.PenaltyMinutes.Should().Be(14);
    }

    [Fact]
    public async Task Floorball_Handle_Search_PassesMatchingPersonIds_KeepsRepositoryRank()
    {
        Person aino = new("Aino", "Aalto");
        Person cecilia = new("Cecilia", "Corn");
        Guid ceciliaPlayerId = Guid.NewGuid();
        AllTimePlayerPageRequest? captured = null;
        Mock<IFloorballStatisticsRepository> repository = new();
        repository
            .Setup(repo => repo.GetAllTimePlayerPageAsync(
                It.IsAny<TeamCategory>(),
                It.IsAny<AllTimeCompetitionFilter>(),
                It.IsAny<AllTimePlayerPageRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback((TeamCategory _, AllTimeCompetitionFilter _, AllTimePlayerPageRequest pageRequest, CancellationToken _) => captured = pageRequest)
            .ReturnsAsync(Page(1, Totals(3, ceciliaPlayerId, cecilia.Id, "C", games: 10, goals: 1, assists: 1, points: 2)));

        GetFloorballAllTimePlayerStatisticsHandler handler = new(
            repository.Object,
            Persons(aino, cecilia),
            Mock.Of<ILogger<GetFloorballAllTimePlayerStatisticsHandler>>());

        Result<PagedResult<FloorballAllTimePlayerStatisticsDto>> result = await handler.Handle(
            new GetFloorballAllTimePlayerStatisticsQuery(Search: "  cecil "),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured!.PersonIds.Should().Equal(cecilia.Id);
        FloorballAllTimePlayerStatisticsDto player = result.Data!.Items.Single();
        player.PlayerId.Should().Be(ceciliaPlayerId);
        player.Rank.Should().Be(3);
        result.Data.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Hockey_AllTimeTeams_ListsTeamsWithGames_UsesLatestName()
    {
        Guid renamedTeamId = Guid.NewGuid();
        Guid benchTeamId = Guid.NewGuid();

        List<AllTimePlayerStatRow> rows =
        [
            Row(Guid.NewGuid(), Guid.NewGuid(), "Old Name", new DateTime(2023, 9, 1), games: 5, goals: 1, assists: 1, points: 2, teamId: renamedTeamId),
            Row(Guid.NewGuid(), Guid.NewGuid(), "New Name", new DateTime(2025, 9, 1), games: 5, goals: 1, assists: 1, points: 2, teamId: renamedTeamId),
            Row(Guid.NewGuid(), Guid.NewGuid(), "Bench", new DateTime(2025, 9, 1), games: 0, goals: 0, assists: 0, points: 0, teamId: benchTeamId)
        ];

        GetHockeyAllTimeTeamsHandler handler = new(HockeyRepository(rows));

        Result<List<AllTimeTeamOptionDto>> result = await handler.Handle(
            new GetHockeyAllTimeTeamsQuery(),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data![0].TeamId.Should().Be(renamedTeamId);
        result.Data[0].TeamName.Should().Be("New Name");
    }

    [Fact]
    public void Football_PenaltiesSort_ShouldHaveValidationError()
    {
        GetFootballAllTimePlayerStatisticsQueryValidator validator = new();
        TestValidationResult<GetFootballAllTimePlayerStatisticsQuery> result = validator.TestValidate(
            new GetFootballAllTimePlayerStatisticsQuery(Sort: AllTimeStatSort.Penalties));

        result.ShouldHaveValidationErrorFor(query => query.Sort);
    }

    [Fact]
    public void Floorball_YellowCardsSort_ShouldHaveValidationError()
    {
        GetFloorballAllTimePlayerStatisticsQueryValidator validator = new();
        TestValidationResult<GetFloorballAllTimePlayerStatisticsQuery> result = validator.TestValidate(
            new GetFloorballAllTimePlayerStatisticsQuery(Sort: AllTimeStatSort.YellowCards));

        result.ShouldHaveValidationErrorFor(query => query.Sort);
    }

    [Fact]
    public void Hockey_RedCardsSort_ShouldHaveValidationError()
    {
        GetHockeyAllTimePlayerStatisticsQueryValidator validator = new();
        TestValidationResult<GetHockeyAllTimePlayerStatisticsQuery> result = validator.TestValidate(
            new GetHockeyAllTimePlayerStatisticsQuery(Sort: AllTimeStatSort.RedCards));

        result.ShouldHaveValidationErrorFor(query => query.Sort);
    }

    private static IHockeyStatisticsRepository HockeyRepository(List<AllTimePlayerStatRow> rows)
    {
        Mock<IHockeyStatisticsRepository> repository = new();
        repository
            .Setup(repo => repo.GetAllTimePlayerStatRowsAsync(
                It.IsAny<TeamCategory>(),
                It.IsAny<AllTimeCompetitionFilter>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rows);
        return repository.Object;
    }

    private static PagedResult<AllTimePlayerTotals> Page(int totalCount, params AllTimePlayerTotals[] items) =>
        PagedResult.Create(items.ToList(), totalCount, 1, 25);

    private static AllTimePlayerTotals Totals(
        int rank,
        Guid playerId,
        Guid personId,
        string team,
        int games,
        int goals,
        int assists,
        int points,
        int penalties = 0,
        int yellow = 0,
        int red = 0) =>
        new(rank, playerId, personId, team, games, goals, assists, points, penalties, yellow, red);

    private static IPersonRepository Persons(params Person[] people)
    {
        Mock<IPersonRepository> repository = new();
        repository
            .Setup(repo => repo.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync((IEnumerable<Guid> ids) => people.Where(person => ids.Contains(person.Id)).ToList());
        repository
            .Setup(repo => repo.GetIdsByNameContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string term, CancellationToken _) => people
                .Where(person => person.FullName.Contains(term.Trim(), StringComparison.OrdinalIgnoreCase))
                .Select(person => person.Id)
                .ToList());
        return repository.Object;
    }

    private static AllTimePlayerStatRow Row(
        Guid playerId,
        Guid personId,
        string team,
        DateTime start,
        int games,
        int goals,
        int assists,
        int points,
        int penalties = 0,
        int yellow = 0,
        int red = 0,
        bool loan = false,
        Guid? teamId = null)
    {
        return new AllTimePlayerStatRow(
            playerId,
            personId,
            loan,
            teamId ?? Guid.NewGuid(),
            team,
            start,
            games,
            goals,
            assists,
            points,
            penalties,
            yellow,
            red);
    }
}
