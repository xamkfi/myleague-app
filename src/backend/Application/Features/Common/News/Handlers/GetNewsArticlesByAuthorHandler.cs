using Application.Queries.NewsArticles;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Application.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.NewsArticles;

/// <summary>
/// Handler for retrieving news articles by author
/// </summary>
public class GetNewsArticlesByAuthorHandler : IRequestHandler<GetNewsArticlesByAuthorQuery, Result<IEnumerable<NewsArticleListDto>>>
{
    private readonly INewsArticleRepository _newsRepository;
    private readonly ILogger<GetNewsArticlesByAuthorHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetNewsArticlesByAuthorHandler class
    /// </summary>
    /// <param name="newsRepository">The news repository</param>
    /// <param name="logger">The logger</param>
    public GetNewsArticlesByAuthorHandler(INewsArticleRepository newsRepository, ILogger<GetNewsArticlesByAuthorHandler> logger)
    {
        _newsRepository = newsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetNewsArticlesByAuthorQuery request
    /// </summary>
    /// <param name="request">The query containing the author name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A collection of news articles by the author as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<NewsArticleListDto>>> Handle(GetNewsArticlesByAuthorQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving news articles by author: {Author}", request.Author);

            // Validate author parameter
            if (string.IsNullOrWhiteSpace(request.Author))
            {
                _logger.LogWarning("Author parameter is empty or whitespace");
                return Result<IEnumerable<NewsArticleListDto>>.Failure("Author cannot be empty or whitespace.");
            }

            IEnumerable<NewsArticle> newsArticles = await _newsRepository.GetByAuthorAsync(request.Author, cancellationToken);

            IEnumerable<NewsArticleListDto> newsDtos = NewsArticleMapper.ToListDtos(newsArticles);
            
            _logger.LogInformation("Successfully retrieved {Count} news articles by author: {Author}", newsArticles.Count(), request.Author);

            return Result<IEnumerable<NewsArticleListDto>>.Success(newsDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving news articles by author: {Author}", request.Author);
            return Result<IEnumerable<NewsArticleListDto>>.Failure("An error occurred while retrieving news articles by author.");
        }
    }
} 