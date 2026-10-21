using Application.Common;
using Application.Features.Common.Content.Advertisements.DTOs;
using Application.Features.Common.Content.Advertisements.Mappings;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.Content.Advertisements.Queries;

/// <summary>
/// Public query: active advertisements whose schedule includes the current time.
/// </summary>
public record GetVisibleAdvertisementsQuery : IRequest<Result<IReadOnlyList<PublicAdvertisementDto>>>;

public class GetVisibleAdvertisementsQueryHandler
    : IRequestHandler<GetVisibleAdvertisementsQuery, Result<IReadOnlyList<PublicAdvertisementDto>>>
{
    private readonly IAdvertisementRepository _repository;
    private readonly TimeProvider _timeProvider;

    public GetVisibleAdvertisementsQueryHandler(IAdvertisementRepository repository, TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<Result<IReadOnlyList<PublicAdvertisementDto>>> Handle(
        GetVisibleAdvertisementsQuery request,
        CancellationToken cancellationToken)
    {
        DateTime utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        IReadOnlyList<Advertisement> advertisements = await _repository.GetVisibleAsync(utcNow, cancellationToken);
        return Result<IReadOnlyList<PublicAdvertisementDto>>.Success(AdvertisementMapper.ToPublicDtos(advertisements));
    }
}
