using Application.Common;
using Application.Features.Common.News.Handlers;
using Application.Features.Common.News.Queries;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationTestProject.Handlers.NewsArticles;

public class GetNewsArticleTagsHandlerTests
{
    private readonly Mock<INewsArticleRepository> _mockNewsRepository;
    private readonly Mock<ILogger<GetNewsArticleTagsHandler>> _mockLogger;
    private readonly GetNewsArticleTagsHandler _handler;

    public GetNewsArticleTagsHandlerTests()
    {
        _mockNewsRepository = new Mock<INewsArticleRepository>();
        _mockLogger = new Mock<ILogger<GetNewsArticleTagsHandler>>();
        _handler = new GetNewsArticleTagsHandler(_mockNewsRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsTags_ReturnsSuccess()
    {
        List<string> tags = new List<string> { "F-liiga", "Playoffs", "U18" };
        _mockNewsRepository
            .Setup(x => x.GetAllTagsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        Result<IEnumerable<string>> result = await _handler.Handle(
            new GetNewsArticleTagsQuery(),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(tags);
        _mockNewsRepository.Verify(x => x.GetAllTagsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailure()
    {
        _mockNewsRepository
            .Setup(x => x.GetAllTagsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("query failed"));

        Result<IEnumerable<string>> result = await _handler.Handle(
            new GetNewsArticleTagsQuery(),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
    }
}
