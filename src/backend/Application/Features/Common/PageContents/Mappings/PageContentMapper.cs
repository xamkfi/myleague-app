using Domain.Entities.Common;
using Application.Features.Common.PageContents.DTOs;
using System.Security.Cryptography.X509Certificates;
using Application.Features.Common.PageContents.Commands;

namespace Application.Features.Common.PageContents.Mappings
{
    /// <summary>
    /// Mapper class for PageContent entity and raleted DTOs
    /// </summary>
    public static class PageContentMapper
    {
        public static PageContentDto ToDto(PageContent pageContent)
        {
            if (pageContent == null)
                throw new ArgumentNullException(nameof(pageContent));

            return new PageContentDto
            {
                Id = pageContent.Id,
                PageSlug = pageContent.PageSlug,
                Title = pageContent.Title,
                ContentHtml = pageContent.ContentHtml,
                LastModifiedBy = pageContent.LastModifiedBy,
                UpdatedAt = pageContent.UpdatedAt
            };
        }      
    }
}
