// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Common;
using Application.DTOs.Common;
using Application.Interfaces.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Common.InfoPageContent.Queries;

public record GetInfoPageContentBySlugQuery(string Slug) : IRequest<Result<InfoPageContentDto>>;

public class GetInfoPageContentBySlugQueryHandler
    : IRequestHandler<GetInfoPageContentBySlugQuery, Result<InfoPageContentDto>>
{
    private readonly ICommonDbContext _context;

    public GetInfoPageContentBySlugQueryHandler(ICommonDbContext context)
    {
        _context = context;
    }

    public async Task<Result<InfoPageContentDto>> Handle(
        GetInfoPageContentBySlugQuery request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Common.InfoPageContent? entity = await _context.InfoPageContents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PageSlug == request.Slug, cancellationToken);

        if (entity == null)
        {
            return Result<InfoPageContentDto>.Failure(
                $"Info page content with slug '{request.Slug}' not found.");
        }

        return Result<InfoPageContentDto>.Success(MapToDto(entity));
    }

    internal static InfoPageContentDto MapToDto(Domain.Entities.Common.InfoPageContent entity)
    {
        return new InfoPageContentDto
        {
            Id = entity.Id,
            PageSlug = entity.PageSlug,
            Title = entity.Title,
            ContentHtml = entity.ContentHtml,
            LastModifiedBy = entity.LastModifiedBy,
            UpdatedAt = entity.UpdatedAt ?? entity.CreatedAt,
        };
    }
}
