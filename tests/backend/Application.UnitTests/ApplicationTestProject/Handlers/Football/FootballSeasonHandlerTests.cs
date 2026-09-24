using Application.Features.Football.Competitions.Commands;
using Application.Features.Football.Competitions.Handlers;
using Application.Common;
using Application.Features.Football.Seasons.Commands;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Seasons.Handlers;
using Application.Features.Football.Seasons.Queries;
using Domain.Entities.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Statistics;
using Domain.Entities.Football.Teams;
using Domain.Enums.Common;
using Domain.Enums.Football;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using Domain.ValueObjects.Football;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Football;

public class FootballSeasonHandlerTests
{
    private readonly Mock<IFootballCompetitionRepository> _competitionRepo = new();
    private readonly Mock<IFootballCompetitionDivisionRepository> _divisionRepo = new();
    private readonly Mock<IFootballTeamRepository> _teamRepo = new();
    private readonly Mock<IFootballMatchRepository> _matchRepo = new();
    private readonly Mock<IClubRepository> _clubRepo = new();
    private readonly Mock<IUnitOfWork> _commonUow = new();
    private readonly Mock<IFootballUnitOfWork> _footballUow = new();
    private readonly Mock<IFootballStatisticsRepository> _statsRepo = new();

    private static FootballSeason CreateSeason(string name = "Hobby Season") =>
        new(
            name,
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            new FootballMatchRules(2, 20, 5, true, 0, false, false, 2, 5, false));

    private static FootballTeam CreateTeam(string name = "United")
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
    public async Task CreateFootballSeason_ValidCommand_AddsAndSaves()
    {
        CreateFootballSeasonHandler handler = new(
            _competitionRepo.Object,
            _divisionRepo.Object,
            _footballUow.Object,
            Mock.Of<ILogger<CreateFootballSeasonHandler>>());

        Guid divisionId = Guid.NewGuid();
        _divisionRepo
            .Setup(r => r.GetCompetitionDivisionsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Enumerable.Empty<FootballCompetitionDivision>());

        CreateFootballSeasonCommand command = new(
            "Football 2026",
            [divisionId],
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            PlayersOnField: 5);

        Result<FootballSeasonDto> result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Football 2026");
        _competitionRepo.Verify(r => r.AddAsync(It.IsAny<FootballSeason>()), Times.Once);
        _footballUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateFootballSeason_WhenNameAlreadyExists_ReturnsFailure()
    {
        CreateFootballSeasonHandler handler = new(
            _competitionRepo.Object,
            _divisionRepo.Object,
            _footballUow.Object,
            Mock.Of<ILogger<CreateFootballSeasonHandler>>());

        FootballSeason existing = CreateSeason("Football 2026");
        _competitionRepo.Setup(r => r.GetSeasonByNameAsync("Football 2026")).ReturnsAsync(existing);

        CreateFootballSeasonCommand command = new(
            "Football 2026",
            [Guid.NewGuid()],
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            PlayersOnField: 5);

        Result<FootballSeasonDto> result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");
        _competitionRepo.Verify(r => r.AddAsync(It.IsAny<FootballSeason>()), Times.Never);
        _footballUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateFootballSeason_WhenNameBelongsToAnotherSeason_ReturnsFailure()
    {
        UpdateFootballSeasonHandler handler = new(
            _competitionRepo.Object,
            _divisionRepo.Object,
            _footballUow.Object,
            Mock.Of<ILogger<UpdateFootballSeasonHandler>>());

        FootballSeason season = CreateSeason("Own Season");
        FootballSeason other = CreateSeason("Taken Season");
        _competitionRepo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);
        _competitionRepo.Setup(r => r.GetSeasonByNameAsync("Taken Season")).ReturnsAsync(other);

        Result<FootballSeasonDto> result = await handler.Handle(
            new UpdateFootballSeasonCommand(
                season.Id,
                "Taken Season",
                new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");
        season.Name.Should().Be("Own Season");
        _competitionRepo.Verify(r => r.UpdateAsync(It.IsAny<FootballCompetition>()), Times.Never);
        _footballUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateFootballSeason_WhenNameUnchanged_Saves()
    {
        UpdateFootballSeasonHandler handler = new(
            _competitionRepo.Object,
            _divisionRepo.Object,
            _footballUow.Object,
            Mock.Of<ILogger<UpdateFootballSeasonHandler>>());

        FootballSeason season = CreateSeason("Own Season");
        _competitionRepo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);
        _competitionRepo.Setup(r => r.GetSeasonByNameAsync(season.Name)).ReturnsAsync(season);
        _divisionRepo
            .Setup(r => r.GetCompetitionDivisionsAsync(season.Id))
            .ReturnsAsync(Enumerable.Empty<FootballCompetitionDivision>());

        Result<FootballSeasonDto> result = await handler.Handle(
            new UpdateFootballSeasonCommand(
                season.Id,
                season.Name,
                new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Own Season");
        _footballUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActivateFootballSeason_WhenMissing_ReturnsNotFound()
    {
        ActivateFootballSeasonHandler handler = new(
            _competitionRepo.Object,
            _divisionRepo.Object,
            _clubRepo.Object,
            _commonUow.Object,
            _footballUow.Object,
            _statsRepo.Object,
            Mock.Of<ILogger<ActivateFootballSeasonHandler>>());

        Guid id = Guid.NewGuid();
        _competitionRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((FootballCompetition?)null);

        Result<FootballSeasonDto> result = await handler.Handle(
            new ActivateFootballSeasonCommand(id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task AddTeamToSeason_WhenSeasonActive_SeedsTeamSeasonStats()
    {
        AddTeamToSeasonHandler handler = new(
            _competitionRepo.Object,
            _teamRepo.Object,
            _divisionRepo.Object,
            _clubRepo.Object,
            _footballUow.Object,
            _statsRepo.Object,
            Mock.Of<ILogger<AddTeamToSeasonHandler>>());

        FootballSeason season = CreateSeason();
        season.Activate();
        FootballTeam team = CreateTeam();
        FootballPlayer player = new(Guid.NewGuid(), new FootballPositionPreference(FootballPosition.Midfielder));
        team.AddPlayer(player, FootballPosition.Midfielder, jerseyNumber: 8);

        _competitionRepo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);
        _teamRepo.Setup(r => r.GetByIdAsync((Guid?)team.Id)).ReturnsAsync(team);
        _divisionRepo
            .Setup(r => r.GetCompetitionDivisionsAsync(season.Id))
            .ReturnsAsync(Enumerable.Empty<FootballCompetitionDivision>());
        _clubRepo.Setup(r => r.GetByIdAsync(team.ClubId)).ReturnsAsync(new Club("Club"));

        Result<FootballSeasonDto> result = await handler.Handle(
            new AddFootballTeamToSeasonCommand(season.Id, team.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _statsRepo.Verify(
            r => r.SaveTeamSeasonStatisticsAsync(
                It.Is<FootballTeamSeasonStatistics>(s => s.TeamId == team.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _statsRepo.Verify(
            r => r.SavePlayerSeasonStatisticsAsync(
                It.Is<FootballPlayerSeasonStatistics>(s => s.PlayerId == player.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AddTeamToSeason_WhenTeamMissing_ReturnsNotFound()
    {
        AddTeamToSeasonHandler handler = new(
            _competitionRepo.Object,
            _teamRepo.Object,
            _divisionRepo.Object,
            _clubRepo.Object,
            _footballUow.Object,
            _statsRepo.Object,
            Mock.Of<ILogger<AddTeamToSeasonHandler>>());

        FootballSeason season = CreateSeason();
        Guid teamId = Guid.NewGuid();
        _competitionRepo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);
        _teamRepo.Setup(r => r.GetByIdAsync((Guid?)teamId)).ReturnsAsync((FootballTeam?)null);

        Result<FootballSeasonDto> result = await handler.Handle(
            new AddFootballTeamToSeasonCommand(season.Id, teamId),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("FootballTeam");
    }

    [Fact]
    public async Task DeleteFootballSeason_WhenMatchesAreUnplayed_DeletesMatchesStatsAndSeason()
    {
        DeleteFootballSeasonHandler handler = new(
            _competitionRepo.Object,
            _matchRepo.Object,
            _statsRepo.Object,
            _footballUow.Object,
            Mock.Of<ILogger<DeleteFootballSeasonHandler>>());

        Guid id = Guid.NewGuid();
        _competitionRepo.Setup(r => r.ExistsAsync(id)).ReturnsAsync(true);
        _matchRepo
            .Setup(r => r.HasMatchThatBlocksSeasonDeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _matchRepo
            .Setup(r => r.DeleteAllByCompetitionIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        Result result = await handler.Handle(new DeleteFootballSeasonCommand(id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _matchRepo.Verify(r => r.DeleteAllByCompetitionIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _statsRepo.Verify(r => r.ResetCompetitionStatisticsAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _competitionRepo.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteFootballSeason_WhenMatchHasBeenRecorded_ReturnsFailure()
    {
        DeleteFootballSeasonHandler handler = new(
            _competitionRepo.Object,
            _matchRepo.Object,
            _statsRepo.Object,
            _footballUow.Object,
            Mock.Of<ILogger<DeleteFootballSeasonHandler>>());

        Guid id = Guid.NewGuid();
        _competitionRepo.Setup(r => r.ExistsAsync(id)).ReturnsAsync(true);
        _matchRepo
            .Setup(r => r.HasMatchThatBlocksSeasonDeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Result result = await handler.Handle(new DeleteFootballSeasonCommand(id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("started, finished, or been cancelled");
        _competitionRepo.Verify(r => r.DeleteAsync(id), Times.Never);
        _statsRepo.Verify(
            r => r.ResetCompetitionStatisticsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetFootballSeasonYears_WithTeamCategory_ReturnsOnlyThatAudiencesYears()
    {
        _competitionRepo
            .Setup(r => r.GetSeasonDateSummariesAsync(TeamCategory.Women, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new FootballSeasonDateSummary(new DateTime(2014, 1, 1), new DateTime(2014, 12, 31), false),
            ]);

        GetFootballSeasonYearsHandler handler = new(
            _competitionRepo.Object,
            Mock.Of<ILogger<GetFootballSeasonYearsHandler>>());

        Result<IEnumerable<FootballSeasonYearDto>> result = await handler.Handle(
            new GetFootballSeasonYearsQuery(TeamCategory.Women),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        List<FootballSeasonYearDto> years = result.Data!.ToList();
        years.Should().ContainSingle();
        years[0].Year.Should().Be("2014");
        years[0].HasActiveSeason.Should().BeFalse();
    }
}
