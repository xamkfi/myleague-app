using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Handlers;
using Domain.Entities.Hockey.Matches;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Hockey;

public class ImportHockeyMatchEventsHandlerTests
{
    [Fact]
    public async Task Handle_WhenMatchMissing_ReturnsNotFound()
    {
        Mock<IHockeyMatchRepository> matchRepo = new();
        ImportHockeyMatchEventsHandler handler = new(
            matchRepo.Object,
            Mock.Of<IHockeyUnitOfWork>(),
            Mock.Of<ILogger<ImportHockeyMatchEventsHandler>>());

        Guid matchId = Guid.NewGuid();
        matchRepo.Setup(r => r.GetByIdAsync(matchId)).ReturnsAsync((HockeyMatch?)null);

        Result<HockeyMatchEventsImportDto> result = await handler.Handle(
            new ImportHockeyMatchEventsCommand(
                matchId,
                [new ImportHockeyMatchEventItem(
                    "Goal",
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    null,
                    null,
                    null,
                    1,
                    10,
                    null,
                    false,
                    null,
                    null,
                    null,
                    null,
                    null,
                    false)]),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("HockeyMatch");
    }
}
