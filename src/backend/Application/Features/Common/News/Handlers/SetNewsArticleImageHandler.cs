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
/// Handler for setting an image on a news article
/// </summary>
public class SetNewsArticleImageHandler : IRequestHandler<SetNewsArticleImageCommand, Result<bool>>
{
    private readonly INewsArticleRepository _newsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetNewsArticleImageHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the SetNewsImageHandler class
    /// </summary>
    /// <param name="newsRepository">The news repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public SetNewsArticleImageHandler(INewsArticleRepository newsRepository, IUnitOfWork unitOfWork, ILogger<SetNewsArticleImageHandler> logger)
    {
        _newsRepository = newsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the SetNewsImageCommand request
    /// </summary>
    /// <param name="request">The command containing the news ID and image URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A Result indicating success or failure</returns>
    public async Task<Result<bool>> Handle(SetNewsArticleImageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Setting image '{ImageUrl}' for news article with ID: {NewsId}", request.ImageUrl, request.NewsId);

            // Validate the image URL
            if (string.IsNullOrWhiteSpace(request.ImageUrl))
            {
                _logger.LogWarning("Attempt to set empty or whitespace image URL for news article with ID: {NewsId}", request.NewsId);
                return Result<bool>.Failure("Image URL cannot be empty or whitespace.");
            }

            if (!Uri.TryCreate(request.ImageUrl, UriKind.Absolute, out Uri? imageUri))
            {
                _logger.LogWarning("Invalid image URL '{ImageUrl}' for news article with ID: {NewsId}", request.ImageUrl, request.NewsId);
                return Result<bool>.Failure("Invalid image URL format.");
            }

            // Check if the news article exists
            NewsArticle? existingNews = await _newsRepository.GetByIdAsync(request.NewsId, cancellationToken);
            if (existingNews == null)
            {
                _logger.LogWarning("Attempt to set image on non-existent news article with ID: {NewsId}", request.NewsId);
                return Result<bool>.Failure($"News article with ID '{request.NewsId}' not found.");
            }

            _logger.LogInformation("Setting image for news article: {Title}", existingNews.Title);

            // Set the image
            existingNews.SetImage(imageUri);

            // Save the updated entity
            await _newsRepository.SaveAsync(existingNews, cancellationToken);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Successfully set image '{ImageUrl}' for news article with ID: {NewsId}", request.ImageUrl, request.NewsId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting image '{ImageUrl}' for news article with ID: {NewsId}", request.ImageUrl, request.NewsId);
            return Result<bool>.Failure("An error occurred while setting the image for the news article.");
        }
    }
} 