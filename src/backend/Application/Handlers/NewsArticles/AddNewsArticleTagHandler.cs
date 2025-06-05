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
/// Handler for adding a tag to a news article
/// </summary>
public class AddNewsArticleTagHandler : IRequestHandler<AddNewsArticleTagCommand, Result<bool>>
{
    private readonly INewsArticleRepository _newsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddNewsArticleTagHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the AddNewsTagHandler class
    /// </summary>
    /// <param name="newsRepository">The news repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public AddNewsArticleTagHandler(INewsArticleRepository newsRepository, IUnitOfWork unitOfWork, ILogger<AddNewsArticleTagHandler> logger)
    {
        _newsRepository = newsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the AddNewsTagCommand request
    /// </summary>
    /// <param name="request">The command containing the news ID and tag to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A Result indicating success or failure</returns>
    public async Task<Result<bool>> Handle(AddNewsArticleTagCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Adding tag '{Tag}' to news article with ID: {NewsId}", request.Tag, request.NewsId);

            // Validate the tag input
            if (string.IsNullOrWhiteSpace(request.Tag))
            {
                _logger.LogWarning("Attempt to add empty or whitespace tag to news article with ID: {NewsId}", request.NewsId);
                return Result<bool>.Failure("Tag cannot be empty or whitespace.");
            }

            // Check if the news article exists
            NewsArticle? existingNews = await _newsRepository.GetByIdAsync(request.NewsId, cancellationToken);
            if (existingNews == null)
            {
                _logger.LogWarning("Attempt to add tag to non-existent news article with ID: {NewsId}", request.NewsId);
                return Result<bool>.Failure($"News article with ID '{request.NewsId}' not found.");
            }

            // Check if tag already exists
            if (existingNews.Tags.Contains(request.Tag, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Tag '{Tag}' already exists on news article with ID: {NewsId}", request.Tag, request.NewsId);
                return Result<bool>.Success(true);
            }

            _logger.LogInformation("Adding tag '{Tag}' to news article: {Title}", request.Tag, existingNews.Title);

            // Add the tag
            existingNews.AddTag(request.Tag);

            // Save the updated entity
            await _newsRepository.SaveAsync(existingNews, cancellationToken);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Successfully added tag '{Tag}' to news article with ID: {NewsId}", request.Tag, request.NewsId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding tag '{Tag}' to news article with ID: {NewsId}", request.Tag, request.NewsId);
            return Result<bool>.Failure("An error occurred while adding the tag to the news article.");
        }
    }
} 