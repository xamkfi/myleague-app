using Application.Commands.NewsArticles;
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
/// Handler for creating a new news article
/// </summary>
public class CreateNewsArticleHandler : IRequestHandler<CreateNewsArticleCommand, Result<NewsArticleDto>>
{
    private readonly INewsArticleRepository _newsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateNewsArticleHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateNewsArticleHandler class
    /// </summary>
    /// <param name="newsRepository">The news repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public CreateNewsArticleHandler(INewsArticleRepository newsRepository, IUnitOfWork unitOfWork, ILogger<CreateNewsArticleHandler> logger)
    {
        _newsRepository = newsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateNewsArticleCommand request
    /// </summary>
    /// <param name="request">The command containing news information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created news as a DTO wrapped in a Result</returns>
    public async Task<Result<NewsArticleDto>> Handle(CreateNewsArticleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check for cancellation before starting
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Creating new news article with title: {Title}", request.Title);

            // Create the news entity using the mapper
            NewsArticle newsArticle = NewsArticleMapper.ToEntity(request);

            _logger.LogInformation("Creating news article with ID: {NewsId}", newsArticle.Id);

            // Check for cancellation before database operations
            cancellationToken.ThrowIfCancellationRequested();

            // Save the entity
            await _newsRepository.SaveAsync(newsArticle, cancellationToken);
            
            // Check for cancellation before committing transaction
            cancellationToken.ThrowIfCancellationRequested();
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to DTO and return
            NewsArticleDto newsDto = NewsArticleMapper.ToDto(newsArticle);
            
            _logger.LogInformation("Successfully created news article with ID: {NewsId}", newsArticle.Id);

            return Result<NewsArticleDto>.Success(newsDto);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("News article creation was cancelled for title: {Title}", request.Title);
            throw; // Re-throw to let the framework handle it
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating news article: {Title}", request.Title);
            return Result<NewsArticleDto>.Failure("An error occurred while creating the news article.");
        }
    }
} 