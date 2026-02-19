using Application.Common;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Clubs.Handlers;
using Application.Features.Common.Clubs.Queries;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ApplicationTestProject.Handlers.Clubs;

public class GetClubByIdHandlerTests
{
    private readonly Mock<IClubRepository> _mockClubRepository;
    private readonly Mock<ILogger<GetClubByIdHandler>> _mockLogger;
    private readonly GetClubByIdHandler _handler;

    public GetClubByIdHandlerTests()
    {
        _mockClubRepository = new Mock<IClubRepository>();
        _mockLogger = new Mock<ILogger<GetClubByIdHandler>>();
        _handler = new GetClubByIdHandler(_mockClubRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsSuccessResult()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        GetClubByIdQuery query = new GetClubByIdQuery(clubId);

        Club club = new Club(
            "Test Club",
            "Test City",
            "Test Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new Uri("https://testclub.com"),
            new Uri("https://testclub.com/logo.png"),
            "contact@testclub.com"
        );

        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ReturnsAsync(club);

        // Act
        Result<ClubDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(club.Id);
        result.Data.Name.Should().Be(club.Name);
        result.Data.City.Should().Be(club.City);
        result.Data.Country.Should().Be(club.Country);
        result.Data.FoundingDate.Should().Be(club.FoundingDate);
        result.Data.WebsiteUrl.Should().Be(club.WebsiteUrl!.ToString());
        result.Data.LogoUrl.Should().Be(club.LogoUrl!.ToString());
        result.Data.ContactEmail.Should().Be(club.ContactEmail);

        _mockClubRepository.Verify(x => x.GetByIdAsync(clubId), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidQueryWithMinimalClubData_ReturnsSuccessResult()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        GetClubByIdQuery query = new GetClubByIdQuery(clubId);

        Club club = new Club(
            "Minimal Club",
            "City",
            "Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ReturnsAsync(club);

        // Act
        Result<ClubDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(club.Id);
        result.Data.Name.Should().Be(club.Name);
        result.Data.WebsiteUrl.Should().Be("https://example.com/");
        result.Data.LogoUrl.Should().Be("https://example.com/logo.png");
        result.Data.ContactEmail.Should().Be("contact@example.com");

        _mockClubRepository.Verify(x => x.GetByIdAsync(clubId), Times.Once);
    }

    [Fact]
    public async Task Handle_ClubNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        GetClubByIdQuery query = new GetClubByIdQuery(clubId);

        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ReturnsAsync((Club?)null);

        // Act
        Result<ClubDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        result.Data.Should().BeNull();

        _mockClubRepository.Verify(x => x.GetByIdAsync(clubId), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ReturnsFailureResult()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        GetClubByIdQuery query = new GetClubByIdQuery(clubId);

        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Result<ClubDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("error occurred while retrieving");
        result.Data.Should().BeNull();

        _mockClubRepository.Verify(x => x.GetByIdAsync(clubId), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyGuid_ReturnsNotFoundResult()
    {
        // Arrange
        GetClubByIdQuery query = new GetClubByIdQuery(Guid.Empty);

        _mockClubRepository.Setup(x => x.GetByIdAsync(Guid.Empty))
            .ReturnsAsync((Club?)null);

        // Act
        Result<ClubDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        result.Data.Should().BeNull();

        _mockClubRepository.Verify(x => x.GetByIdAsync(Guid.Empty), Times.Once);
    }

    [Fact]
    public async Task Handle_LogsAppropriateMessages_OnSuccess()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        GetClubByIdQuery query = new GetClubByIdQuery(clubId);

        Club club = new Club(
            "Test Club",
            "Test City",
            "Test Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ReturnsAsync(club);

        // Act
        Result<ClubDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify logging calls were made
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieving club with ID")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully retrieved club")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_LogsWarningMessage_OnNotFound()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        GetClubByIdQuery query = new GetClubByIdQuery(clubId);

        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ReturnsAsync((Club?)null);

        // Act
        Result<ClubDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        // Verify warning log was made
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_LogsErrorMessage_OnException()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        GetClubByIdQuery query = new GetClubByIdQuery(clubId);

        Exception testException = new Exception("Test exception");
        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ThrowsAsync(testException);

        // Act
        Result<ClubDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        // Verify error log was made
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while retrieving club")),
                testException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    public async Task Handle_VariousGuidFormats_HandledCorrectly(string guidString)
    {
        // Arrange
        Guid clubId = Guid.Parse(guidString);
        GetClubByIdQuery query = new GetClubByIdQuery(clubId);

        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ReturnsAsync((Club?)null);

        // Act
        Result<ClubDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");

        _mockClubRepository.Verify(x => x.GetByIdAsync(clubId), Times.Once);
    }

    [Fact]
    public async Task Handle_ClubWithNullUrls_ReturnsEmptyStringsInDto()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        GetClubByIdQuery query = new GetClubByIdQuery(clubId);

        Club club = new Club(
            "Test Club",
            "Test City",
            "Test Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            null, // WebsiteUrl
            null, // LogoUrl
            "" // ContactEmail
        );

        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ReturnsAsync(club);

        // Act
        Result<ClubDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.WebsiteUrl.Should().Be("https://example.com/");
        result.Data.LogoUrl.Should().Be("https://example.com/logo.png");
        result.Data.ContactEmail.Should().Be("");
    }

    [Fact]
    public async Task Handle_MultipleCallsWithSameId_EachCallProcessedIndependently()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        GetClubByIdQuery query = new GetClubByIdQuery(clubId);

        Club club = new Club(
            "Test Club",
            "Test City",
            "Test Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ReturnsAsync(club);

        // Act
        Result<ClubDto> result1 = await _handler.Handle(query, CancellationToken.None);
        Result<ClubDto> result2 = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Data!.Id.Should().Be(result2.Data!.Id);

        _mockClubRepository.Verify(x => x.GetByIdAsync(clubId), Times.Exactly(2));
    }
} 