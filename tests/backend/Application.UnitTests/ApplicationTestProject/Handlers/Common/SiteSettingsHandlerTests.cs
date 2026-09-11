using Application.Common;
using Application.Features.Common.SiteSettings.Commands;
using Application.Features.Common.SiteSettings.DTOs;
using Application.Features.Common.SiteSettings.Queries;
using Application.Interfaces.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Moq;
using SiteSettingsEntity = Domain.Entities.Common.SiteSettings;

namespace ApplicationTestProject.Handlers.Common;

public class SiteSettingsHandlerTests
{
    private readonly Mock<ISiteSettingsRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ISiteSettingsProvider> _provider = new();

    [Fact]
    public async Task Get_WhenNotPersisted_ReturnsFallback()
    {
        _provider
            .Setup(p => p.GetEffectiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveAuthSettings(15, 7, 10, 5, 5, false));

        GetSiteSettingsQueryHandler handler = new(_provider.Object);

        Result<SiteSettingsDto> result = await handler.Handle(
            new GetSiteSettingsQuery(),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.IsPersisted.Should().BeFalse();
        result.Data.AccessTokenExpirationMinutes.Should().Be(15);
        result.Data.SessionExpiryWarningMinutes.Should().Be(5);
    }

    [Fact]
    public async Task Get_WhenPersisted_ReturnsRowValues()
    {
        _provider
            .Setup(p => p.GetEffectiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveAuthSettings(20, 14, 8, 6, 4, true));

        GetSiteSettingsQueryHandler handler = new(_provider.Object);

        Result<SiteSettingsDto> result = await handler.Handle(
            new GetSiteSettingsQuery(),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.IsPersisted.Should().BeTrue();
        result.Data.AccessTokenExpirationMinutes.Should().Be(20);
        result.Data.RefreshTokenExpirationDays.Should().Be(14);
    }

    [Fact]
    public async Task Update_WhenNoRow_CreatesEntity()
    {
        _repo.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((SiteSettingsEntity?)null);

        UpdateSiteSettingsCommandHandler handler = new(_repo.Object, _uow.Object, _provider.Object);

        Result<SiteSettingsDto> result = await handler.Handle(
            new UpdateSiteSettingsCommand(20, 14, 8, 6, 4),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.IsPersisted.Should().BeTrue();
        result.Data.AccessTokenExpirationMinutes.Should().Be(20);
        _repo.Verify(r => r.AddAsync(It.IsAny<SiteSettingsEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _provider.Verify(p => p.Invalidate(), Times.Once);
    }

    [Fact]
    public async Task Update_WhenRowExists_UpdatesEntity()
    {
        SiteSettingsEntity existing = new(Guid.NewGuid(), 15, 7, 10, 5, 5);
        _repo.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        UpdateSiteSettingsCommandHandler handler = new(_repo.Object, _uow.Object, _provider.Object);

        Result<SiteSettingsDto> result = await handler.Handle(
            new UpdateSiteSettingsCommand(30, 21, 12, 7, 3),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        existing.AccessTokenExpirationMinutes.Should().Be(30);
        existing.RefreshTokenExpirationDays.Should().Be(21);
        _repo.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        _provider.Verify(p => p.Invalidate(), Times.Once);
    }
}
