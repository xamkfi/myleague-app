using Application.Common;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Handlers;
using Application.Interfaces.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Floorball;

public class ImportFloorballMatchEventsHandlerTests
{
    [Fact]
    public async Task Handle_WhenMatchMissing_ReturnsNotFound()
    {
        Mock<IFloorballMatchRepository> matchRepo = new();
        ImportFloorballMatchEventsHandler handler = new(
            matchRepo.Object,
            Mock.Of<IFloorballTeamRepository>(),
            Mock.Of<IFloorballPlayerRepository>(),
            Mock.Of<IFloorballStatisticsRepository>(),
            Mock.Of<IFloorballUnitOfWork>(),
            Mock.Of<INotificationSenderService>(),
            Mock.Of<ILogger<ImportFloorballMatchEventsHandler>>());

        Guid matchId = Guid.NewGuid();
        matchRepo.Setup(r => r.GetByIdAsync(matchId)).ReturnsAsync((FloorballMatch?)null);

        Result<FloorballMatchEventsImportDto> result = await handler.Handle(
            new ImportFloorballMatchEventsCommand(
                matchId,
                [new ImportFloorballMatchEventItem(
                    "Goal",
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    null,
                    null,
                    1,
                    10,
                    null,
                    null,
                    null,
                    null)]),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("FloorballMatch");
    }
}
