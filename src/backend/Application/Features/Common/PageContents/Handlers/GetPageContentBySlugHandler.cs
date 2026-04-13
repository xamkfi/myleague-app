using Application.Common;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.PageContents.DTOs;
using Application.Features.Common.PageContents.Mappings;
using Application.Features.Common.PageContents.Queries;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.PageContents.Handlers
{
    public class GetPageContentBySlugHandler : IRequestHandler<GetPageContentBySlugQuery, Result<PageContentDto>>
    {
        private readonly IPageContentRepository _pageContentRepository;
        private readonly ILogger<GetPageContentBySlugHandler> _logger;

        /// <summary>
        /// Initialize new instance of GetPageContentBySlugHandler
        /// </summary>
        /// <param name="pageContentRepository"></param>
        /// <param name="logger"></param>
        public GetPageContentBySlugHandler(IPageContentRepository pageContentRepository, ILogger<GetPageContentBySlugHandler> logger)
        {
            _pageContentRepository = pageContentRepository;
            _logger = logger;
        }

        public async Task<Result<PageContentDto>> Handle(GetPageContentBySlugQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving page content for slug: {Slug}", request.Slug);

                // Validate slug
                if (string.IsNullOrWhiteSpace(request.Slug))
                {
                    _logger.LogWarning("Slug is empty or whitespace");
                    return Result<PageContentDto>.Failure("Slug cannot be empty or whitespace.");
                }

                // Retrieve page content by slug
                PageContent? entity = await _pageContentRepository.GetBySlugAsync(request.Slug, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                if (entity == null)
                {
                    _logger.LogInformation("Page content not found for slug: {Slug}", request.Slug);
                    return Result<PageContentDto>.Failure($"Page content with slug '{request.Slug}' not found.");
                }

                // Map entity to DTO and return success result
                PageContentDto dto = PageContentMapper.ToDto(entity);
                return Result<PageContentDto>.Success(dto);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("GetPageContentBySlug operation cancelled for slug: {Slug}", request.Slug);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving page content for slug: {Slug}", request.Slug);
                return Result<PageContentDto>.Failure("An error occurred while retrieving page content.");
            }
        }
    }
}
