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
/// Handler for retrieving news articles by category
/// </summary>
public class GetNewsArticlesByCategoryHandler : IRequestHandler<GetNewsArticlesByCategoryQuery, Result<IEnumerable<NewsArticleListDto>>>
{
    private readonly INewsArticleRepository _newsRepository;
    private readonly ILogger<GetNewsArticlesByCategoryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetNewsArticlesByCategoryHandler class
    /// </summary>
    /// <param name="newsRepository">The news repository</param>
    /// <param name="logger">The logger</param>
    public GetNewsArticlesByCategoryHandler(INewsArticleRepository newsRepository, ILogger<GetNewsArticlesByCategoryHandler> logger)
    {
        _newsRepository = newsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetNewsArticlesByCategoryQuery request
    /// </summary>
    /// <param name="request">The query containing the category</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A collection of news articles by category as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<NewsArticleListDto>>> Handle(GetNewsArticlesByCategoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving news articles by category: {Category}", request.Category);

            // Validate category parameter
            if (string.IsNullOrWhiteSpace(request.Category))
            {
                _logger.LogWarning("Category parameter is empty or whitespace");
                return Result<IEnumerable<NewsArticleListDto>>.Failure("Category cannot be empty or whitespace.");
            }

            IEnumerable<NewsArticle> newsArticles = await _newsRepository.GetByCategoryAsync(request.Category, cancellationToken);

            IEnumerable<NewsArticleListDto> newsDtos = NewsArticleMapper.ToListDtos(newsArticles);
            
            _logger.LogInformation("Successfully retrieved {Count} news articles by category: {Category}", newsArticles.Count(), request.Category);

            return Result<IEnumerable<NewsArticleListDto>>.Success(newsDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving news articles by category: {Category}", request.Category);
            return Result<IEnumerable<NewsArticleListDto>>.Failure("An error occurred while retrieving news articles by category.");
        }
    }
} 