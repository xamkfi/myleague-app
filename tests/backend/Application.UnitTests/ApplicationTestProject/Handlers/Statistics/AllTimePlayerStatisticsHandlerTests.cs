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
    public async Task Floorball_Handle_SumsTwoSeasons_ExcludesLoanPlayer_OrdersByGoals()
    {
        Person aino = new("Aino", "Aalto");
        Person bea = new("Bea", "Berg");
        Guid ainoPlayerId = Guid.NewGuid();
        Guid beaPlayerId = Guid.NewGuid();
        Guid loanPlayerId = Guid.NewGuid();

        List<AllTimePlayerStatRow> rows =
        [
            Row(ainoPlayerId, aino.Id, "Old Club", new DateTime(2024, 9, 1), games: 10, goals: 5, assists: 3, points: 8, penalties: 2),
            Row(ainoPlayerId, aino.Id, "New Club", new DateTime(2025, 9, 1), games: 8, goals: 4, assists: 2, points: 6, penalties: 4),
            Row(beaPlayerId, bea.Id, "Solo", new DateTime(2025, 9, 1), games: 6, goals: 2, assists: 20, points: 22, penalties: 1),
            Row(loanPlayerId, Guid.NewGuid(), "Loan", new DateTime(2025, 9, 1), games: 9, goals: 30, assists: 0, points: 30, penalties: 0, loan: true),
            Row(Guid.NewGuid(), Guid.NewGuid(), "Bench", new DateTime(2025, 9, 1), games: 0, goals: 0, assists: 0, points: 0, penalties: 12)
        ];

        GetFloorballAllTimePlayerStatisticsHandler handler = new(
            FloorballRepository(rows),
            Persons(aino, bea),
            Mock.Of<ILogger<GetFloorballAllTimePlayerStatisticsHandler>>());

        Result<PagedResult<FloorballAllTimePlayerStatisticsDto>> result = await handler.Handle(
            new GetFloorballAllTimePlayerStatisticsQuery(Sort: AllTimeStatSort.Goals),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        List<FloorballAllTimePlayerStatisticsDto> players = result.Data!.Items.ToList();
        players.Should().HaveCount(2);
        players[0].PlayerId.Should().Be(ainoPlayerId);
        players[0].PlayerName.Should().Be("Aino Aalto");
        players[0].TeamName.Should().Be("New Club");
        players[0].GamesPlayed.Should().Be(18);
        players[0].Goals.Should().Be(9);
        players[0].Assists.Should().Be(5);
        players[0].Points.Should().Be(14);
        players[0].PenaltyMinutes.Should().Be(6);
        players[1].PlayerId.Should().Be(beaPlayerId);
        players[1].Goals.Should().Be(2);
        players.Should().NotContain(player => player.PlayerId == loanPlayerId);
    }

    [Fact]
    public async Task Football_Handle_SumsCardsAcrossSeasons_OrdersByYellowCards()
    {
        Person aino = new("Aino", "Aalto");
        Person bea = new("Bea", "Berg");
        Guid ainoPlayerId = Guid.NewGuid();
        Guid beaPlayerId = Guid.NewGuid();
        Guid loanPlayerId = Guid.NewGuid();

        List<AllTimePlayerStatRow> rows =
        [
            Row(ainoPlayerId, aino.Id, "Old Club", new DateTime(2024, 4, 1), games: 5, goals: 3, assists: 1, points: 4, yellow: 2, red: 0),
            Row(ainoPlayerId, aino.Id, "New Club", new DateTime(2025, 4, 1), games: 5, goals: 1, assists: 1, points: 2, yellow: 1, red: 1),
            Row(beaPlayerId, bea.Id, "Solo", new DateTime(2025, 4, 1), games: 8, goals: 10, assists: 10, points: 20, yellow: 1, red: 0),
            Row(loanPlayerId, Guid.NewGuid(), "Loan", new DateTime(2025, 4, 1), games: 4, goals: 1, assists: 0, points: 1, yellow: 8, red: 2, loan: true)
        ];

        GetFootballAllTimePlayerStatisticsHandler handler = new(
            FootballRepository(rows),
            Persons(aino, bea),
            Mock.Of<ILogger<GetFootballAllTimePlayerStatisticsHandler>>());

        Result<PagedResult<FootballAllTimePlayerStatisticsDto>> result = await handler.Handle(
            new GetFootballAllTimePlayerStatisticsQuery(Sort: AllTimeStatSort.YellowCards),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        List<FootballAllTimePlayerStatisticsDto> players = result.Data!.Items.ToList();
        players.Should().HaveCount(2);
        players[0].PlayerId.Should().Be(ainoPlayerId);
        players[0].TeamName.Should().Be("New Club");
        players[0].GamesPlayed.Should().Be(10);
        players[0].Goals.Should().Be(4);
        players[0].Assists.Should().Be(2);
        players[0].Points.Should().Be(6);
        players[0].YellowCards.Should().Be(3);
        players[0].RedCards.Should().Be(1);
        players[1].YellowCards.Should().Be(1);
        players.Should().NotContain(player => player.PlayerId == loanPlayerId);
    }

    [Fact]
    public async Task Hockey_Handle_SumsTwoSeasons_ExcludesLoanPlayer_OrdersByPenalties()
    {
        Person aino = new("Aino", "Aalto");
        Person bea = new("Bea", "Berg");
        Guid ainoPlayerId = Guid.NewGuid();
        Guid beaPlayerId = Guid.NewGuid();
        Guid loanPlayerId = Guid.NewGuid();

        List<AllTimePlayerStatRow> rows =
        [
            Row(ainoPlayerId, aino.Id, "Old Club", new DateTime(2024, 9, 1), games: 10, goals: 2, assists: 2, points: 4, penalties: 6),
            Row(ainoPlayerId, aino.Id, "New Club", new DateTime(2025, 9, 1), games: 12, goals: 3, assists: 1, points: 4, penalties: 8),
            Row(beaPlayerId, bea.Id, "Solo", new DateTime(2025, 9, 1), games: 20, goals: 15, assists: 15, points: 30, penalties: 4),
            Row(loanPlayerId, Guid.NewGuid(), "Loan", new DateTime(2025, 9, 1), games: 7, goals: 1, assists: 0, points: 1, penalties: 40, loan: true)
        ];

        GetHockeyAllTimePlayerStatisticsHandler handler = new(
            HockeyRepository(rows),
            Persons(aino, bea),
            Mock.Of<ILogger<GetHockeyAllTimePlayerStatisticsHandler>>());

        Result<PagedResult<HockeyAllTimePlayerStatisticsDto>> result = await handler.Handle(
            new GetHockeyAllTimePlayerStatisticsQuery(Sort: AllTimeStatSort.Penalties),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        List<HockeyAllTimePlayerStatisticsDto> players = result.Data!.Items.ToList();
        players.Should().HaveCount(2);
        players[0].PlayerId.Should().Be(ainoPlayerId);
        players[0].PlayerName.Should().Be("Aino Aalto");
        players[0].TeamName.Should().Be("New Club");
        players[0].GamesPlayed.Should().Be(22);
        players[0].Goals.Should().Be(5);
        players[0].Assists.Should().Be(3);
        players[0].Points.Should().Be(8);
        players[0].PenaltyMinutes.Should().Be(14);
        players[1].PenaltyMinutes.Should().Be(4);
        players.Should().NotContain(player => player.PlayerId == loanPlayerId);
    }

    [Fact]
    public async Task Floorball_Handle_Search_KeepsRankFromFullList()
    {
        Person aino = new("Aino", "Aalto");
        Person bea = new("Bea", "Berg");
        Person cecilia = new("Cecilia", "Corn");
        Guid ceciliaPlayerId = Guid.NewGuid();

        List<AllTimePlayerStatRow> rows =
        [
            Row(Guid.NewGuid(), aino.Id, "A", new DateTime(2025, 9, 1), games: 10, goals: 10, assists: 10, points: 20),
            Row(Guid.NewGuid(), bea.Id, "B", new DateTime(2025, 9, 1), games: 10, goals: 5, assists: 5, points: 10),
            Row(ceciliaPlayerId, cecilia.Id, "C", new DateTime(2025, 9, 1), games: 10, goals: 1, assists: 1, points: 2)
        ];

        GetFloorballAllTimePlayerStatisticsHandler handler = new(
            FloorballRepository(rows),
            Persons(aino, bea, cecilia),
            Mock.Of<ILogger<GetFloorballAllTimePlayerStatisticsHandler>>());

        Result<PagedResult<FloorballAllTimePlayerStatisticsDto>> result = await handler.Handle(
            new GetFloorballAllTimePlayerStatisticsQuery(Search: "  cecil "),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        List<FloorballAllTimePlayerStatisticsDto> players = result.Data!.Items.ToList();
        players.Should().ContainSingle();
        players[0].PlayerId.Should().Be(ceciliaPlayerId);
        players[0].Rank.Should().Be(3);
        result.Data.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Floorball_Handle_TeamFilter_RanksWithinTeamOnly()
    {
        Person aino = new("Aino", "Aalto");
        Person bea = new("Bea", "Berg");
        Person cecilia = new("Cecilia", "Corn");
        Guid teamId = Guid.NewGuid();
        Guid otherTeamId = Guid.NewGuid();
        Guid beaPlayerId = Guid.NewGuid();
        Guid ceciliaPlayerId = Guid.NewGuid();

        List<AllTimePlayerStatRow> rows =
        [
            Row(Guid.NewGuid(), aino.Id, "Other", new DateTime(2025, 9, 1), games: 10, goals: 30, assists: 0, points: 30, teamId: otherTeamId),
            Row(beaPlayerId, bea.Id, "Club", new DateTime(2024, 9, 1), games: 10, goals: 4, assists: 4, points: 8, teamId: teamId),
            Row(beaPlayerId, bea.Id, "Club", new DateTime(2025, 9, 1), games: 10, goals: 2, assists: 2, points: 4, teamId: teamId),
            Row(ceciliaPlayerId, cecilia.Id, "Club", new DateTime(2025, 9, 1), games: 10, goals: 5, assists: 5, points: 10, teamId: teamId)
        ];

        GetFloorballAllTimePlayerStatisticsHandler handler = new(
            FloorballRepository(rows),
            Persons(aino, bea, cecilia),
            Mock.Of<ILogger<GetFloorballAllTimePlayerStatisticsHandler>>());

        Result<PagedResult<FloorballAllTimePlayerStatisticsDto>> result = await handler.Handle(
            new GetFloorballAllTimePlayerStatisticsQuery(TeamId: teamId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        List<FloorballAllTimePlayerStatisticsDto> players = result.Data!.Items.ToList();
        players.Select(player => player.PlayerId).Should().Equal(beaPlayerId, ceciliaPlayerId);
        players.Select(player => player.Rank).Should().Equal(1, 2);
        players[0].Points.Should().Be(12);
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

    private static IFloorballStatisticsRepository FloorballRepository(List<AllTimePlayerStatRow> rows)
    {
        Mock<IFloorballStatisticsRepository> repository = new();
        repository
            .Setup(repo => repo.GetAllTimePlayerStatRowsAsync(
                It.IsAny<TeamCategory>(),
                It.IsAny<AllTimeCompetitionFilter>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rows);
        return repository.Object;
    }

    private static IFootballStatisticsRepository FootballRepository(List<AllTimePlayerStatRow> rows)
    {
        Mock<IFootballStatisticsRepository> repository = new();
        repository
            .Setup(repo => repo.GetAllTimePlayerStatRowsAsync(
                It.IsAny<TeamCategory>(),
                It.IsAny<AllTimeCompetitionFilter>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rows);
        return repository.Object;
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

    private static IPersonRepository Persons(params Person[] people)
    {
        Mock<IPersonRepository> repository = new();
        repository
            .Setup(repo => repo.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync((IEnumerable<Guid> ids) => people.Where(person => ids.Contains(person.Id)).ToList());
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
