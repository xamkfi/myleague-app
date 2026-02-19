using Application.Common;
using Application.Features.Common.Clubs.Commands;
using Application.Features.Common.Clubs.Handlers;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ApplicationTestProject.Handlers.Clubs;

public class DeleteClubHandlerTests
{
    private readonly Mock<IClubRepository> _mockClubRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<DeleteClubHandler>> _mockLogger;
    private readonly DeleteClubHandler _handler;

    public DeleteClubHandlerTests()
    {
        _mockClubRepository = new Mock<IClubRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<DeleteClubHandler>>();
        _handler = new DeleteClubHandler(_mockClubRepository.Object, _mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessResult()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        DeleteClubCommand command = new DeleteClubCommand(clubId);

        _mockClubRepository.Setup(x => x.ExistsAsync(clubId))
            .ReturnsAsync(true);

        _mockClubRepository.Setup(x => x.DeleteAsync(clubId))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockClubRepository.Verify(x => x.ExistsAsync(clubId), Times.Once);
        _mockClubRepository.Verify(x => x.DeleteAsync(clubId), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ClubNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        DeleteClubCommand command = new DeleteClubCommand(clubId);

        _mockClubRepository.Setup(x => x.ExistsAsync(clubId))
            .ReturnsAsync(false);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");

        _mockClubRepository.Verify(x => x.ExistsAsync(clubId), Times.Once);
        _mockClubRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RepositoryExistsThrowsException_ReturnsFailureResult()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        DeleteClubCommand command = new DeleteClubCommand(clubId);

        _mockClubRepository.Setup(x => x.ExistsAsync(clubId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("error occurred while deleting");

        _mockClubRepository.Verify(x => x.ExistsAsync(clubId), Times.Once);
        _mockClubRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RepositoryDeleteThrowsException_ReturnsFailureResult()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        DeleteClubCommand command = new DeleteClubCommand(clubId);

        _mockClubRepository.Setup(x => x.ExistsAsync(clubId))
            .ReturnsAsync(true);

        _mockClubRepository.Setup(x => x.DeleteAsync(clubId))
            .ThrowsAsync(new Exception("Delete error"));

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("error occurred while deleting");

        _mockClubRepository.Verify(x => x.ExistsAsync(clubId), Times.Once);
        _mockClubRepository.Verify(x => x.DeleteAsync(clubId), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UnitOfWorkThrowsException_ReturnsFailureResult()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        DeleteClubCommand command = new DeleteClubCommand(clubId);

        _mockClubRepository.Setup(x => x.ExistsAsync(clubId))
            .ReturnsAsync(true);

        _mockClubRepository.Setup(x => x.DeleteAsync(clubId))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save error"));

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("error occurred while deleting");

        _mockClubRepository.Verify(x => x.ExistsAsync(clubId), Times.Once);
        _mockClubRepository.Verify(x => x.DeleteAsync(clubId), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyGuid_ReturnsFailureResult()
    {
        // Arrange
        DeleteClubCommand command = new DeleteClubCommand(Guid.Empty);

        _mockClubRepository.Setup(x => x.ExistsAsync(Guid.Empty))
            .ReturnsAsync(false);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");

        _mockClubRepository.Verify(x => x.ExistsAsync(Guid.Empty), Times.Once);
        _mockClubRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_LogsAppropriateMessages_OnSuccess()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        DeleteClubCommand command = new DeleteClubCommand(clubId);

        _mockClubRepository.Setup(x => x.ExistsAsync(clubId))
            .ReturnsAsync(true);

        _mockClubRepository.Setup(x => x.DeleteAsync(clubId))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify logging calls were made
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleting club")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully deleted club")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_LogsWarningMessage_OnNotFound()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        DeleteClubCommand command = new DeleteClubCommand(clubId);

        _mockClubRepository.Setup(x => x.ExistsAsync(clubId))
            .ReturnsAsync(false);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        // Verify warning log was made
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Attempt to delete non-existent club")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_LogsErrorMessage_OnException()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        DeleteClubCommand command = new DeleteClubCommand(clubId);

        Exception testException = new Exception("Test exception");
        _mockClubRepository.Setup(x => x.ExistsAsync(clubId))
            .ThrowsAsync(testException);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        // Verify error log was made
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while deleting club")),
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
        DeleteClubCommand command = new DeleteClubCommand(clubId);

        _mockClubRepository.Setup(x => x.ExistsAsync(clubId))
            .ReturnsAsync(false);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");

        _mockClubRepository.Verify(x => x.ExistsAsync(clubId), Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleCallsWithSameId_EachCallProcessedIndependently()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        DeleteClubCommand command = new DeleteClubCommand(clubId);

        _mockClubRepository.Setup(x => x.ExistsAsync(clubId))
            .ReturnsAsync(true);

        _mockClubRepository.Setup(x => x.DeleteAsync(clubId))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        Result result1 = await _handler.Handle(command, CancellationToken.None);
        Result result2 = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();

        _mockClubRepository.Verify(x => x.ExistsAsync(clubId), Times.Exactly(2));
        _mockClubRepository.Verify(x => x.DeleteAsync(clubId), Times.Exactly(2));
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
} 