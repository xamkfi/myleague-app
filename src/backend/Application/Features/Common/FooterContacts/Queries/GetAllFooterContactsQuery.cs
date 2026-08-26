using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.FooterContacts.Mappings;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.FooterContacts.Queries;

public record GetAllFooterContactsQuery : IRequest<Result<IReadOnlyList<FooterContactDto>>>;

public class GetAllFooterContactsQueryHandler
    : IRequestHandler<GetAllFooterContactsQuery, Result<IReadOnlyList<FooterContactDto>>>
{
    private readonly IFooterContactRepository _repository;

    public GetAllFooterContactsQueryHandler(IFooterContactRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<FooterContactDto>>> Handle(
        GetAllFooterContactsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<FooterContact> contacts = await _repository.GetAllAsync(cancellationToken);
        IReadOnlyList<FooterContactDto> dtos = contacts.Select(FooterContactMapper.ToDto).ToList();
        return Result<IReadOnlyList<FooterContactDto>>.Success(dtos);
    }
}
