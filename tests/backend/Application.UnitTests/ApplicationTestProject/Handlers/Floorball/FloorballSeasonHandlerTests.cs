using Application.Features.Floorball.Competitions.Commands;
using Application.Features.Floorball.Competitions.Handlers;
using Application.Common;
using Application.Features.Floorball.Seasons.Commands;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Seasons.Handlers;
using Application.Features.Floorball.Seasons.Queries;
using Domain.Entities.Common;
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Floorball;

public class FloorballSeasonHandlerTests
{
    private readonly Mock<IFloorballCompetitionRepository> _competitionRepo = new();
    private readonly Mock<IFloorballCompetitionDivisionRepository> _divisionRepo = new();
    private readonly Mock<IFloorballTeamRepository> _teamRepo = new();
    private readonly Mock<IFloorballMatchRepository> _matchRepo = new();
    private readonly Mock<IClubRepository> _clubRepo = new();
    private readonly Mock<IUnitOfWork> _commonUow = new();
    private readonly Mock<IFloorballUnitOfWork> _floorballUow = new();
    private readonly Mock<IFloorballStatisticsRepository> _statsRepo = new();

    private static FloorballSeason CreateSeason(string name = "Test Season") =>
        new(
            name,
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc));

    private static FloorballTeam CreateTeam(string name = "Wolves")
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
    public async Task CreateFloorballSeason_ValidCommand_AddsAndSaves()
    {
        CreateFloorballSeasonHandler handler = new(
            _competitionRepo.Object,
            _divisionRepo.Object,
            _floorballUow.Object,
            Mock.Of<ILogger<CreateFloorballSeasonHandler>>());

        Guid divisionId = Guid.NewGuid();
        _divisionRepo
            .Setup(r => r.GetCompetitionDivisionsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Enumerable.Empty<FloorballCompetitionDivision>());

        CreateFloorballSeasonCommand command = new(
            "Championship 2026",
            [divisionId],
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc));

        Result<FloorballSeasonDto> result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Championship 2026");
        _competitionRepo.Verify(r => r.AddAsync(It.IsAny<FloorballSeason>()), Times.Once);
        _floorballUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _divisionRepo.Verify(r => r.AddCompetitionDivisionAsync(It.IsAny<Guid>(), divisionId), Times.Once);
    }

    [Fact]
    public async Task ActivateFloorballSeason_WhenMissing_ReturnsNotFound()
    {
        ActivateFloorballSeasonHandler handler = new(
            _competitionRepo.Object,
            _divisionRepo.Object,
            _clubRepo.Object,
            _commonUow.Object,
            _floorballUow.Object,
            _statsRepo.Object,
            Mock.Of<ILogger<ActivateFloorballSeasonHandler>>());

        Guid id = Guid.NewGuid();
        _competitionRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((FloorballCompetition?)null);

        Result<FloorballSeasonDto> result = await handler.Handle(
            new ActivateFloorballSeasonCommand(id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _floorballUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ActivateFloorballSeason_WithTeams_SeedsTeamSeasonStats()
    {
        ActivateFloorballSeasonHandler handler = new(
            _competitionRepo.Object,
            _divisionRepo.Object,
            _clubRepo.Object,
            _commonUow.Object,
            _floorballUow.Object,
            _statsRepo.Object,
            Mock.Of<ILogger<ActivateFloorballSeasonHandler>>());

        FloorballSeason season = CreateSeason();
        FloorballTeam team = CreateTeam();
        season.AddTeam(team);
        _competitionRepo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);
        _divisionRepo
            .Setup(r => r.GetCompetitionDivisionsAsync(season.Id))
            .ReturnsAsync(Enumerable.Empty<FloorballCompetitionDivision>());
        _clubRepo.Setup(r => r.GetByIdAsync(team.ClubId)).ReturnsAsync(new Club("Club"));

        Result<FloorballSeasonDto> result = await handler.Handle(
            new ActivateFloorballSeasonCommand(season.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        season.IsActive.Should().BeTrue();
        _statsRepo.Verify(
            r => r.SaveTeamSeasonStatisticsAsync(It.Is<FloorballTeamSeasonStatistics>(s => s.TeamId == team.Id), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AddTeamToSeason_WhenSeasonMissing_ReturnsNotFound()
    {
        AddTeamToSeasonHandler handler = new(
            _competitionRepo.Object,
            _teamRepo.Object,
            _divisionRepo.Object,
            _clubRepo.Object,
            _floorballUow.Object,
            _statsRepo.Object,
            Mock.Of<ILogger<AddTeamToSeasonHandler>>());

        Guid seasonId = Guid.NewGuid();
        _competitionRepo.Setup(r => r.GetByIdAsync(seasonId)).ReturnsAsync((FloorballCompetition?)null);

        Result<FloorballSeasonDto> result = await handler.Handle(
            new AddFloorballTeamToSeasonCommand(seasonId, Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("FloorballSeason");
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task AddTeamToSeason_WhenSeasonActive_SeedsTeamAndPlayerStats()
    {
        AddTeamToSeasonHandler handler = new(
            _competitionRepo.Object,
            _teamRepo.Object,
            _divisionRepo.Object,
            _clubRepo.Object,
            _floorballUow.Object,
            _statsRepo.Object,
            Mock.Of<ILogger<AddTeamToSeasonHandler>>());

        FloorballSeason season = CreateSeason();
        season.Activate();
        FloorballTeam team = CreateTeam();
        FloorballPlayer player = new(Guid.NewGuid(), new Domain.ValueObjects.Floorball.Position(Domain.Enums.Floorball.FloorballPosition.Forward));
        team.AddPlayer(player, Domain.Enums.Floorball.FloorballPosition.Forward, jerseyNumber: 9);

        _competitionRepo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);
        _teamRepo.Setup(r => r.GetByIdAsync((Guid?)team.Id)).ReturnsAsync(team);
        _divisionRepo
            .Setup(r => r.GetCompetitionDivisionsAsync(season.Id))
            .ReturnsAsync(Enumerable.Empty<FloorballCompetitionDivision>());
        _clubRepo.Setup(r => r.GetByIdAsync(team.ClubId)).ReturnsAsync(new Club("Club"));

        Result<FloorballSeasonDto> result = await handler.Handle(
            new AddFloorballTeamToSeasonCommand(season.Id, team.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        season.Teams.Should().Contain(t => t.Id == team.Id);
        _statsRepo.Verify(
            r => r.SaveTeamSeasonStatisticsAsync(It.Is<FloorballTeamSeasonStatistics>(s => s.TeamId == team.Id), It.IsAny<CancellationToken>()),
            Times.Once);
        _statsRepo.Verify(
            r => r.SavePlayerSeasonStatisticsBatchAsync(
                It.Is<List<FloorballPlayerSeasonStatistics>>(list => list.Count == 1 && list[0].PlayerId == player.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteFloorballSeason_WhenHasMatches_ReturnsFailure()
    {
        DeleteFloorballSeasonHandler handler = new(
            _competitionRepo.Object,
            _matchRepo.Object,
            _floorballUow.Object,
            Mock.Of<ILogger<DeleteFloorballSeasonHandler>>());

        Guid id = Guid.NewGuid();
        _competitionRepo.Setup(r => r.ExistsAsync(id)).ReturnsAsync(true);
        _matchRepo
            .Setup(r => r.GetByCompetitionIdAsync(id))
            .ReturnsAsync([CreateMatchStub()]);

        Result result = await handler.Handle(new DeleteFloorballSeasonCommand(id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("matches");
        _competitionRepo.Verify(r => r.DeleteAsync(id), Times.Never);
    }

    [Fact]
    public async Task DeleteFloorballSeason_WhenEmpty_Deletes()
    {
        DeleteFloorballSeasonHandler handler = new(
            _competitionRepo.Object,
            _matchRepo.Object,
            _floorballUow.Object,
            Mock.Of<ILogger<DeleteFloorballSeasonHandler>>());

        Guid id = Guid.NewGuid();
        _competitionRepo.Setup(r => r.ExistsAsync(id)).ReturnsAsync(true);
        _matchRepo
            .Setup(r => r.GetByCompetitionIdAsync(id))
            .ReturnsAsync(Enumerable.Empty<FloorballMatch>());

        Result result = await handler.Handle(new DeleteFloorballSeasonCommand(id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _competitionRepo.Verify(r => r.DeleteAsync(id), Times.Once);
        _floorballUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetFloorballSeasonYears_WithTeamCategory_ReturnsOnlyThatAudiencesYears()
    {
        _competitionRepo
            .Setup(r => r.GetSeasonDateSummariesAsync(TeamCategory.Youth, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new FloorballSeasonDateSummary(new DateTime(2025, 9, 1), new DateTime(2026, 4, 30), true),
                new FloorballSeasonDateSummary(new DateTime(2025, 9, 1), new DateTime(2026, 4, 30), false),
                new FloorballSeasonDateSummary(new DateTime(2024, 9, 1), new DateTime(2025, 4, 30), false),
            ]);

        GetFloorballSeasonYearsHandler handler = new(
            _competitionRepo.Object,
            Mock.Of<ILogger<GetFloorballSeasonYearsHandler>>());

        Result<IEnumerable<FloorballSeasonYearDto>> result = await handler.Handle(
            new GetFloorballSeasonYearsQuery(TeamCategory.Youth),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        List<FloorballSeasonYearDto> years = result.Data!.ToList();
        years.Should().HaveCount(2);
        years[0].Year.Should().Be("2025-2026");
        years[0].SeasonCount.Should().Be(2);
        years[0].HasActiveSeason.Should().BeTrue();
        years[1].Year.Should().Be("2024-2025");
        years[1].HasActiveSeason.Should().BeFalse();
    }

    private static FloorballMatch CreateMatchStub()
    {
        FloorballSeason season = CreateSeason();
        FloorballTeam home = CreateTeam("Home");
        FloorballTeam away = CreateTeam("Away");
        season.AddTeam(home);
        season.AddTeam(away);
        return new FloorballMatch(
            season,
            home,
            away,
            new DateTime(2027, 1, 1, 18, 0, 0, DateTimeKind.Utc),
            "Arena");
    }
}
