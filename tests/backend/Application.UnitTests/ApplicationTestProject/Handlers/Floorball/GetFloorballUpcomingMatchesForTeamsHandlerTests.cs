using Application.Common;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Handlers;
using Application.Features.Floorball.Matches.Queries;
using Domain.Common;
using Domain.Entities.Floorball.Matches;
using Domain.Enums.Common;
using Domain.Enums.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Floorball;

public class GetFloorballUpcomingMatchesForTeamsHandlerTests
{
    [Fact]
    public async Task Handle_QueriesScheduledMatchesForDistinctTeams_EarliestFirst()
    {
        Guid firstTeam = Guid.NewGuid();
        Guid secondTeam = Guid.NewGuid();
        DateTime from = new(2026, 10, 6, 0, 0, 0, DateTimeKind.Utc);
        IReadOnlyCollection<Guid>? requestedTeams = null;
        Mock<IFloorballMatchRepository> matches = new();
        matches
            .Setup(repository => repository.GetPagedAsync(
                1,
                GetFloorballUpcomingMatchesForTeamsQuery.MaxMatchesPerTeam * 2,
                null,
                null,
                from,
                null,
                FloorballMatchStatus.Scheduled,
                "asc",
                null,
                null,
                null,
                null,
                true,
                false,
                It.IsAny<IReadOnlyCollection<Guid>?>(),
                It.IsAny<CancellationToken>()))
            .Callback((int _, int _, Guid? _, Guid? _, DateTime? _, DateTime? _, FloorballMatchStatus? _, string _,
                string? _, Guid? _, FloorballCompetitionType? _, TeamCategory? _, bool _, bool _,
                IReadOnlyCollection<Guid>? teamIds, CancellationToken _) => requestedTeams = teamIds)
            .ReturnsAsync(PagedResult.Create(new List<FloorballMatch>(), 0, 1, 200));

        GetFloorballUpcomingMatchesForTeamsHandler handler = new(
            matches.Object,
            Mock.Of<IClubRepository>(),
            Mock.Of<ILogger<GetFloorballUpcomingMatchesForTeamsHandler>>());

        Result<IEnumerable<FloorballMatchDto>> result = await handler.Handle(
            new GetFloorballUpcomingMatchesForTeamsQuery([firstTeam, secondTeam, firstTeam, Guid.Empty], from),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
        requestedTeams.Should().BeEquivalentTo([firstTeam, secondTeam]);
    }

    [Fact]
    public async Task Handle_NoTeams_ReturnsEmptyWithoutQuerying()
    {
        Mock<IFloorballMatchRepository> matches = new(MockBehavior.Strict);
        GetFloorballUpcomingMatchesForTeamsHandler handler = new(
            matches.Object,
            Mock.Of<IClubRepository>(),
            Mock.Of<ILogger<GetFloorballUpcomingMatchesForTeamsHandler>>());

        Result<IEnumerable<FloorballMatchDto>> result = await handler.Handle(
            new GetFloorballUpcomingMatchesForTeamsQuery([], DateTime.UtcNow.Date),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
