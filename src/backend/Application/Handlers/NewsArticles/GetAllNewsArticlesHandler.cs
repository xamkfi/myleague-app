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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.NewsArticles;

/// <summary>
/// Handler for retrieving all news articles with pagination and filtering
/// </summary>
public class GetAllNewsArticlesHandler : IRequestHandler<GetAllNewsArticlesQuery, Result<IEnumerable<NewsArticleListDto>>>
{
    private readonly INewsArticleRepository _newsRepository;
    private readonly ILogger<GetAllNewsArticlesHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetAllNewsArticlesHandler class
    /// </summary>
    /// <param name="newsRepository">The news repository</param>
    /// <param name="logger">The logger</param>
    public GetAllNewsArticlesHandler(INewsArticleRepository newsRepository, ILogger<GetAllNewsArticlesHandler> logger)
    {
        _newsRepository = newsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetAllNewsArticlesQuery request
    /// </summary>
    /// <param name="request">The query containing pagination and filtering parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A collection of news articles as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<NewsArticleListDto>>> Handle(GetAllNewsArticlesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving news articles - Page: {Page}, PageSize: {PageSize}, Category: {Category}, SportCategory: {SportCategory}, Author: {Author}, IncludeArchived: {IncludeArchived}", 
                request.Page, request.PageSize, request.Category, request.SportCategory, request.Author, request.IncludeArchived);

            // Validate pagination parameters
            if (request.Page < 1)
            {
                _logger.LogWarning("Invalid page number: {Page}", request.Page);
                return Result<IEnumerable<NewsArticleListDto>>.Failure("Page number must be greater than 0.");
            }

            if (request.PageSize < 1 || request.PageSize > 100)
            {
                _logger.LogWarning("Invalid page size: {PageSize}", request.PageSize);
                return Result<IEnumerable<NewsArticleListDto>>.Failure("Page size must be between 1 and 100.");
            }

            IEnumerable<NewsArticle> newsArticles = await _newsRepository.GetAllAsync(
                request.Page, 
                request.PageSize, 
                request.Category, 
                request.SportCategory, 
                request.Author, 
                request.IncludeArchived, 
                cancellationToken);

            IEnumerable<NewsArticleListDto> newsDtos = NewsArticleMapper.ToListDtos(newsArticles);
            
            _logger.LogInformation("Successfully retrieved {Count} news articles", newsArticles.Count());

            return Result<IEnumerable<NewsArticleListDto>>.Success(newsDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving news articles");
            return Result<IEnumerable<NewsArticleListDto>>.Failure("An error occurred while retrieving news articles.");
        }
    }
} 