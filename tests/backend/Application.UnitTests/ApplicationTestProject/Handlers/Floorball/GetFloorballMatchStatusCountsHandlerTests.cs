using Application.Common;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Floorball.Matches.Handlers;
using Application.Features.Floorball.Matches.Queries;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Floorball;

public class GetFloorballMatchStatusCountsHandlerTests
{
    [Fact]
    public async Task Handle_GroupedCounts_MapsEachStatusAndTotal()
    {
        Guid competitionId = Guid.NewGuid();
        Mock<IFloorballMatchRepository> matches = new();
        matches
            .Setup(repository => repository.GetStatusCountsAsync(
                competitionId,
                "hifk",
                FloorballCompetitionType.Season,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<FloorballMatchStatus, int>
            {
                [FloorballMatchStatus.Scheduled] = 4,
                [FloorballMatchStatus.InProgress] = 1,
                [FloorballMatchStatus.Completed] = 7,
                [FloorballMatchStatus.Postponed] = 2,
            });

        GetFloorballMatchStatusCountsHandler handler = new(
            matches.Object,
            Mock.Of<ILogger<GetFloorballMatchStatusCountsHandler>>());

        Result<MatchStatusCountsDto> result = await handler.Handle(
            new GetFloorballMatchStatusCountsQuery(competitionId, "hifk", FloorballCompetitionType.Season, IncludeDrafts: true),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(new MatchStatusCountsDto(
            Total: 14,
            Scheduled: 4,
            Postponed: 2,
            InProgress: 1,
            Completed: 7,
            Cancelled: 0));
    }
}
