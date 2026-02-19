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
/// Handler for archiving a news article
/// </summary>
public class ArchiveNewsArticleHandler : IRequestHandler<ArchiveNewsArticleCommand, Result<bool>>
{
    private readonly INewsArticleRepository _newsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ArchiveNewsArticleHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the ArchiveNewsHandler class
    /// </summary>
    /// <param name="newsRepository">The news repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public ArchiveNewsArticleHandler(INewsArticleRepository newsRepository, IUnitOfWork unitOfWork, ILogger<ArchiveNewsArticleHandler> logger)
    {
        _newsRepository = newsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the ArchiveNewsCommand request
    /// </summary>
    /// <param name="request">The command containing the news ID to archive</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A Result indicating success or failure</returns>
    public async Task<Result<bool>> Handle(ArchiveNewsArticleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check for cancellation before starting
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Archiving news article with ID: {NewsId}", request.Id);

            // Check if the news article exists
            NewsArticle? existingNews = await _newsRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingNews == null)
            {
                _logger.LogWarning("Attempt to archive non-existent news article with ID: {NewsId}", request.Id);
                return Result<bool>.Failure($"News article with ID '{request.Id}' not found.");
            }

            // Check for cancellation after database read
            cancellationToken.ThrowIfCancellationRequested();

            // Check if already archived
            if (existingNews.IsArchived)
            {
                _logger.LogInformation("News article with ID: {NewsId} is already archived", request.Id);
                return Result<bool>.Success(true);
            }

            _logger.LogInformation("Archiving news article: {Title}", existingNews.Title);

            // Archive the news article
            existingNews.Archive();

            // Check for cancellation before saving
            cancellationToken.ThrowIfCancellationRequested();

            // Save the updated entity
            await _newsRepository.SaveAsync(existingNews, cancellationToken);
            
            // Check for cancellation before committing transaction
            cancellationToken.ThrowIfCancellationRequested();
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Successfully archived news article with ID: {NewsId}", request.Id);

            return Result<bool>.Success(true);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Archive operation was cancelled for news article ID: {NewsId}", request.Id);
            throw; // Re-throw to let the framework handle it
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while archiving news article with ID: {NewsId}", request.Id);
            return Result<bool>.Failure("An error occurred while archiving the news article.");
        }
    }
} 