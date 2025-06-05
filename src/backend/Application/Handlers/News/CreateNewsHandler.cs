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
/// Handler for creating a new news article
/// </summary>
public class CreateNewsHandler : IRequestHandler<CreateNewsCommand, Result<NewsDto>>
{
    private readonly INewsRepository _newsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateNewsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateNewsHandler class
    /// </summary>
    /// <param name="newsRepository">The news repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public CreateNewsHandler(INewsRepository newsRepository, IUnitOfWork unitOfWork, ILogger<CreateNewsHandler> logger)
    {
        _newsRepository = newsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateNewsCommand request
    /// </summary>
    /// <param name="request">The command containing news information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created news as a DTO wrapped in a Result</returns>
    public async Task<Result<NewsDto>> Handle(CreateNewsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating new news article with title: {Title}", request.Title);

            // Create the news entity using the mapper
            News news = NewsMapper.ToEntity(request);

            _logger.LogInformation("Creating news article with ID: {NewsId}", news.Id);

            // Save the entity
            await _newsRepository.SaveAsync(news, cancellationToken);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to DTO and return
            NewsDto newsDto = NewsMapper.ToDto(news);
            
            _logger.LogInformation("Successfully created news article with ID: {NewsId}", news.Id);

            return Result<NewsDto>.Success(newsDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating news article: {Title}", request.Title);
            return Result<NewsDto>.Failure("An error occurred while creating the news article.");
        }
    }
} 