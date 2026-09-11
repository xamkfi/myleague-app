using Application.Features.Common.News.Queries;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;
using Application.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Common.News.Handlers;

/// <summary>
/// Handler for retrieving recent news articles
/// </summary>
public class GetRecentNewsArticlesHandler : IRequestHandler<GetRecentNewsArticlesQuery, Result<IEnumerable<NewsArticleListDto>>>
{
    private readonly INewsArticleRepository _newsRepository;
    private readonly ILogger<GetRecentNewsArticlesHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetRecentNewsArticlesHandler class
    /// </summary>
    /// <param name="newsRepository">The news repository</param>
    /// <param name="logger">The logger</param>
    public GetRecentNewsArticlesHandler(INewsArticleRepository newsRepository, ILogger<GetRecentNewsArticlesHandler> logger)
    {
        _newsRepository = newsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetRecentNewsArticlesQuery request
    /// </summary>
    /// <param name="request">The query containing recent news parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A collection of recent news articles as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<NewsArticleListDto>>> Handle(GetRecentNewsArticlesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving recent news articles - Count: {Count}, IncludeArchived: {IncludeArchived}", 
                request.Count, request.IncludeArchived);

            // Validate count parameter
            if (request.Count < 1 || request.Count > 100)
            {
                _logger.LogWarning("Invalid count: {Count}", request.Count);
                return Result<IEnumerable<NewsArticleListDto>>.Failure("Count must be between 1 and 100.");
            }

            IEnumerable<NewsArticle> newsArticles = await _newsRepository.GetRecentAsync(
                request.Count, 
                request.IncludeArchived, 
                cancellationToken);

            IEnumerable<NewsArticleListDto> newsDtos = NewsArticleMapper.ToListDtos(newsArticles);
            
            _logger.LogInformation("Successfully retrieved {Count} recent news articles", newsArticles.Count());

            return Result<IEnumerable<NewsArticleListDto>>.Success(newsDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving recent news articles");
            return Result<IEnumerable<NewsArticleListDto>>.Failure("An error occurred while retrieving recent news articles.");
        }
    }
} 
