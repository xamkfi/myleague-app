using Application.Features.Common.News.Queries;
using Application.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Common.News.Handlers;

/// <summary>
/// Handler for retrieving all used tags in news articles
/// </summary>
public class GetNewsArticleTagsHandler : IRequestHandler<GetNewsArticleTagsQuery, Result<IEnumerable<string>>>
{
    private readonly INewsArticleRepository _newsRepository;
    private readonly ILogger<GetNewsArticleTagsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetNewsArticleTagsHandler class
    /// </summary>
    /// <param name="newsRepository">The news repository</param>
    /// <param name="logger">The logger</param>
    public GetNewsArticleTagsHandler(INewsArticleRepository newsRepository, ILogger<GetNewsArticleTagsHandler> logger)
    {
        _newsRepository = newsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetNewsArticleTagsQuery request
    /// </summary>
    /// <param name="request">The query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A collection of all used tags wrapped in a Result</returns>
    public async Task<Result<IEnumerable<string>>> Handle(GetNewsArticleTagsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving all used tags in news articles");

            IEnumerable<string> tags = await _newsRepository.GetAllTagsAsync(cancellationToken);
            
            _logger.LogInformation("Successfully retrieved {Count} unique tags", tags.Count());

            return Result<IEnumerable<string>>.Success(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving news article tags");
            return Result<IEnumerable<string>>.Failure("An error occurred while retrieving news article tags.");
        }
    }
} 
