using Application.Common;
using Application.Features.Common.Content.Images.Commands;
using Application.Features.Common.Content.Images.Handlers;
using Application.Interfaces.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Images;

public class UploadImageCommandHandlerTests
{
    private readonly Mock<IImageStorageService> _storage = new();

    public UploadImageCommandHandlerTests()
    {
        _storage
            .Setup(s => s.SaveImage(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stream _, string fileName, CancellationToken _) => new Uri($"https://example.test/{fileName}"));
    }

    [Theory]
    [InlineData("logo.png", "image/png")]
    [InlineData("photo.JPG", "image/jpeg")]
    [InlineData("banner.webp", "image/webp")]
    public async Task Handle_WithAllowedExtensionAndMatchingType_SavesWithGeneratedName(string fileName, string contentType)
    {
        Result<Uri> result = await Handle(fileName, contentType);

        result.IsSuccess.Should().BeTrue();
        string expectedExtension = Path.GetExtension(fileName).ToLowerInvariant();
        _storage.Verify(
            s => s.SaveImage(
                It.IsAny<Stream>(),
                It.Is<string>(name => name.EndsWith(expectedExtension) && name != fileName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("evil.svg", "image/svg+xml")]
    [InlineData("evil.html", "image/png")]
    [InlineData("noextension", "image/png")]
    public async Task Handle_WithDisallowedExtension_ReturnsValidationFailure(string fileName, string contentType)
    {
        Result<Uri> result = await Handle(fileName, contentType);

        result.IsSuccess.Should().BeFalse();
        result.ErrorKind.Should().Be(ResultErrorKind.Validation);
        _storage.Verify(
            s => s.SaveImage(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithContentTypeNotMatchingExtension_ReturnsValidationFailure()
    {
        Result<Uri> result = await Handle("logo.png", "image/jpeg");

        result.IsSuccess.Should().BeFalse();
        result.ErrorKind.Should().Be(ResultErrorKind.Validation);
        _storage.Verify(
            s => s.SaveImage(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private async Task<Result<Uri>> Handle(string fileName, string contentType)
    {
        UploadImageCommandHandler handler = new(
            _storage.Object,
            new Mock<ILogger<UploadImageCommandHandler>>().Object);
        using MemoryStream stream = new(new byte[] { 1, 2, 3 });
        return await handler.Handle(new UploadImageCommand(stream, fileName, contentType), CancellationToken.None);
    }
}
