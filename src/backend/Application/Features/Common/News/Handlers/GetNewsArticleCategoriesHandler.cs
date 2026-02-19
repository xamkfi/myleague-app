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
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Common.News.Handlers;

/// <summary>
/// Handler for retrieving all available news article categories
/// </summary>
public class GetNewsArticleCategoriesHandler : IRequestHandler<GetNewsArticleCategoriesQuery, Result<IEnumerable<NewsArticleCategoryDto>>>
{
    private readonly ILogger<GetNewsArticleCategoriesHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetNewsArticleCategoriesHandler class
    /// </summary>
    /// <param name="logger">The logger</param>
    public GetNewsArticleCategoriesHandler(ILogger<GetNewsArticleCategoriesHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetNewsArticleCategoriesQuery request
    /// </summary>
    /// <param name="request">The query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A collection of available categories as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<NewsArticleCategoryDto>>> Handle(GetNewsArticleCategoriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving all news article categories");

            // Get categories from mapper - this is a synchronous operation
            IEnumerable<NewsArticleCategoryDto> categories = NewsArticleMapper.GetCategoryDtos();
            
            _logger.LogInformation("Successfully retrieved {Count} news article categories", categories.Count());

            return await Task.FromResult(Result<IEnumerable<NewsArticleCategoryDto>>.Success(categories));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving news article categories");
            return Result<IEnumerable<NewsArticleCategoryDto>>.Failure("An error occurred while retrieving news article categories.");
        }
    }
} 
