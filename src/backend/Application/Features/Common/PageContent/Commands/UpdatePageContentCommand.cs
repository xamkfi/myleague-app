// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.DTOs.Common;
using Application.Interfaces.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Common.PageContent.Commands;

public record UpdatePageContentCommand(
    string Slug,
    string Title,
    string ContentHtml,
    string? LastModifiedBy
) : IRequest<PageContentDto>;

public class UpdatePageContentCommandHandler : IRequestHandler<UpdatePageContentCommand, PageContentDto>
{
    private readonly ICommonDbContext _context;

    public UpdatePageContentCommandHandler(ICommonDbContext context)
    {
        _context = context;
    }

    public async Task<PageContentDto> Handle(UpdatePageContentCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Common.PageContent entity = await _context.PageContents
            .FirstOrDefaultAsync(x => x.PageSlug == request.Slug, cancellationToken);

        if (entity == null)
        {
            entity = new Domain.Entities.Common.PageContent
            {
                PageSlug = request.Slug,
                Title = request.Title,
                ContentHtml = request.ContentHtml,
                LastModifiedBy = request.LastModifiedBy
            };

            _context.PageContents.Add(entity);
        }
        else
        {
            entity.Title = request.Title;
            entity.ContentHtml = request.ContentHtml;
            entity.LastModifiedBy = request.LastModifiedBy;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new PageContentDto
        {
            Id = entity.Id,
            PageSlug = entity.PageSlug,
            Title = entity.Title,
            ContentHtml = entity.ContentHtml,
            LastModifiedBy = entity.LastModifiedBy,
            UpdatedAt = entity.UpdatedAt ?? entity.CreatedAt
        };
    }
}
