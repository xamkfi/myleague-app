using Application.Common;
using Application.Features.Common.Content.Advertisements.Commands;
using Application.Features.Common.Content.Advertisements.DTOs;
using Application.Features.Common.Content.Advertisements.Queries;
using Application.Interfaces.Common;
using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Common;

public class AdvertisementHandlerTests
{
    private const string ImageUrl = "https://cdn.example.com/ad.png";

    private readonly Mock<IAdvertisementRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IImageStorageService> _storage = new();

    private static Advertisement Existing()
    {
        return new Advertisement(
            Guid.NewGuid(),
            "Sponsor",
            new Uri(ImageUrl),
            null,
            null,
            AdSlot.LeftTop,
            true,
            0,
            null,
            null);
    }

    private UpdateAdvertisementCommandHandler UpdateHandler()
    {
        return new UpdateAdvertisementCommandHandler(
            _repo.Object,
            _uow.Object,
            _storage.Object,
            Mock.Of<ILogger<UpdateAdvertisementCommandHandler>>());
    }

    private DeleteAdvertisementCommandHandler DeleteHandler()
    {
        return new DeleteAdvertisementCommandHandler(
            _repo.Object,
            _uow.Object,
            _storage.Object,
            Mock.Of<ILogger<DeleteAdvertisementCommandHandler>>());
    }

    [Fact]
    public async Task Create_ValidCommand_SavesAndReturnsDto()
    {
        CreateAdvertisementCommandHandler handler = new(_repo.Object, _uow.Object);
        CreateAdvertisementCommand command = new(
            "Sponsor", ImageUrl, "https://sponsor.fi", "Logo", AdSlot.RightBottom, true, 1, null, null);

        Result<AdvertisementDto> result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Slot.Should().Be(AdSlot.RightBottom);
        result.Data.LinkUrl.Should().Be("https://sponsor.fi/");
        _repo.Verify(r => r.AddAsync(It.IsAny<Advertisement>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_EndBeforeStart_ReturnsFailureWithoutSaving()
    {
        CreateAdvertisementCommandHandler handler = new(_repo.Object, _uow.Object);
        DateTime start = new(2026, 10, 16, 0, 0, 0, DateTimeKind.Utc);
        CreateAdvertisementCommand command = new(
            "Sponsor", ImageUrl, null, null, AdSlot.LeftTop, true, 0, start, start.AddDays(-1));

        Result<AdvertisementDto> result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_WhenMissing_ReturnsNotFound()
    {
        Guid id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Advertisement?)null);

        Result<AdvertisementDto> result = await UpdateHandler().Handle(
            new UpdateAdvertisementCommand(id, "Ad", ImageUrl, null, null, AdSlot.LeftTop, true, 0, null, null),
            CancellationToken.None);

        result.ErrorKind.Should().Be(ResultErrorKind.NotFound);
    }

    [Fact]
    public async Task Update_WithNewImage_DeletesPreviousImage()
    {
        Advertisement ad = Existing();
        _repo.Setup(r => r.GetByIdAsync(ad.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ad);
        const string newImage = "https://cdn.example.com/new.png";

        Result<AdvertisementDto> result = await UpdateHandler().Handle(
            new UpdateAdvertisementCommand(ad.Id, "Ad", newImage, null, null, AdSlot.LeftTop, true, 0, null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.ImageUrl.Should().Be(newImage);
        _storage.Verify(s => s.DeleteImage(new Uri(ImageUrl), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithSameImage_KeepsImage()
    {
        Advertisement ad = Existing();
        _repo.Setup(r => r.GetByIdAsync(ad.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ad);

        await UpdateHandler().Handle(
            new UpdateAdvertisementCommand(ad.Id, "Renamed", ImageUrl, null, null, AdSlot.LeftTop, false, 0, null, null),
            CancellationToken.None);

        _storage.Verify(s => s.DeleteImage(It.IsAny<Uri>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_WhenMissing_ReturnsNotFound()
    {
        Guid id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Advertisement?)null);

        Result<bool> result = await DeleteHandler().Handle(new DeleteAdvertisementCommand(id), CancellationToken.None);

        result.ErrorKind.Should().Be(ResultErrorKind.NotFound);
    }

    [Fact]
    public async Task Delete_WhenImageDeletionFails_StillSucceeds()
    {
        Advertisement ad = Existing();
        _repo.Setup(r => r.GetByIdAsync(ad.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ad);
        _storage.Setup(s => s.DeleteImage(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("storage down"));

        Result<bool> result = await DeleteHandler().Handle(new DeleteAdvertisementCommand(ad.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.RemoveAsync(ad, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetVisible_PassesCurrentTimeAndMapsPublicDtos()
    {
        DateTime utcNow = new(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);
        _repo.Setup(r => r.GetVisibleAsync(utcNow, It.IsAny<CancellationToken>()))
            .ReturnsAsync([Existing()]);
        GetVisibleAdvertisementsQueryHandler handler = new(
            _repo.Object,
            new FixedTimeProvider(new DateTimeOffset(utcNow, TimeSpan.Zero)));

        Result<IReadOnlyList<PublicAdvertisementDto>> result = await handler.Handle(
            new GetVisibleAdvertisementsQuery(),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle().Which.ImageUrl.Should().Be(ImageUrl);
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
