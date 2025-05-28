using Application.Commands.Clubs;
using Application.Common;
using Application.DTOs.Common;
using Application.Handlers.Clubs;
using Application.Mappings.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ApplicationTestProject.Handlers.Clubs;

public class CreateClubHandlerTests
{
    private readonly Mock<IClubRepository> _mockClubRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<CreateClubHandler>> _mockLogger;
    private readonly CreateClubHandler _handler;

    public CreateClubHandlerTests()
    {
        _mockClubRepository = new Mock<IClubRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<CreateClubHandler>>();
        _handler = new CreateClubHandler(_mockClubRepository.Object, _mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessResult()
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Test Club",
            "Test City",
            "Test Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            "https://testclub.com",
            "https://testclub.com/logo.png",
            "contact@testclub.com"
        );

        _mockClubRepository.Setup(x => x.ExistsByNameAsync(command.Name))
            .ReturnsAsync(false);

        _mockClubRepository.Setup(x => x.AddAsync(It.IsAny<Club>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        Result<ClubDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(command.Name);
        result.Data.City.Should().Be(command.City);
        result.Data.Country.Should().Be(command.Country);
        result.Data.FoundingDate.Should().Be(command.FoundingDate);
        result.Data.WebsiteUrl.Should().Be("https://testclub.com/");
        result.Data.LogoUrl.Should().Be(command.LogoUrl);
        result.Data.ContactEmail.Should().Be(command.ContactEmail);

        _mockClubRepository.Verify(x => x.ExistsByNameAsync(command.Name), Times.Once);
        _mockClubRepository.Verify(x => x.AddAsync(It.IsAny<Club>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommandWithMinimalData_ReturnsSuccessResult()
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Minimal Club",
            "City",
            "Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        _mockClubRepository.Setup(x => x.ExistsByNameAsync(command.Name))
            .ReturnsAsync(false);

        _mockClubRepository.Setup(x => x.AddAsync(It.IsAny<Club>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        Result<ClubDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(command.Name);
        result.Data.WebsiteUrl.Should().Be("https://example.com/");
        result.Data.LogoUrl.Should().Be("https://example.com/logo.png");
        result.Data.ContactEmail.Should().Be("");
    }

    [Fact]
    public async Task Handle_ClubNameAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Existing Club",
            "Test City",
            "Test Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        _mockClubRepository.Setup(x => x.ExistsByNameAsync(command.Name))
            .ReturnsAsync(true);

        // Act
        Result<ClubDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");
        result.Data.Should().BeNull();

        _mockClubRepository.Verify(x => x.ExistsByNameAsync(command.Name), Times.Once);
        _mockClubRepository.Verify(x => x.AddAsync(It.IsAny<Club>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ReturnsFailureResult()
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Test Club",
            "Test City",
            "Test Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        _mockClubRepository.Setup(x => x.ExistsByNameAsync(command.Name))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Result<ClubDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("error occurred while creating");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UnitOfWorkThrowsException_ReturnsFailureResult()
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Test Club",
            "Test City",
            "Test Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        _mockClubRepository.Setup(x => x.ExistsByNameAsync(command.Name))
            .ReturnsAsync(false);

        _mockClubRepository.Setup(x => x.AddAsync(It.IsAny<Club>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save error"));

        // Act
        Result<ClubDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("error occurred while creating");
        result.Data.Should().BeNull();
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public async Task Handle_NonUtcFoundingDate_ConvertsToUtcAndReturnsSuccess(DateTimeKind dateTimeKind)
    {
        // Arrange
        DateTime foundingDate = dateTimeKind == DateTimeKind.Local 
            ? new DateTime(2020, 1, 1, 12, 0, 0, DateTimeKind.Local)
            : new DateTime(2020, 1, 1, 12, 0, 0, DateTimeKind.Unspecified);

        CreateClubCommand command = new CreateClubCommand(
            "Test Club",
            "Test City",
            "Test Country",
            foundingDate
        );

        _mockClubRepository.Setup(x => x.ExistsByNameAsync(command.Name))
            .ReturnsAsync(false);

        _mockClubRepository.Setup(x => x.AddAsync(It.IsAny<Club>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        Result<ClubDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FoundingDate.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task Handle_ValidCommandWithUrls_CreatesClubWithParsedUris()
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Test Club",
            "Test City",
            "Test Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            "https://example.com",
            "https://example.com/logo.png",
            "test@example.com"
        );

        Club? capturedClub = null;
        _mockClubRepository.Setup(x => x.ExistsByNameAsync(command.Name))
            .ReturnsAsync(false);

        _mockClubRepository.Setup(x => x.AddAsync(It.IsAny<Club>()))
            .Callback<Club>(club => capturedClub = club)
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        Result<ClubDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedClub.Should().NotBeNull();
        capturedClub!.WebsiteUrl.Should().NotBeNull();
        capturedClub.WebsiteUrl!.ToString().Should().Be("https://example.com/");
        capturedClub.LogoUrl.Should().NotBeNull();
        capturedClub.LogoUrl!.ToString().Should().Be(command.LogoUrl);
        capturedClub.ContactEmail.Should().Be(command.ContactEmail);
    }

    [Fact]
    public async Task Handle_LogsAppropriateMessages()
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Test Club",
            "Test City",
            "Test Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        _mockClubRepository.Setup(x => x.ExistsByNameAsync(command.Name))
            .ReturnsAsync(false);

        _mockClubRepository.Setup(x => x.AddAsync(It.IsAny<Club>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        Result<ClubDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify logging calls were made (using Moq's verification)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating new club")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully created club")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
} 
