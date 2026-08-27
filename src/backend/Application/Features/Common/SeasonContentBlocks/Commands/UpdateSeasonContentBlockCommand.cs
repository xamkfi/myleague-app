using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.SeasonContentBlocks.Mappings;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.SeasonContentBlocks.Commands;

/// <summary>
/// Command for updating a season content block
/// </summary>
public record UpdateSeasonContentBlockCommand(
    Guid Id,
    string Title,
    string ContentHtml,
    int SortOrder,
    string? LastModifiedBy
) : IRequest<Result<SeasonContentBlockDto>>;

/// <summary>
/// Handler for updating a season content block
/// </summary>
public class UpdateSeasonContentBlockCommandHandler
    : IRequestHandler<UpdateSeasonContentBlockCommand, Result<SeasonContentBlockDto>>
{
    private readonly ISeasonContentBlockRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the UpdateSeasonContentBlockCommandHandler class
    /// </summary>
    public UpdateSeasonContentBlockCommandHandler(
        ISeasonContentBlockRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result<SeasonContentBlockDto>> Handle(
        UpdateSeasonContentBlockCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Common.SeasonContentBlock? entity =
            await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<SeasonContentBlockDto>.NotFound("SeasonContentBlock", request.Id);
        }

        entity.UpdateContent(
            request.Title,
            request.ContentHtml,
            request.SortOrder,
            request.LastModifiedBy);

        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SeasonContentBlockDto>.Success(SeasonContentBlockMapper.ToDto(entity));
    }
}
