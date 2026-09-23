using Application.Common;
using Application.Features.Common.Organization.PlayerLicences.Commands;
using Application.Features.Common.Organization.PlayerLicences.DTOs;
using Application.Features.Common.Organization.PlayerLicences.Handlers;
using Application.Interfaces.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Football;
using Domain.Repositories.Hockey;
using Microsoft.Extensions.Logging;
using Moq;
using SiteSettingsEntity = Domain.Entities.Common.SiteSettings;

namespace ApplicationTestProject.Handlers.Common;

public class ResetExpiredPlayerLicencesHandlerTests
{
    private readonly Mock<ISiteSettingsRepository> _settingsRepo = new();
    private readonly Mock<ISiteSettingsProvider> _provider = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IFloorballTeamRepository> _floorball = new();
    private readonly Mock<IFootballTeamRepository> _football = new();
    private readonly Mock<IHockeyTeamRepository> _hockey = new();

    private ResetExpiredPlayerLicencesHandler CreateHandler(DateTime utcNow)
    {
        _floorball
            .Setup(x => x.DeactivateOpenPlayerLicencesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);
        _football
            .Setup(x => x.DeactivateOpenPlayerLicencesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);
        _hockey
            .Setup(x => x.DeactivateOpenPlayerLicencesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return new ResetExpiredPlayerLicencesHandler(
            _settingsRepo.Object,
            _provider.Object,
            _uow.Object,
            _floorball.Object,
            _football.Object,
            _hockey.Object,
            new FixedTimeProvider(new DateTimeOffset(utcNow, TimeSpan.Zero)),
            Mock.Of<ILogger<ResetExpiredPlayerLicencesHandler>>());
    }

    [Fact]
    public async Task Handle_BeforeCutoff_DoesNotDeactivate()
    {
        SiteSettingsEntity settings = new(Guid.NewGuid(), 15, 7, 10, 5, 5, 5, 1);
        settings.MarkPlayerLicenceReset(2025);
        _settingsRepo.Setup(x => x.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(settings);

        ResetExpiredPlayerLicencesHandler handler = CreateHandler(
            new DateTime(2026, 4, 30, 10, 0, 0, DateTimeKind.Utc));

        Result<PlayerLicenceResetResultDto> result = await handler.Handle(
            new ResetExpiredPlayerLicencesCommand(Force: false),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Ran.Should().BeFalse();
        _floorball.Verify(x => x.DeactivateOpenPlayerLicencesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AfterCutoffAndNotYetRun_Deactivates()
    {
        SiteSettingsEntity settings = new(Guid.NewGuid(), 15, 7, 10, 5, 5, 5, 1);
        settings.MarkPlayerLicenceReset(2025);
        _settingsRepo.Setup(x => x.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(settings);

        ResetExpiredPlayerLicencesHandler handler = CreateHandler(
            new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc));

        Result<PlayerLicenceResetResultDto> result = await handler.Handle(
            new ResetExpiredPlayerLicencesCommand(Force: false),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Ran.Should().BeTrue();
        result.Data.FloorballDeactivated.Should().Be(3);
        result.Data.FootballDeactivated.Should().Be(2);
        result.Data.HockeyDeactivated.Should().Be(1);
        settings.LastPlayerLicenceResetYear.Should().Be(2026);
        _provider.Verify(x => x.Invalidate(), Times.Once);
    }

    [Fact]
    public async Task Handle_AlreadyRunThisYear_IsNoOp()
    {
        SiteSettingsEntity settings = new(Guid.NewGuid(), 15, 7, 10, 5, 5, 5, 1);
        settings.MarkPlayerLicenceReset(2026);
        _settingsRepo.Setup(x => x.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(settings);

        ResetExpiredPlayerLicencesHandler handler = CreateHandler(
            new DateTime(2026, 9, 21, 10, 0, 0, DateTimeKind.Utc));

        Result<PlayerLicenceResetResultDto> result = await handler.Handle(
            new ResetExpiredPlayerLicencesCommand(Force: false),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Ran.Should().BeFalse();
        _floorball.Verify(x => x.DeactivateOpenPlayerLicencesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NullLastYear_InitializesWithoutDeactivating()
    {
        SiteSettingsEntity settings = new(Guid.NewGuid(), 15, 7, 10, 5, 5, 5, 1);
        _settingsRepo.Setup(x => x.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(settings);

        ResetExpiredPlayerLicencesHandler handler = CreateHandler(
            new DateTime(2026, 9, 21, 10, 0, 0, DateTimeKind.Utc));

        Result<PlayerLicenceResetResultDto> result = await handler.Handle(
            new ResetExpiredPlayerLicencesCommand(Force: false),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Ran.Should().BeFalse();
        settings.LastPlayerLicenceResetYear.Should().Be(2026);
        _floorball.Verify(x => x.DeactivateOpenPlayerLicencesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Force_DeactivatesEvenWhenAlreadyRun()
    {
        SiteSettingsEntity settings = new(Guid.NewGuid(), 15, 7, 10, 5, 5, 5, 1);
        settings.MarkPlayerLicenceReset(2026);
        _settingsRepo.Setup(x => x.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(settings);

        ResetExpiredPlayerLicencesHandler handler = CreateHandler(
            new DateTime(2026, 9, 21, 10, 0, 0, DateTimeKind.Utc));

        Result<PlayerLicenceResetResultDto> result = await handler.Handle(
            new ResetExpiredPlayerLicencesCommand(Force: true),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Ran.Should().BeTrue();
        _floorball.Verify(x => x.DeactivateOpenPlayerLicencesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _football.Verify(x => x.DeactivateOpenPlayerLicencesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _hockey.Verify(x => x.DeactivateOpenPlayerLicencesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
