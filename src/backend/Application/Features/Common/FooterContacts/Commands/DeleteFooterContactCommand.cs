using Application.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.FooterContacts.Commands;

public record DeleteFooterContactCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteFooterContactCommandHandler
    : IRequestHandler<DeleteFooterContactCommand, Result<bool>>
{
    private readonly IFooterContactRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFooterContactCommandHandler(
        IFooterContactRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(
        DeleteFooterContactCommand request,
        CancellationToken cancellationToken)
    {
        FooterContact? entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
        {
            return Result<bool>.NotFound("FooterContact", request.Id);
        }

        await _repository.RemoveAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
