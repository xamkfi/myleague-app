using Application.Queries.NewsArticles;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Application.Common;
using Domain.Common;
using Application.Handlers.Common;
using Application.Services.Common;
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
public class GetAllNewsArticlesHandler : BasePagedQueryHandler<GetAllNewsArticlesQuery, NewsArticleListDto>, 
    IRequestHandler<GetAllNewsArticlesQuery, Result<PagedResult<NewsArticleListDto>>>
{
    private readonly INewsArticleRepository _newsRepository;

    /// <summary>
    /// Initializes a new instance of the GetAllNewsArticlesHandler class
    /// </summary>
    /// <param name="newsRepository">The news repository</param>
    /// <param name="paginationService">The pagination service</param>
    /// <param name="logger">The logger</param>
    public GetAllNewsArticlesHandler(
        INewsArticleRepository newsRepository,
        IPaginationService paginationService,
        ILogger<GetAllNewsArticlesHandler> logger) : base(paginationService, logger)
    {
        _newsRepository = newsRepository;
    }

    /// <summary>
    /// Handles the GetAllNewsArticlesQuery request
    /// </summary>
    /// <param name="request">The query containing pagination and filtering parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated collection of news articles as DTOs wrapped in a Result</returns>
    public async Task<Result<PagedResult<NewsArticleListDto>>> Handle(GetAllNewsArticlesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check for cancellation before starting
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Retrieving news articles - Page: {Page}, PageSize: {PageSize}, Category: {Category}, SportCategory: {SportCategory}, Search: {Search}, Author: {Author}, IncludeArchived: {IncludeArchived}", 
                request.Page, request.PageSize, request.Category, request.SportCategory, request.Search, request.Author, request.IncludeArchived);

            // Validate pagination parameters using base handler
            Result<PaginationValidationResult> validationResult = ValidatePaginationParameters(
                request.Page, request.PageSize, GetAllNewsArticlesQuery.ResourceKey);
            
            if (validationResult.IsFailure)
            {
                return Result<PagedResult<NewsArticleListDto>>.Failure(validationResult.Error!);
            }

            int actualPageSize = validationResult.Data!.ActualPageSize;

            // Check for cancellation before database operations
            cancellationToken.ThrowIfCancellationRequested();

            // Execute operations sequentially instead of in parallel
            IEnumerable<NewsArticle> newsArticles = await _newsRepository.GetAllAsync(
                request.Page, 
                actualPageSize, 
                request.Category, 
                request.SportCategory,
                request.Search,
                request.Author,
                request.IncludeArchived, 
                cancellationToken);

            int totalCount = await _newsRepository.GetCountAsync(
                request.Category,
                request.SportCategory,
                request.Search,
                request.Author,
                request.IncludeArchived,
                cancellationToken);

            // Check for cancellation after database operations
            cancellationToken.ThrowIfCancellationRequested();

            IEnumerable<NewsArticleListDto> newsDtos = NewsArticleMapper.ToListDtos(newsArticles);
            
            PagedResult<NewsArticleListDto> pagedResult = CreatePagedResult(
                newsDtos, 
                totalCount, 
                request.Page, 
                actualPageSize);
            
            _logger.LogInformation("Successfully retrieved {Count} news articles out of {TotalCount} total", 
                newsArticles.Count(), totalCount);

            return Result<PagedResult<NewsArticleListDto>>.Success(pagedResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("News articles retrieval was cancelled - Page: {Page}, PageSize: {PageSize}", 
                request.Page, request.PageSize);
            throw; // Re-throw to let the framework handle it
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving news articles");
            return Result<PagedResult<NewsArticleListDto>>.Failure("An error occurred while retrieving news articles.");
        }
    }
} 
