using Application.Common;
using Application.Features.Common.Clubs.Commands;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Clubs.Handlers;
using Application.Features.Common.Clubs.Mappings;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ApplicationTestProject.Handlers.Clubs;

public class UpdateClubHandlerTests
{
    private readonly Mock<IClubRepository> _mockClubRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<UpdateClubHandler>> _mockLogger;
    private readonly UpdateClubHandler _handler;

    public UpdateClubHandlerTests()
    {
        _mockClubRepository = new Mock<IClubRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<UpdateClubHandler>>();
        _handler = new UpdateClubHandler(_mockClubRepository.Object, _mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessResult()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        UpdateClubCommand command = new UpdateClubCommand(
            clubId,
            "Updated Club",
            "Updated City",
            "Updated Country",
            new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            "https://updatedclub.com",
            "https://updatedclub.com/logo.png",
            "updated@club.com"
        );

        Club existingClub = new Club(
            "Original Club",
            "Original City",
            "Original Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ReturnsAsync(existingClub);

        _mockClubRepository.Setup(x => x.GetByNameAsync(command.Name))
            .ReturnsAsync((Club?)null);

        _mockClubRepository.Setup(x => x.UpdateAsync(It.IsAny<Club>()))
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
        result.Data.WebsiteUrl.Should().Be("https://updatedclub.com/");
        result.Data.LogoUrl.Should().Be(command.LogoUrl);
        result.Data.ContactEmail.Should().Be(command.ContactEmail);

        _mockClubRepository.Verify(x => x.GetByIdAsync(clubId), Times.Once);
        _mockClubRepository.Verify(x => x.GetByNameAsync(command.Name), Times.Once);
        _mockClubRepository.Verify(x => x.UpdateAsync(It.IsAny<Club>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ClubNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        UpdateClubCommand command = new UpdateClubCommand(
            clubId,
            "Updated Club",
            "Updated City",
            "Updated Country",
            new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ReturnsAsync((Club?)null);

        // Act
        Result<ClubDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        result.Data.Should().BeNull();

        _mockClubRepository.Verify(x => x.GetByIdAsync(clubId), Times.Once);
        _mockClubRepository.Verify(x => x.GetByNameAsync(It.IsAny<string>()), Times.Never);
        _mockClubRepository.Verify(x => x.UpdateAsync(It.IsAny<Club>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NameAlreadyExistsForDifferentClub_ReturnsFailureResult()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        Guid otherClubId = Guid.NewGuid();
        UpdateClubCommand command = new UpdateClubCommand(
            clubId,
            "Existing Club Name",
            "Updated City",
            "Updated Country",
            new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        Club existingClub = new Club(
            "Original Club",
            "Original City",
            "Original Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        Club clubWithSameName = new Club(
            "Existing Club Name",
            "Other City",
            "Other Country",
            new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );
        // Set the ID using reflection to simulate different club
        typeof(Club).GetProperty("Id")!.SetValue(clubWithSameName, otherClubId);

        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ReturnsAsync(existingClub);

        _mockClubRepository.Setup(x => x.GetByNameAsync(command.Name))
            .ReturnsAsync(clubWithSameName);

        // Act
        Result<ClubDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");
        result.Data.Should().BeNull();

        _mockClubRepository.Verify(x => x.GetByIdAsync(clubId), Times.Once);
        _mockClubRepository.Verify(x => x.GetByNameAsync(command.Name), Times.Once);
        _mockClubRepository.Verify(x => x.UpdateAsync(It.IsAny<Club>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NameExistsForSameClub_ReturnsSuccessResult()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        UpdateClubCommand command = new UpdateClubCommand(
            clubId,
            "Same Club Name",
            "Updated City",
            "Updated Country",
            new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        Club existingClub = new Club(
            "Same Club Name",
            "Original City",
            "Original Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );
        // Set the ID using reflection to simulate same club
        typeof(Club).GetProperty("Id")!.SetValue(existingClub, clubId);

        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ReturnsAsync(existingClub);

        _mockClubRepository.Setup(x => x.GetByNameAsync(command.Name))
            .ReturnsAsync(existingClub);

        _mockClubRepository.Setup(x => x.UpdateAsync(It.IsAny<Club>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        Result<ClubDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        _mockClubRepository.Verify(x => x.GetByIdAsync(clubId), Times.Once);
        _mockClubRepository.Verify(x => x.GetByNameAsync(command.Name), Times.Once);
        _mockClubRepository.Verify(x => x.UpdateAsync(It.IsAny<Club>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ReturnsFailureResult()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        UpdateClubCommand command = new UpdateClubCommand(
            clubId,
            "Updated Club",
            "Updated City",
            "Updated Country",
            new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Result<ClubDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("error occurred while updating");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UnitOfWorkThrowsException_ReturnsFailureResult()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        UpdateClubCommand command = new UpdateClubCommand(
            clubId,
            "Updated Club",
            "Updated City",
            "Updated Country",
            new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        Club existingClub = new Club(
            "Original Club",
            "Original City",
            "Original Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ReturnsAsync(existingClub);

        _mockClubRepository.Setup(x => x.GetByNameAsync(command.Name))
            .ReturnsAsync((Club?)null);

        _mockClubRepository.Setup(x => x.UpdateAsync(It.IsAny<Club>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save error"));

        // Act
        Result<ClubDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("error occurred while updating");
        result.Data.Should().BeNull();
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public async Task Handle_NonUtcFoundingDate_ConvertsToUtcAndReturnsSuccess(DateTimeKind dateTimeKind)
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        DateTime foundingDate = dateTimeKind == DateTimeKind.Local 
            ? new DateTime(2021, 1, 1, 12, 0, 0, DateTimeKind.Local)
            : new DateTime(2021, 1, 1, 12, 0, 0, DateTimeKind.Unspecified);

        UpdateClubCommand command = new UpdateClubCommand(
            clubId,
            "Updated Club",
            "Updated City",
            "Updated Country",
            foundingDate
        );

        Club existingClub = new Club(
            "Original Club",
            "Original City",
            "Original Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ReturnsAsync(existingClub);

        _mockClubRepository.Setup(x => x.GetByNameAsync(command.Name))
            .ReturnsAsync((Club?)null);

        _mockClubRepository.Setup(x => x.UpdateAsync(It.IsAny<Club>()))
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
    public async Task Handle_ValidCommandWithUrls_UpdatesClubWithParsedUris()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        UpdateClubCommand command = new UpdateClubCommand(
            clubId,
            "Updated Club",
            "Updated City",
            "Updated Country",
            new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            "https://updated.com",
            "https://updated.com/logo.png",
            "updated@example.com"
        );

        Club existingClub = new Club(
            "Original Club",
            "Original City",
            "Original Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        Club? capturedClub = null;
        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ReturnsAsync(existingClub);

        _mockClubRepository.Setup(x => x.GetByNameAsync(command.Name))
            .ReturnsAsync((Club?)null);

        _mockClubRepository.Setup(x => x.UpdateAsync(It.IsAny<Club>()))
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
        capturedClub.WebsiteUrl!.ToString().Should().Be("https://updated.com/");
        capturedClub.LogoUrl.Should().NotBeNull();
        capturedClub.LogoUrl!.ToString().Should().Be(command.LogoUrl);
        capturedClub.ContactEmail.Should().Be(command.ContactEmail);
    }

    [Fact]
    public async Task Handle_EmptyUrls_UpdatesClubWithNullUris()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        UpdateClubCommand command = new UpdateClubCommand(
            clubId,
            "Updated Club",
            "Updated City",
            "Updated Country",
            new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            "",
            "",
            ""
        );

        Club existingClub = new Club(
            "Original Club",
            "Original City",
            "Original Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new Uri("https://original.com"),
            new Uri("https://original.com/logo.png"),
            "original@example.com"
        );

        Club? capturedClub = null;
        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ReturnsAsync(existingClub);

        _mockClubRepository.Setup(x => x.GetByNameAsync(command.Name))
            .ReturnsAsync((Club?)null);

        _mockClubRepository.Setup(x => x.UpdateAsync(It.IsAny<Club>()))
            .Callback<Club>(club => capturedClub = club)
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        Result<ClubDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedClub.Should().NotBeNull();
        capturedClub!.WebsiteUrl.Should().Be(new Uri("https://example.com/"));
        capturedClub.LogoUrl.Should().Be(new Uri("https://example.com/logo.png"));
        capturedClub.ContactEmail.Should().Be("");
    }

    [Fact]
    public async Task Handle_LogsAppropriateMessages()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        UpdateClubCommand command = new UpdateClubCommand(
            clubId,
            "Updated Club",
            "Updated City",
            "Updated Country",
            new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        Club existingClub = new Club(
            "Original Club",
            "Original City",
            "Original Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        _mockClubRepository.Setup(x => x.GetByIdAsync(clubId))
            .ReturnsAsync(existingClub);

        _mockClubRepository.Setup(x => x.GetByNameAsync(command.Name))
            .ReturnsAsync((Club?)null);

        _mockClubRepository.Setup(x => x.UpdateAsync(It.IsAny<Club>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        Result<ClubDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify logging calls were made
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Updating club")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully updated club")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
} 