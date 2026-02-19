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
/// Handler for retrieving news articles by tag
/// </summary>
public class GetNewsArticlesByTagHandler : IRequestHandler<GetNewsArticlesByTagQuery, Result<IEnumerable<NewsArticleListDto>>>
{
    private readonly INewsArticleRepository _newsRepository;
    private readonly ILogger<GetNewsArticlesByTagHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetNewsArticlesByTagHandler class
    /// </summary>
    /// <param name="newsRepository">The news repository</param>
    /// <param name="logger">The logger</param>
    public GetNewsArticlesByTagHandler(INewsArticleRepository newsRepository, ILogger<GetNewsArticlesByTagHandler> logger)
    {
        _newsRepository = newsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetNewsArticlesByTagQuery request
    /// </summary>
    /// <param name="request">The query containing the tag</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A collection of news articles by tag as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<NewsArticleListDto>>> Handle(GetNewsArticlesByTagQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving news articles by tag: {Tag}", request.Tag);

            // Validate tag parameter
            if (string.IsNullOrWhiteSpace(request.Tag))
            {
                _logger.LogWarning("Tag parameter is empty or whitespace");
                return Result<IEnumerable<NewsArticleListDto>>.Failure("Tag cannot be empty or whitespace.");
            }

            IEnumerable<NewsArticle> newsArticles = await _newsRepository.GetByTagAsync(request.Tag, cancellationToken);

            IEnumerable<NewsArticleListDto> newsDtos = NewsArticleMapper.ToListDtos(newsArticles);
            
            _logger.LogInformation("Successfully retrieved {Count} news articles by tag: {Tag}", newsArticles.Count(), request.Tag);

            return Result<IEnumerable<NewsArticleListDto>>.Success(newsDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving news articles by tag: {Tag}", request.Tag);
            return Result<IEnumerable<NewsArticleListDto>>.Failure("An error occurred while retrieving news articles by tag.");
        }
    }
} 