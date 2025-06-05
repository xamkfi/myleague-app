using Application.Commands.News;
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

namespace Application.Handlers.News;

/// <summary>
/// Handler for updating an existing news article
/// </summary>
public class UpdateNewsHandler : IRequestHandler<UpdateNewsCommand, Result<NewsDto>>
{
    private readonly INewsRepository _newsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateNewsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateNewsHandler class
    /// </summary>
    /// <param name="newsRepository">The news repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateNewsHandler(INewsRepository newsRepository, IUnitOfWork unitOfWork, ILogger<UpdateNewsHandler> logger)
    {
        _newsRepository = newsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateNewsCommand request
    /// </summary>
    /// <param name="request">The command containing updated news information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated news as a DTO wrapped in a Result</returns>
    public async Task<Result<NewsDto>> Handle(UpdateNewsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating news article with ID: {NewsId}", request.Id);

            // Check if the news article exists
            News? existingNews = await _newsRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingNews == null)
            {
                _logger.LogWarning("Attempt to update non-existent news article with ID: {NewsId}", request.Id);
                return Result<NewsDto>.Failure($"News article with ID '{request.Id}' not found.");
            }

            _logger.LogInformation("Found existing news article: {Title}", existingNews.Title);

            // Update the entity using the mapper
            NewsMapper.UpdateFromCommand(existingNews, request);

            // Save the updated entity
            await _newsRepository.SaveAsync(existingNews, cancellationToken);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to DTO and return
            NewsDto newsDto = NewsMapper.ToDto(existingNews);
            
            _logger.LogInformation("Successfully updated news article with ID: {NewsId}", request.Id);

            return Result<NewsDto>.Success(newsDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating news article with ID: {NewsId}", request.Id);
            return Result<NewsDto>.Failure("An error occurred while updating the news article.");
        }
    }
} 