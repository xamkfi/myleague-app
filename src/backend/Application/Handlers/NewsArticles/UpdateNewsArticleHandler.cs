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
/// Handler for updating an existing news article
/// </summary>
public class UpdateNewsArticleHandler : IRequestHandler<UpdateNewsArticleCommand, Result<NewsArticleDto>>
{
    private readonly INewsArticleRepository _newsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateNewsArticleHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateNewsArticleHandler class
    /// </summary>
    /// <param name="newsRepository">The news repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateNewsArticleHandler(INewsArticleRepository newsRepository, IUnitOfWork unitOfWork, ILogger<UpdateNewsArticleHandler> logger)
    {
        _newsRepository = newsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateNewsArticleCommand request
    /// </summary>
    /// <param name="request">The command containing updated news information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated news as a DTO wrapped in a Result</returns>
    public async Task<Result<NewsArticleDto>> Handle(UpdateNewsArticleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating news article with ID: {NewsId}", request.Id);

            // Check if the news article exists
            NewsArticle? existingNews = await _newsRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingNews == null)
            {
                _logger.LogWarning("Attempt to update non-existent news article with ID: {NewsId}", request.Id);
                return Result<NewsArticleDto>.Failure($"News article with ID '{request.Id}' not found.");
            }

            _logger.LogInformation("Found existing news article: {Title}", existingNews.Title);

            // Update the entity using the mapper
            NewsArticleMapper.UpdateFromCommand(existingNews, request);

            // Save the updated entity
            await _newsRepository.SaveAsync(existingNews, cancellationToken);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to DTO and return
            NewsArticleDto newsDto = NewsArticleMapper.ToDto(existingNews);
            
            _logger.LogInformation("Successfully updated news article with ID: {NewsId}", request.Id);

            return Result<NewsArticleDto>.Success(newsDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating news article with ID: {NewsId}", request.Id);
            return Result<NewsArticleDto>.Failure("An error occurred while updating the news article.");
        }
    }
} 