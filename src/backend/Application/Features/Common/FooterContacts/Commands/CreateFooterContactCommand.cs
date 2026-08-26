using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.FooterContacts.Mappings;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.FooterContacts.Commands;

public record CreateFooterContactCommand(
    string Title,
    string? Details,
    string? Email,
    string? Phone,
    string? Url,
    int SortOrder,
    string? LastModifiedBy
) : IRequest<Result<FooterContactDto>>;

public class CreateFooterContactCommandHandler
    : IRequestHandler<CreateFooterContactCommand, Result<FooterContactDto>>
{
    private readonly IFooterContactRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFooterContactCommandHandler(
        IFooterContactRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FooterContactDto>> Handle(
        CreateFooterContactCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            FooterContact entity = new(
                Guid.NewGuid(),
                request.Title,
                request.Details,
                request.Email,
                request.Phone,
                request.Url,
                request.SortOrder,
                request.LastModifiedBy);

            await _repository.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FooterContactDto>.Success(FooterContactMapper.ToDto(entity));
        }
        catch (ArgumentException ex)
        {
            return Result<FooterContactDto>.Failure(ex.Message);
        }
    }
}
