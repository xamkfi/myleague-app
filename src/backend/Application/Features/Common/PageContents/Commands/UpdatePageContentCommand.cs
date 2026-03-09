using Application.Common;
using Application.Features.Common.PageContents.DTOs;
using MediatR;

namespace Application.Features.Common.PageContents.Commands;

/// <summary>
/// Command for updating or creating page content based on slug. If content with the given slug exists, it will be updated; otherwise, a new content will be created.
/// </summary>
public record UpdatePageContentCommand(
     string Slug,
     string Title,
     string ContentHtml,
     string? ModifiedBy = null) : IRequest<Result<PageContentDto>>;
