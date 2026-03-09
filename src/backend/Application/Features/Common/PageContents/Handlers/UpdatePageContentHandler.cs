using Application.Common;
using Application.Features.Common.PageContents.Commands;
using Application.Features.Common.PageContents.DTOs;
using Application.Features.Common.PageContents.Mappings;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using Application.Interfaces.Common;


namespace Application.Features.Common.PageContents.Handlers
{
    /// <summary>
    /// Handler for updating or creating page content based on slug.
    /// </summary>
    public class UpdatePageContentHandler : IRequestHandler<UpdatePageContentCommand, Result<PageContentDto>>
    {
        private readonly IPageContentRepository _pageContentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdatePageContentHandler> _logger;

        public UpdatePageContentHandler(IPageContentRepository pageContentRepository, IUnitOfWork unitOfWork, ILogger<UpdatePageContentHandler> logger)
        {
            _pageContentRepository = pageContentRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }


        /// <summary>
        /// Handles the UpdatePageContentCommand request.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<PageContentDto>> Handle(UpdatePageContentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogInformation("Updating page content for slug: {Slug}", request.Slug);

                //Check if content exists for the given slug
                PageContent? existing = await _pageContentRepository.GetBySlugAsync(request.Slug, cancellationToken);

                if (existing == null)
                {
                    // create new
                    existing = new PageContent(Guid.NewGuid(), request.Slug, request.Title, request.ContentHtml, request.ModifiedBy);
                    // Check for cancellation before saving
                    cancellationToken.ThrowIfCancellationRequested();
                    await _pageContentRepository.SaveAsync(existing, cancellationToken);
                    _logger.LogInformation("Created new PageContent for slug: {Slug}", request.Slug);
                }
                else
                {
                    // update
                    existing.UpdateContent(request.Title, request.ContentHtml, request.ModifiedBy);
                    // Check for cancellation before saving
                    cancellationToken.ThrowIfCancellationRequested();
                    await _pageContentRepository.SaveAsync(existing, cancellationToken);
                    _logger.LogInformation("Updated PageContent for slug: {Slug}", request.Slug);
                }

                
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map to DTO and return
                PageContentDto dto = PageContentMapper.ToDto(existing);
                return Result<PageContentDto>.Success(dto);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("UpdatePageContent operation cancelled for slug: {Slug}", request.Slug);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating page content for slug: {Slug}", request.Slug);
                return Result<PageContentDto>.Failure("An error occurred while updating page content.");
            }
        }
    }
}
