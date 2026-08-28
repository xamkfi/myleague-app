using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.FooterContacts.Mappings;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.FooterContacts.Queries;

public record GetFooterContactByIdQuery(Guid Id) : IRequest<Result<FooterContactDto>>;

public class GetFooterContactByIdQueryHandler
    : IRequestHandler<GetFooterContactByIdQuery, Result<FooterContactDto>>
{
    private readonly IFooterContactRepository _repository;

    public GetFooterContactByIdQueryHandler(IFooterContactRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<FooterContactDto>> Handle(
        GetFooterContactByIdQuery request,
        CancellationToken cancellationToken)
    {
        FooterContact? entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
        {
            return Result<FooterContactDto>.NotFound("FooterContact", request.Id);
        }

        return Result<FooterContactDto>.Success(FooterContactMapper.ToDto(entity));
    }
}
