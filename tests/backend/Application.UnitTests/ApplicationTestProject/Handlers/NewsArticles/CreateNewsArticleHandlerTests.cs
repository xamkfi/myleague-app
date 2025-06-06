using Application.Commands.NewsArticles;
using Application.Common;
using Application.DTOs.Common;
using Application.Handlers.NewsArticles;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ApplicationTestProject.Handlers.NewsArticles;

public class CreateNewsArticleHandlerTests
{
    private readonly Mock<INewsArticleRepository> _mockNewsRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<CreateNewsArticleHandler>> _mockLogger;
    private readonly CreateNewsArticleHandler _handler;

    public CreateNewsArticleHandlerTests()
    {
        _mockNewsRepository = new Mock<INewsArticleRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<CreateNewsArticleHandler>>();
        _handler = new CreateNewsArticleHandler(_mockNewsRepository.Object, _mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessResult()
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Test News Article",
            "<p>Test content</p>",
            "Test summary",
            new List<string> { "https://example.com/image1.jpg" },
            "Test Author",
            "General",
            "Football",
            new List<string> { "tag1", "tag2" }
        );

        _mockNewsRepository.Setup(x => x.SaveAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        Result<NewsArticleDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be(command.Title);
        result.Data.ContentHtml.Should().Be(command.ContentHtml);
        result.Data.Summary.Should().Be(command.Summary);
        result.Data.Author.Should().Be(command.Author);

        _mockNewsRepository.Verify(x => x.SaveAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommandWithMinimalData_ReturnsSuccessResult()
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Minimal Article",
            "<p>Basic content</p>"
        );

        _mockNewsRepository.Setup(x => x.SaveAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        Result<NewsArticleDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be(command.Title);
        result.Data.ContentHtml.Should().Be(command.ContentHtml);
        result.Data.Summary.Should().BeNull();
        result.Data.Author.Should().BeNull();
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ReturnsFailureResult()
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Test Article",
            "<p>Test content</p>"
        );

        _mockNewsRepository.Setup(x => x.SaveAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Result<NewsArticleDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("error occurred while creating");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UnitOfWorkThrowsException_ReturnsFailureResult()
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Test Article",
            "<p>Test content</p>"
        );

        _mockNewsRepository.Setup(x => x.SaveAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save error"));

        // Act
        Result<NewsArticleDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("error occurred while creating");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Test Article",
            "<p>Test content</p>"
        );

        using CancellationTokenSource cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(command, cts.Token));

        _mockNewsRepository.Verify(x => x.SaveAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CancellationRequestedDuringOperation_ThrowsOperationCanceledException()
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Test Article",
            "<p>Test content</p>"
        );

        using CancellationTokenSource cts = new CancellationTokenSource();

        _mockNewsRepository.Setup(x => x.SaveAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()))
            .Callback(() => cts.Cancel()) // Cancel during repository operation
            .Returns(Task.CompletedTask);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(command, cts.Token));
    }

    [Theory]
    [InlineData("General")]
    [InlineData("MatchReports")]
    [InlineData("Transfers")]
    public async Task Handle_ValidCategory_SetsCategoryCorrectly(string category)
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Test Article",
            "<p>Test content</p>",
            null,
            null,
            null,
            category
        );

        _mockNewsRepository.Setup(x => x.SaveAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        Result<NewsArticleDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Theory]
    [InlineData("Football")]
    [InlineData("Icehockey")]
    [InlineData("Basketball")]
    public async Task Handle_ValidSportCategory_SetsSportCategoryCorrectly(string sportCategory)
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Test Article",
            "<p>Test content</p>",
            null,
            null,
            null,
            null,
            sportCategory
        );

        _mockNewsRepository.Setup(x => x.SaveAsync(It.IsAny<NewsArticle>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        Result<NewsArticleDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }
} 