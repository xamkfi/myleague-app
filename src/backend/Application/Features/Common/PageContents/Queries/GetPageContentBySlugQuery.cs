using Application.Common;
using Application.Features.Common.PageContents.DTOs;
using MediatR;

namespace Application.Features.Common.PageContents.Queries;

/// <summary>
/// Query for retrieving page content by slug
/// </summary>
/// <param name="Slug"></param>
public record GetPageContentBySlugQuery(
    string Slug) : IRequest<Result<PageContentDto>>;
