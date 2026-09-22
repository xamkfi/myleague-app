using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Handlers;
using Application.Features.Hockey.Matches.Queries;
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Hockey.Matches;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Teams;
using Domain.Repositories.Hockey;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Hockey;

public class GetHockeyMatchesHandlerTests
{
    private readonly Mock<IHockeyMatchRepository> _matchRepo = new();
    private readonly Mock<IHockeyTeamRepository> _teamRepo = new();
    private readonly Mock<IPaginationService> _pagination = new();

    public GetHockeyMatchesHandlerTests()
    {
        _pagination
            .Setup(service => service.ResolvePageSize(It.IsAny<string>(), It.IsAny<int>()))
            .Returns<string, int>((_, size) => size == 0 ? 25 : size);
    }

    [Fact]
    public async Task Handle_PassesDateAndTeamCategoryFilters()
    {
        DateTime start = new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime end = new(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc);
        HockeyMatch match = new(start.AddDays(2), HockeyMatchType.League, venue: "Arena");

        _matchRepo
            .Setup(repository => repository.GetPagedAsync(
                1,
                100,
                null,
                null,
                start,
                end,
                null,
                "asc",
                null,
                TeamCategory.Youth,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(PagedResult.Create(new List<HockeyMatch> { match }, 1, 1, 100));

        GetHockeyMatchesHandler handler = CreateHandler();

        Result<PagedResult<HockeyMatchListDto>> result = await handler.Handle(
            new GetHockeyMatchesQuery(1, 100, start, end, TeamCategory.Youth, "asc"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().ContainSingle(item => item.Id == match.Id && item.Venue == "Arena");
        _teamRepo.Verify(
            repository => repository.GetNamesByIdsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_MapsTeamNamesFromLookup()
    {
        Guid homeTeamId = Guid.NewGuid();
        Guid awayTeamId = Guid.NewGuid();
        HockeyMatch match = new(
            new DateTime(2026, 9, 10, 18, 0, 0, DateTimeKind.Utc),
            HockeyMatchType.League,
            venue: "Nokia Arena");
        match.AssignMatchTeam(homeTeamId, HockeyTeamSlot.Home);
        match.AssignMatchTeam(awayTeamId, HockeyTeamSlot.Away);

        _matchRepo
            .Setup(repository => repository.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<HockeyMatchStatus?>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<TeamCategory?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(PagedResult.Create(new List<HockeyMatch> { match }, 1, 1, 25));

        _teamRepo
            .Setup(repository => repository.GetNamesByIdsAsync(
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(homeTeamId) && ids.Contains(awayTeamId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, string>
            {
                [homeTeamId] = "HIFK",
                [awayTeamId] = "Tappara",
            });

        GetHockeyMatchesHandler handler = CreateHandler();

        Result<PagedResult<HockeyMatchListDto>> result = await handler.Handle(
            new GetHockeyMatchesQuery(),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        HockeyMatchListDto item = result.Data!.Items.Should().ContainSingle().Subject;
        item.HomeTeamName.Should().Be("HIFK");
        item.AwayTeamName.Should().Be("Tappara");
        item.HomeTeamId.Should().Be(homeTeamId);
        item.AwayTeamId.Should().Be(awayTeamId);
    }

    private GetHockeyMatchesHandler CreateHandler() =>
        new(
            _matchRepo.Object,
            _teamRepo.Object,
            _pagination.Object,
            Mock.Of<ILogger<GetHockeyMatchesHandler>>());
}
