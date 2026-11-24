using Application.Common;
using Application.Features.Football.Statistics.DTOs;
using Application.Features.Football.Statistics.Handlers;
using Application.Features.Football.Statistics.Queries;
using Domain.Entities.Football.Statistics;
using Domain.Repositories.Football;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Football;

public class GetFootballMatchStatisticsHandlerTests
{
    [Fact]
    public async Task Handle_WhenMatchIsMissing_ReturnsEmptySuccess()
    {
        Mock<IFootballMatchRepository> repository = new();
        Guid matchId = Guid.NewGuid();

        GetMatchStatisticsHandler handler = new(repository.Object);

        Result<List<FootballMatchTeamStatisticsDto>> result = await handler.Handle(
            new GetFootballMatchStatisticsQuery(matchId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }
}
