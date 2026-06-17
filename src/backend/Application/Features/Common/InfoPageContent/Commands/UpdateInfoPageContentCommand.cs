using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.InfoPageContent.Queries;
using Application.Interfaces.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Common.InfoPageContent.Commands;

public record UpdateInfoPageContentCommand(
    string Slug,
    string Title,
    string ContentHtml,
    string? LastModifiedBy
) : IRequest<Result<InfoPageContentDto>>;

public class UpdateInfoPageContentCommandHandler
    : IRequestHandler<UpdateInfoPageContentCommand, Result<InfoPageContentDto>>
{
    private readonly ICommonDbContext _context;

    public UpdateInfoPageContentCommandHandler(ICommonDbContext context)
    {
        _context = context;
    }

    public async Task<Result<InfoPageContentDto>> Handle(
        UpdateInfoPageContentCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Common.InfoPageContent? entity = await _context.InfoPageContents
            .FirstOrDefaultAsync(x => x.PageSlug == request.Slug, cancellationToken);

        if (entity == null)
        {
            entity = new Domain.Entities.Common.InfoPageContent(
                Guid.NewGuid(),
                request.Slug,
                request.Title,
                request.ContentHtml,
                request.LastModifiedBy);

            _context.InfoPageContents.Add(entity);
        }
        else
        {
            entity.UpdateContent(request.Title, request.ContentHtml, request.LastModifiedBy);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<InfoPageContentDto>.Success(
            GetInfoPageContentBySlugQueryHandler.MapToDto(entity));
    }
}
