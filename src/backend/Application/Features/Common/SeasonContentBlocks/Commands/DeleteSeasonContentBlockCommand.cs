using Application.Common;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.SeasonContentBlocks.Commands;

/// <summary>
/// Command for deleting a season content block
/// </summary>
public record DeleteSeasonContentBlockCommand(Guid Id) : IRequest<Result<bool>>;

/// <summary>
/// Handler for deleting a season content block
/// </summary>
public class DeleteSeasonContentBlockCommandHandler
    : IRequestHandler<DeleteSeasonContentBlockCommand, Result<bool>>
{
    private readonly ISeasonContentBlockRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the DeleteSeasonContentBlockCommandHandler class
    /// </summary>
    public DeleteSeasonContentBlockCommandHandler(
        ISeasonContentBlockRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result<bool>> Handle(
        DeleteSeasonContentBlockCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Common.SeasonContentBlock? entity =
            await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<bool>.NotFound("SeasonContentBlock", request.Id);
        }

        await _repository.RemoveAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
