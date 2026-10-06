using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Handlers;
using Application.Features.Hockey.Matches.Queries;
using Domain.Entities.Hockey.Matches;
using Domain.Enums.Hockey.Matches;
using Domain.Repositories.Hockey;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Hockey;

public class GetHockeyLiveMatchesHandlerTests
{
    [Fact]
    public async Task Handle_MapsSlimRowsAndPassesPollingWindow()
    {
        Guid competitionId = Guid.NewGuid();
        Guid homeTeamId = Guid.NewGuid();
        HockeyMatch match = new(DateTime.UtcNow.AddMinutes(30), HockeyMatchType.Friendly, venue: "Arena");
        match.AssignMatchTeam(homeTeamId, HockeyTeamSlot.Home);

        DateTime before = DateTime.UtcNow;
        Mock<IHockeyMatchRepository> repository = new();
        repository
            .Setup(repo => repo.GetLiveAsync(
                competitionId,
                It.Is<DateTime>(until => until >= before.AddHours(2)),
                It.Is<DateTime>(since => since <= DateTime.UtcNow.AddMinutes(-10)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HockeyMatch> { match });

        GetHockeyLiveMatchesHandler handler = new(
            repository.Object,
            Mock.Of<ILogger<GetHockeyLiveMatchesHandler>>());

        Result<IEnumerable<HockeyLiveMatchDto>> result = await handler.Handle(
            new GetHockeyLiveMatchesQuery(competitionId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        HockeyLiveMatchDto row = result.Data!.Should().ContainSingle().Subject;
        row.Id.Should().Be(match.Id);
        row.Status.Should().Be(nameof(HockeyMatchStatus.Scheduled));
        row.HomeTeamId.Should().Be(homeTeamId);
        row.HomeScore.Should().Be(0);
    }
}
