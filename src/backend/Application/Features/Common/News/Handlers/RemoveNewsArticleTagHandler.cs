using Application.Commands.NewsArticles;
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
/// Handler for removing a tag from a news article
/// </summary>
public class RemoveNewsArticleTagHandler : IRequestHandler<RemoveNewsArticleTagCommand, Result<bool>>
{
    private readonly INewsArticleRepository _newsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveNewsArticleTagHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the RemoveNewsTagHandler class
    /// </summary>
    /// <param name="newsRepository">The news repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public RemoveNewsArticleTagHandler(INewsArticleRepository newsRepository, IUnitOfWork unitOfWork, ILogger<RemoveNewsArticleTagHandler> logger)
    {
        _newsRepository = newsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the RemoveNewsTagCommand request
    /// </summary>
    /// <param name="request">The command containing the news ID and tag to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A Result indicating success or failure</returns>
    public async Task<Result<bool>> Handle(RemoveNewsArticleTagCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Removing tag '{Tag}' from news article with ID: {NewsId}", request.Tag, request.NewsId);

            // Check if the news article exists
            NewsArticle? existingNews = await _newsRepository.GetByIdAsync(request.NewsId, cancellationToken);
            if (existingNews == null)
            {
                _logger.LogWarning("Attempt to remove tag from non-existent news article with ID: {NewsId}", request.NewsId);
                return Result<bool>.Failure($"News article with ID '{request.NewsId}' not found.");
            }

            // Check if tag exists
            if (!existingNews.Tags.Contains(request.Tag, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Tag '{Tag}' does not exist on news article with ID: {NewsId}", request.Tag, request.NewsId);
                return Result<bool>.Success(true);
            }

            _logger.LogInformation("Removing tag '{Tag}' from news article: {Title}", request.Tag, existingNews.Title);

            // Remove the tag
            existingNews.RemoveTag(request.Tag);

            // Save the updated entity
            await _newsRepository.SaveAsync(existingNews, cancellationToken);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Successfully removed tag '{Tag}' from news article with ID: {NewsId}", request.Tag, request.NewsId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing tag '{Tag}' from news article with ID: {NewsId}", request.Tag, request.NewsId);
            return Result<bool>.Failure("An error occurred while removing the tag from the news article.");
        }
    }
} 