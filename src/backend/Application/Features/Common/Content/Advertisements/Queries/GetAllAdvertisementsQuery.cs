using Application.Common;
using Application.Features.Common.Content.Advertisements.DTOs;
using Application.Features.Common.Content.Advertisements.Mappings;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.Content.Advertisements.Queries;

/// <summary>
/// Admin query: every advertisement, including inactive and expired ones.
/// </summary>
public record GetAllAdvertisementsQuery : IRequest<Result<IReadOnlyList<AdvertisementDto>>>;

public class GetAllAdvertisementsQueryHandler
    : IRequestHandler<GetAllAdvertisementsQuery, Result<IReadOnlyList<AdvertisementDto>>>
{
    private readonly IAdvertisementRepository _repository;

    public GetAllAdvertisementsQueryHandler(IAdvertisementRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<AdvertisementDto>>> Handle(
        GetAllAdvertisementsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Advertisement> advertisements = await _repository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<AdvertisementDto>>.Success(AdvertisementMapper.ToDtos(advertisements));
    }
}
