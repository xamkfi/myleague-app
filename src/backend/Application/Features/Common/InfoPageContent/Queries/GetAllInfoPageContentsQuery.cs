// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Common;
using Application.DTOs.Common;
using Application.Interfaces.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Common.InfoPageContent.Queries;

public record GetAllInfoPageContentsQuery() : IRequest<Result<IReadOnlyList<InfoPageContentDto>>>;

public class GetAllInfoPageContentsQueryHandler
    : IRequestHandler<GetAllInfoPageContentsQuery, Result<IReadOnlyList<InfoPageContentDto>>>
{
    private readonly ICommonDbContext _context;

    public GetAllInfoPageContentsQueryHandler(ICommonDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<InfoPageContentDto>>> Handle(
        GetAllInfoPageContentsQuery request,
        CancellationToken cancellationToken)
    {
        List<InfoPageContentDto> items = await _context.InfoPageContents
            .AsNoTracking()
            .OrderBy(x => x.PageSlug)
            .Select(x => new InfoPageContentDto
            {
                Id = x.Id,
                PageSlug = x.PageSlug,
                Title = x.Title,
                ContentHtml = x.ContentHtml,
                LastModifiedBy = x.LastModifiedBy,
                UpdatedAt = x.UpdatedAt ?? x.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<InfoPageContentDto>>.Success(items);
    }
}
