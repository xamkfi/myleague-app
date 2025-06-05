using Application.Queries.NewsArticles;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Application.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.NewsArticles;

/// <summary>
/// Handler for retrieving a news article by its ID
/// </summary>
public class GetNewsArticleByIdHandler : IRequestHandler<GetNewsArticleByIdQuery, Result<NewsArticleDto>>
{
    private readonly INewsArticleRepository _newsRepository;
    private readonly ILogger<GetNewsArticleByIdHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetNewsArticleByIdHandler class
    /// </summary>
    /// <param name="newsRepository">The news repository</param>
    /// <param name="logger">The logger</param>
    public GetNewsArticleByIdHandler(INewsArticleRepository newsRepository, ILogger<GetNewsArticleByIdHandler> logger)
    {
        _newsRepository = newsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetNewsArticleByIdQuery request
    /// </summary>
    /// <param name="request">The query containing the news article ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The news article as a DTO wrapped in a Result</returns>
    public async Task<Result<NewsArticleDto>> Handle(GetNewsArticleByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving news article with ID: {NewsId}", request.NewsId);

            NewsArticle? newsArticle = await _newsRepository.GetByIdAsync(request.NewsId, cancellationToken);
            if (newsArticle == null)
            {
                _logger.LogWarning("News article with ID: {NewsId} not found", request.NewsId);
                return Result<NewsArticleDto>.Failure($"News article with ID '{request.NewsId}' not found.");
            }

            NewsArticleDto newsDto = NewsArticleMapper.ToDto(newsArticle);
            
            _logger.LogInformation("Successfully retrieved news article: {Title}", newsArticle.Title);

            return Result<NewsArticleDto>.Success(newsDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving news article with ID: {NewsId}", request.NewsId);
            return Result<NewsArticleDto>.Failure("An error occurred while retrieving the news article.");
        }
    }
} 