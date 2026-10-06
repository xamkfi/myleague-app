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

public class GetHockeyUpcomingMatchesForTeamsHandlerTests
{
    [Fact]
    public async Task Handle_PassesDistinctTeamsAndMapsMatches()
    {
        Guid firstTeam = Guid.NewGuid();
        Guid secondTeam = Guid.NewGuid();
        DateTime from = DateTime.UtcNow.Date;
        HockeyMatch match = new(DateTime.UtcNow.AddDays(2), HockeyMatchType.Friendly, venue: "Arena");
        match.AssignMatchTeam(firstTeam, HockeyTeamSlot.Home);

        Mock<IHockeyMatchRepository> repository = new();
        repository
            .Setup(repo => repo.GetScheduledForTeamsAsync(
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 2 && ids.Contains(firstTeam) && ids.Contains(secondTeam)),
                from,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HockeyMatch> { match });

        GetHockeyUpcomingMatchesForTeamsHandler handler = new(
            repository.Object,
            Mock.Of<ILogger<GetHockeyUpcomingMatchesForTeamsHandler>>());

        Result<IEnumerable<HockeyMatchDto>> result = await handler.Handle(
            new GetHockeyUpcomingMatchesForTeamsQuery([firstTeam, secondTeam, secondTeam], from),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        HockeyMatchDto dto = result.Data!.Should().ContainSingle().Subject;
        dto.Id.Should().Be(match.Id);
        dto.HomeTeamId.Should().Be(firstTeam);
    }
}
