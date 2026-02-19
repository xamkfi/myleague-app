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
/// Handler for searching news articles
/// </summary>
public class SearchNewsArticlesHandler : IRequestHandler<SearchNewsArticlesQuery, Result<IEnumerable<NewsArticleListDto>>>
{
    private readonly INewsArticleRepository _newsRepository;
    private readonly ILogger<SearchNewsArticlesHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the SearchNewsArticlesHandler class
    /// </summary>
    /// <param name="newsRepository">The news repository</param>
    /// <param name="logger">The logger</param>
    public SearchNewsArticlesHandler(INewsArticleRepository newsRepository, ILogger<SearchNewsArticlesHandler> logger)
    {
        _newsRepository = newsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the SearchNewsArticlesQuery request
    /// </summary>
    /// <param name="request">The query containing the search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A collection of matching news articles as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<NewsArticleListDto>>> Handle(SearchNewsArticlesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Searching news articles with term: {SearchTerm}", request.SearchTerm);

            // Validate search term parameter
            if (string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                _logger.LogWarning("Search term is empty or whitespace");
                return Result<IEnumerable<NewsArticleListDto>>.Failure("Search term cannot be empty or whitespace.");
            }

            if (request.SearchTerm.Length < 2)
            {
                _logger.LogWarning("Search term too short: {SearchTerm}", request.SearchTerm);
                return Result<IEnumerable<NewsArticleListDto>>.Failure("Search term must be at least 2 characters long.");
            }

            IEnumerable<NewsArticle> newsArticles = await _newsRepository.SearchAsync(request.SearchTerm, cancellationToken);

            IEnumerable<NewsArticleListDto> newsDtos = NewsArticleMapper.ToListDtos(newsArticles);
            
            _logger.LogInformation("Successfully found {Count} news articles matching term: {SearchTerm}", newsArticles.Count(), request.SearchTerm);

            return Result<IEnumerable<NewsArticleListDto>>.Success(newsDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching news articles with term: {SearchTerm}", request.SearchTerm);
            return Result<IEnumerable<NewsArticleListDto>>.Failure("An error occurred while searching news articles.");
        }
    }
} 