// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using Application.Common;
using Application.DTOs.Common;
using Application.Interfaces.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Common.PageContent.Queries
{
    public record GetPageContentBySlugQuery(string Slug) : IRequest<Result<PageContentDto>>;

    public class GetPageContentBySlugQueryHandler : IRequestHandler<GetPageContentBySlugQuery, Result<PageContentDto>>
    {
        private readonly ICommonDbContext _context;

        public GetPageContentBySlugQueryHandler(ICommonDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PageContentDto>> Handle(GetPageContentBySlugQuery request, CancellationToken cancellationToken)
        {
            Domain.Entities.Common.PageContent? entity = await _context.PageContents
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PageSlug == request.Slug, cancellationToken);

            if (entity == null)
            {
                return Result<PageContentDto>.Failure($"Page content with slug '{request.Slug}' not found.");
            }

            PageContentDto dto = new()
            {
                Id = entity.Id,
                PageSlug = entity.PageSlug,
                Title = entity.Title,
                ContentHtml = entity.ContentHtml,
                LastModifiedBy = entity.LastModifiedBy,
                UpdatedAt = entity.UpdatedAt ?? entity.CreatedAt
            };

            return Result<PageContentDto>.Success(dto);
        }
    }
}
