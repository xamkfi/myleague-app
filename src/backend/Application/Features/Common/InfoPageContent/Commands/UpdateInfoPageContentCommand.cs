using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.InfoPageContent.Mappings;
using Domain.Repositories.Common;
using MediatR;
using InfoPageContentEntity = Domain.Entities.Common.InfoPageContent;

namespace Application.Features.Common.InfoPageContent.Commands;

/// <summary>
/// Command for updating info page content by slug
/// </summary>
public record UpdateInfoPageContentCommand(
    string Slug,
    string Title,
    string ContentHtml,
    string? LastModifiedBy
) : IRequest<Result<InfoPageContentDto>>;

/// <summary>
/// Handler for updating info page content
/// </summary>
public class UpdateInfoPageContentCommandHandler
    : IRequestHandler<UpdateInfoPageContentCommand, Result<InfoPageContentDto>>
{
    private readonly IInfoPageContentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the UpdateInfoPageContentCommandHandler class
    /// </summary>
    /// <param name="repository">The info page content repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    public UpdateInfoPageContentCommandHandler(
        IInfoPageContentRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the UpdateInfoPageContentCommand request
    /// </summary>
    /// <param name="request">The command containing updated page content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated info page content as a DTO wrapped in a Result</returns>
    public async Task<Result<InfoPageContentDto>> Handle(
        UpdateInfoPageContentCommand request,
        CancellationToken cancellationToken)
    {
        InfoPageContentEntity? entity = await _repository.GetBySlugAsync(request.Slug, cancellationToken);

        if (entity == null)
        {
            entity = new InfoPageContentEntity(
                Guid.NewGuid(),
                request.Slug,
                request.Title,
                request.ContentHtml,
                request.LastModifiedBy);

            await _repository.AddAsync(entity, cancellationToken);
        }
        else
        {
            entity.UpdateContent(request.Title, request.ContentHtml, request.LastModifiedBy);
            await _repository.UpdateAsync(entity, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InfoPageContentDto>.Success(InfoPageContentMapper.ToDto(entity));
    }
}
