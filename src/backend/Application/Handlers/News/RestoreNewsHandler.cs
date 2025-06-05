using Application.Commands.News;
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
/// Handler for restoring an archived news article
/// </summary>
public class RestoreNewsHandler : IRequestHandler<RestoreNewsCommand, Result<bool>>
{
    private readonly INewsRepository _newsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RestoreNewsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the RestoreNewsHandler class
    /// </summary>
    /// <param name="newsRepository">The news repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public RestoreNewsHandler(INewsRepository newsRepository, IUnitOfWork unitOfWork, ILogger<RestoreNewsHandler> logger)
    {
        _newsRepository = newsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the RestoreNewsCommand request
    /// </summary>
    /// <param name="request">The command containing the news ID to restore</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A Result indicating success or failure</returns>
    public async Task<Result<bool>> Handle(RestoreNewsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Restoring news article with ID: {NewsId}", request.Id);

            // Check if the news article exists
            News? existingNews = await _newsRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingNews == null)
            {
                _logger.LogWarning("Attempt to restore non-existent news article with ID: {NewsId}", request.Id);
                return Result<bool>.Failure($"News article with ID '{request.Id}' not found.");
            }

            // Check if already restored (not archived)
            if (!existingNews.IsArchived)
            {
                _logger.LogInformation("News article with ID: {NewsId} is already active (not archived)", request.Id);
                return Result<bool>.Success(true);
            }

            _logger.LogInformation("Restoring news article: {Title}", existingNews.Title);

            // Restore the news article
            existingNews.Restore();

            // Save the updated entity
            await _newsRepository.SaveAsync(existingNews, cancellationToken);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Successfully restored news article with ID: {NewsId}", request.Id);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while restoring news article with ID: {NewsId}", request.Id);
            return Result<bool>.Failure("An error occurred while restoring the news article.");
        }
    }
} 