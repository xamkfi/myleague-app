using Application.Common;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Statistics.Handlers;
using Application.Features.Floorball.Statistics.Queries;
using Domain.Entities.Common;
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;
using Domain.Enums.Common;
using Domain.Enums.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.ValueObjects.Floorball;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Floorball;

public class GetTeamGoalieStatisticsHandlerTests
{
    private readonly Mock<IFloorballStatisticsRepository> _statistics = new();
    private readonly Mock<IPersonRepository> _persons = new();

    [Fact]
    public async Task Handle_NoGoalieRows_ReturnsEmptyListWithoutLoadingPersons()
    {
        Guid teamId = Guid.NewGuid();
        Guid competitionId = Guid.NewGuid();
        _statistics
            .Setup(repository => repository.GetGoalieStatisticsByTeamAndCompetitionAsync(teamId, competitionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FloorballGoalieSeasonStatistics>());

        Result<List<FloorballGoalieSeasonStatisticsDto>> result =
            await CreateHandler().Handle(new GetFloorballTeamGoalieStatisticsQuery(competitionId, teamId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
        _persons.Verify(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_GoalieRows_MapsPlayerNameAndSavePercentage()
    {
        Person person = new("Niko", "Maalivahti");
        FloorballPlayer player = new(person.Id, new Position(FloorballPosition.Goalkeeper));
        FloorballTeam team = CreateTeam();
        FloorballSeason season = CreateSeason();
        FloorballGoalieSeasonStatistics row = new(player.Id, team.Id, season.Id);
        typeof(FloorballGoalieSeasonStatistics).GetProperty("Player")!.SetValue(row, player);
        typeof(FloorballGoalieSeasonStatistics).GetProperty("Team")!.SetValue(row, team);
        typeof(FloorballGoalieSeasonStatistics).GetProperty("Competition")!.SetValue(row, season);
        row.RecordGamePlayed(wasStarter: true, FloorballGameResult.Win, minutesPlayed: 60);
        row.RecordSaves(saves: 27, shotsAgainst: 30, goalsAllowed: 3);

        _statistics
            .Setup(repository => repository.GetGoalieStatisticsByTeamAndCompetitionAsync(team.Id, season.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FloorballGoalieSeasonStatistics> { row });
        _persons
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Person> { person });

        Result<List<FloorballGoalieSeasonStatisticsDto>> result =
            await CreateHandler().Handle(new GetFloorballTeamGoalieStatisticsQuery(season.Id, team.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        FloorballGoalieSeasonStatisticsDto dto = result.Data!.Should().ContainSingle().Subject;
        dto.PlayerId.Should().Be(player.Id);
        dto.PlayerName.Should().Be(person.FullName);
        dto.GamesPlayed.Should().Be(1);
        dto.SavePercentage.Should().Be(90m);
    }

    private GetTeamGoalieStatisticsHandler CreateHandler() => new(
        _statistics.Object,
        _persons.Object,
        Mock.Of<ILogger<GetTeamGoalieStatisticsHandler>>());

    private static FloorballSeason CreateSeason()
    {
        return new FloorballSeason(
            "Test Season",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc));
    }

    private static FloorballTeam CreateTeam()
    {
        Club club = new("Test Club");
        return new FloorballTeam(
            "Wolves",
            divisionId: null,
            club,
            homeArena: "Arena",
            primaryJerseyColor: "Blue",
            teamCategory: TeamCategory.Adult);
    }
}
