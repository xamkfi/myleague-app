using Application.Common;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.News.Handlers;
using Application.Features.Common.News.Queries;
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ApplicationTestProject.Handlers.NewsArticles;

public class GetAllNewsArticlesHandlerTests
{
    private readonly Mock<INewsArticleRepository> _mockNewsRepository;
    private readonly Mock<IPaginationService> _mockPaginationService;
    private readonly Mock<ILogger<GetAllNewsArticlesHandler>> _mockLogger;
    private readonly GetAllNewsArticlesHandler _handler;

    public GetAllNewsArticlesHandlerTests()
    {
        _mockNewsRepository = new Mock<INewsArticleRepository>();
        _mockPaginationService = new Mock<IPaginationService>();
        _mockLogger = new Mock<ILogger<GetAllNewsArticlesHandler>>();
        
        // Setup pagination service defaults for News resource
        _mockPaginationService.Setup(x => x.IsValidPageSize("News", It.IsAny<int>()))
            .Returns(true);
        _mockPaginationService.Setup(x => x.ResolvePageSize("News", It.IsAny<int>()))
            .Returns<string, int>((_, pageSize) => pageSize <= 0 ? 10 : pageSize);
        
        _handler = new GetAllNewsArticlesHandler(_mockNewsRepository.Object, _mockPaginationService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsPagedResult()
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(
            Page: 1,
            PageSize: 3
        );

        List<NewsArticle> newsArticles = new List<NewsArticle>
        {
            new NewsArticle(Guid.NewGuid(), "Article 1", new Uri("https://example.com/image1.jpg"), "<p>Content 1</p>", "Author 1"),
            new NewsArticle(Guid.NewGuid(), "Article 2", new Uri("https://example.com/image2.jpg"), "<p>Content 2</p>", "Author 2")
        };

        _mockNewsRepository.Setup(x => x.GetAllAsync(1, 3, null, null, null, null, false, It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newsArticles);

        _mockNewsRepository.Setup(x => x.GetCountAsync(null, null, null, null, false, It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        // Act
        Result<PagedResult<NewsArticleListDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(10);
        result.Data.Page.Should().Be(1);
        result.Data.PageSize.Should().Be(3);
        result.Data.TotalPages.Should().Be(4);
        result.Data.HasNextPage.Should().BeTrue();
        result.Data.HasPreviousPage.Should().BeFalse();

        _mockNewsRepository.Verify(x => x.GetAllAsync(1, 3, null, null, null, null, false, It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockNewsRepository.Verify(x => x.GetCountAsync(null, null, null, null, false, It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_QueryWithFilters_PassesFiltersToRepository()
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(
            Page: 2,
            PageSize: 3,
            Category: "General",
            SportCategory: "Football",
            Search: "",
            Author: "Test Author",
            IncludeArchived: true
        );

        List<NewsArticle> newsArticles = new List<NewsArticle>();

        _mockNewsRepository.Setup(x => x.GetAllAsync(2, 3, "General", "Football", "", "Test Author", true, It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newsArticles);

        _mockNewsRepository.Setup(x => x.GetCountAsync("General", "Football", "", "Test Author", true, It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        Result<PagedResult<NewsArticleListDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().BeEmpty();
        result.Data.TotalCount.Should().Be(0);

        _mockNewsRepository.Verify(x => x.GetAllAsync(2, 3, "General", "Football", "", "Test Author", true, It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockNewsRepository.Verify(x => x.GetCountAsync("General", "Football", "", "Test Author", true, It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_InvalidPageNumber_ReturnsFailureResult(int invalidPage)
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(Page: invalidPage);

        // Act
        Result<PagedResult<NewsArticleListDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Page must be greater than 0");

        _mockNewsRepository.Verify(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task Handle_InvalidPageSize_ReturnsFailureResult(int invalidPageSize)
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(PageSize: invalidPageSize);
        
        // Setup pagination service to return invalid for these test cases
        _mockPaginationService.Setup(x => x.IsValidPageSize("News", invalidPageSize))
            .Returns(false);
        _mockPaginationService.Setup(x => x.GetPaginationSettings("News"))
            .Returns(new PaginationSettings(10, 50, 1));

        // Act
        Result<PagedResult<NewsArticleListDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Page size");

        _mockNewsRepository.Verify(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ReturnsFailureResult()
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery();

        _mockNewsRepository.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Result<PagedResult<NewsArticleListDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("error occurred while retrieving");
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery();

        using CancellationTokenSource cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(query, cts.Token));

        _mockNewsRepository.Verify(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CancellationRequestedDuringOperation_ThrowsOperationCanceledException()
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery();

        using CancellationTokenSource cts = new CancellationTokenSource();

        _mockNewsRepository.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .Callback(() => cts.Cancel()) // Cancel during operation
            .ReturnsAsync(new List<NewsArticle>());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(query, cts.Token));
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyPagedResult()
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery();

        _mockNewsRepository.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NewsArticle>());

        _mockNewsRepository.Setup(x => x.GetCountAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        Result<PagedResult<NewsArticleListDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().BeEmpty();
        result.Data.TotalCount.Should().Be(0);
        result.Data.TotalPages.Should().Be(0);
        result.Data.HasNextPage.Should().BeFalse();
        result.Data.HasPreviousPage.Should().BeFalse();
    }
} 
