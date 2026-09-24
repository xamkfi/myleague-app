using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Seasons.Handlers;
using Application.Features.Hockey.Seasons.Queries;
using Domain.Entities.Hockey.Competitions;
using Domain.Repositories.Hockey;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Hockey;

public class DraftCompetitionVisibilityHandlerTests
{
    private readonly Mock<IHockeyCompetitionRepository> _competitionRepo = new();

    private static HockeySeason CreateSeason(string name) =>
        new(
            name,
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 4, 30, 0, 0, 0, DateTimeKind.Utc),
            name);

    [Fact]
    public async Task GetAll_HidesDraft_ShowsItWhenIncludeDrafts_ActiveVisibleEitherWay()
    {
        HockeySeason draft = CreateSeason("Draft 2026-2027");
        HockeySeason active = CreateSeason("Active 2026-2027");
        active.Publish();
        active.Activate();

        _competitionRepo
            .Setup(repository => repository.GetAllSeasonsAsync())
            .ReturnsAsync(new List<HockeySeason> { draft, active });

        GetAllHockeySeasonsHandler handler = new(
            _competitionRepo.Object,
            Mock.Of<ILogger<GetAllHockeySeasonsHandler>>());

        Result<IEnumerable<HockeySeasonDto>> publicResult = await handler.Handle(
            new GetAllHockeySeasonsQuery(),
            CancellationToken.None);
        Result<IEnumerable<HockeySeasonDto>> adminResult = await handler.Handle(
            new GetAllHockeySeasonsQuery(IncludeDrafts: true),
            CancellationToken.None);

        publicResult.IsSuccess.Should().BeTrue();
        publicResult.Data!.Select(season => season.Id).Should().Equal(active.Id);

        adminResult.IsSuccess.Should().BeTrue();
        adminResult.Data!.Select(season => season.Id).Should().BeEquivalentTo(new[] { draft.Id, active.Id });
    }

    [Fact]
    public async Task GetById_DraftIsNotFound_UnlessIncludeDrafts()
    {
        HockeySeason draft = CreateSeason("Draft 2026-2027");
        _competitionRepo
            .Setup(repository => repository.GetSeasonByIdAsync(draft.Id))
            .ReturnsAsync(draft);

        GetHockeySeasonByIdHandler handler = new(
            _competitionRepo.Object,
            Mock.Of<ILogger<GetHockeySeasonByIdHandler>>());

        Result<HockeySeasonDto> publicResult = await handler.Handle(
            new GetHockeySeasonByIdQuery(draft.Id),
            CancellationToken.None);
        Result<HockeySeasonDto> adminResult = await handler.Handle(
            new GetHockeySeasonByIdQuery(draft.Id, IncludeDrafts: true),
            CancellationToken.None);

        publicResult.IsSuccess.Should().BeFalse();
        publicResult.ErrorKind.Should().Be(ResultErrorKind.NotFound);
        adminResult.IsSuccess.Should().BeTrue();
        adminResult.Data!.Id.Should().Be(draft.Id);
    }

    [Fact]
    public async Task GetAll_ShowsEndedDraftSeasonAsHistory()
    {
        HockeySeason history = new(
            "Imported 2015-2016",
            new DateTime(2015, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2016, 4, 7, 0, 0, 0, DateTimeKind.Utc),
            "2015-2016");
        HockeySeason futureDraft = CreateSeason("Draft 2026-2027");

        _competitionRepo
            .Setup(repository => repository.GetAllSeasonsAsync())
            .ReturnsAsync(new List<HockeySeason> { history, futureDraft });

        GetAllHockeySeasonsHandler handler = new(
            _competitionRepo.Object,
            Mock.Of<ILogger<GetAllHockeySeasonsHandler>>());

        Result<IEnumerable<HockeySeasonDto>> publicResult = await handler.Handle(
            new GetAllHockeySeasonsQuery(),
            CancellationToken.None);

        publicResult.IsSuccess.Should().BeTrue();
        publicResult.Data!.Select(season => season.Id).Should().Equal(history.Id);
    }
}
