using Application.Common;
using Application.Features.Floorball.Seasons.Commands;
using Application.Features.Floorball.Seasons.Handlers;
using Application.Features.Football.Seasons.Commands;
using Application.Features.Football.Seasons.Handlers;
using Application.Features.Hockey.Seasons.Commands;
using Application.Features.Hockey.Seasons.Handlers;
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Hockey.Competitions;
using Domain.Enums.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Football;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Common;

/// <summary>
/// Moving a season to another audience group works in every sport, also for completed seasons.
/// </summary>
public class ChangeSeasonTeamCategoryHandlerTests
{
    private static readonly DateTime Start = new(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = new(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Floorball_CompletedSeason_ChangesCategory()
    {
        FloorballSeason season = new("Puumaliiga 2025-2026", Start, End);
        season.Complete();
        Mock<IFloorballCompetitionRepository> repo = new();
        Mock<IFloorballUnitOfWork> unitOfWork = new();
        repo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);
        ChangeFloorballSeasonTeamCategoryHandler handler = new(
            repo.Object, unitOfWork.Object, Mock.Of<ILogger<ChangeFloorballSeasonTeamCategoryHandler>>());

        Result result = await handler.Handle(
            new ChangeFloorballSeasonTeamCategoryCommand(season.Id, TeamCategory.Women), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        season.TeamCategory.Should().Be(TeamCategory.Women);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Floorball_MissingSeason_ReturnsNotFound()
    {
        Mock<IFloorballCompetitionRepository> repo = new();
        Mock<IFloorballUnitOfWork> unitOfWork = new();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid?>())).ReturnsAsync((FloorballCompetition?)null);
        ChangeFloorballSeasonTeamCategoryHandler handler = new(
            repo.Object, unitOfWork.Object, Mock.Of<ILogger<ChangeFloorballSeasonTeamCategoryHandler>>());

        Result result = await handler.Handle(
            new ChangeFloorballSeasonTeamCategoryCommand(Guid.NewGuid(), TeamCategory.Youth), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorKind.Should().Be(ResultErrorKind.NotFound);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Football_CompletedSeason_ChangesCategory()
    {
        FootballSeason season = new("Talvifutis 2025-2026", Start, End);
        season.Complete();
        Mock<IFootballCompetitionRepository> repo = new();
        Mock<IFootballUnitOfWork> unitOfWork = new();
        repo.Setup(r => r.GetByIdAsync(season.Id)).ReturnsAsync(season);
        ChangeFootballSeasonTeamCategoryHandler handler = new(
            repo.Object, unitOfWork.Object, Mock.Of<ILogger<ChangeFootballSeasonTeamCategoryHandler>>());

        Result result = await handler.Handle(
            new ChangeFootballSeasonTeamCategoryCommand(season.Id, TeamCategory.Youth), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        season.TeamCategory.Should().Be(TeamCategory.Youth);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Hockey_CompletedSeason_ChangesCategory()
    {
        HockeySeason season = new("Liiga 2025-2026", Start, End);
        season.Publish();
        season.Activate();
        season.Complete();
        Mock<IHockeyCompetitionRepository> repo = new();
        Mock<IHockeyUnitOfWork> unitOfWork = new();
        repo.Setup(r => r.GetSeasonByIdAsync(season.Id)).ReturnsAsync(season);
        ChangeHockeySeasonTeamCategoryHandler handler = new(
            repo.Object, unitOfWork.Object, Mock.Of<ILogger<ChangeHockeySeasonTeamCategoryHandler>>());

        Result result = await handler.Handle(
            new ChangeHockeySeasonTeamCategoryCommand(season.Id, TeamCategory.Women), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        season.TeamCategory.Should().Be(TeamCategory.Women);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
