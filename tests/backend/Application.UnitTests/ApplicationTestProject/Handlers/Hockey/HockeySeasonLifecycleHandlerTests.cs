using Application.Common;
using Application.Features.Hockey.Seasons.Commands;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Seasons.Handlers;
using Domain.Entities.Hockey.Competitions;
using Domain.Enums.Hockey.Competitions;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Hockey;

public class HockeySeasonLifecycleHandlerTests
{
    private readonly Mock<IHockeyCompetitionRepository> _competitionRepo = new();
    private readonly Mock<IHockeyUnitOfWork> _unitOfWork = new();

    private static HockeySeason CreateSeason() =>
        new(
            "Liiga 2026-27",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 4, 30, 0, 0, 0, DateTimeKind.Utc),
            "2026-27");

    [Fact]
    public async Task Publish_ValidDraft_Saves()
    {
        HockeySeason season = CreateSeason();
        _competitionRepo.Setup(r => r.GetSeasonByIdAsync(season.Id)).ReturnsAsync(season);

        PublishHockeySeasonHandler handler = new(
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<PublishHockeySeasonHandler>>());

        Result<HockeySeasonDto> result = await handler.Handle(
            new PublishHockeySeasonCommand(season.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Status.Should().Be(HockeyCompetitionStatus.Published.ToString());
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_ValidRequest_UpdatesNameAndCode()
    {
        HockeySeason season = CreateSeason();
        _competitionRepo.Setup(r => r.GetSeasonByIdAsync(season.Id)).ReturnsAsync(season);

        UpdateHockeySeasonHandler handler = new(
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<UpdateHockeySeasonHandler>>());

        Result<HockeySeasonDto> result = await handler.Handle(
            new UpdateHockeySeasonCommand(
                season.Id,
                "Updated Liiga",
                new DateTime(2026, 9, 15, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2027, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                "26-27"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Updated Liiga");
        result.Data.SeasonCode.Should().Be("26-27");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetChampion_WhenNotCompleted_Fails()
    {
        HockeySeason season = CreateSeason();
        Guid competitionTeamId = Guid.NewGuid();
        _competitionRepo.Setup(r => r.GetSeasonByIdAsync(season.Id)).ReturnsAsync(season);

        SetHockeySeasonChampionHandler handler = new(
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<SetHockeySeasonChampionHandler>>());

        Result<HockeySeasonDto> result = await handler.Handle(
            new SetHockeySeasonChampionCommand(season.Id, competitionTeamId),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("completed");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetChampion_WhenCompleted_SetsChampion()
    {
        HockeySeason season = CreateSeason();
        season.Publish();
        season.Activate();
        season.Complete();
        Guid competitionTeamId = Guid.NewGuid();
        _competitionRepo.Setup(r => r.GetSeasonByIdAsync(season.Id)).ReturnsAsync(season);

        SetHockeySeasonChampionHandler handler = new(
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<SetHockeySeasonChampionHandler>>());

        Result<HockeySeasonDto> result = await handler.Handle(
            new SetHockeySeasonChampionCommand(season.Id, competitionTeamId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.ChampionCompetitionTeamId.Should().Be(competitionTeamId);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Publish_SeasonNotFound_ReturnsNotFound()
    {
        Guid seasonId = Guid.NewGuid();
        _competitionRepo.Setup(r => r.GetSeasonByIdAsync(seasonId)).ReturnsAsync((HockeySeason?)null);

        PublishHockeySeasonHandler handler = new(
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<PublishHockeySeasonHandler>>());

        Result<HockeySeasonDto> result = await handler.Handle(
            new PublishHockeySeasonCommand(seasonId),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task CreatePlayoffSeries_ValidRequest_AddsSeries()
    {
        HockeySeason season = CreateSeason();
        _competitionRepo.Setup(r => r.GetSeasonByIdAsync(season.Id)).ReturnsAsync(season);

        CreateHockeySeasonPlayoffSeriesHandler handler = new(
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<CreateHockeySeasonPlayoffSeriesHandler>>());

        Result<HockeySeasonDto> result = await handler.Handle(
            new CreateHockeySeasonPlayoffSeriesCommand(season.Id, HockeyPlayoffRound.SemiFinal, 0, 3),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.PlayoffSeries.Should().ContainSingle(
            s => s.Round == HockeyPlayoffRound.SemiFinal.ToString() && s.BestOf == 3);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveTeam_ExistingTeam_RemovesAndSaves()
    {
        HockeySeason season = CreateSeason();
        Guid teamId = Guid.NewGuid();
        season.AddTeam(teamId);
        _competitionRepo.Setup(r => r.GetSeasonByIdAsync(season.Id)).ReturnsAsync(season);

        RemoveTeamFromHockeySeasonHandler handler = new(
            _competitionRepo.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<RemoveTeamFromHockeySeasonHandler>>());

        Result<HockeySeasonDto> result = await handler.Handle(
            new RemoveTeamFromHockeySeasonCommand(season.Id, teamId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Teams.Should().NotContain(t => t.TeamId == teamId && t.IsActive);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
