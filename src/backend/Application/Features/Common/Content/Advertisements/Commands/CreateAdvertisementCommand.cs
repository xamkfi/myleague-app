using Application.Common;
using Application.Features.Common.Content.Advertisements.DTOs;
using Application.Features.Common.Content.Advertisements.Mappings;
using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.Content.Advertisements.Commands;

public record CreateAdvertisementCommand(
    string Name,
    string ImageUrl,
    string? LinkUrl,
    string? AltText,
    AdSlot Slot,
    bool IsActive,
    int SortOrder,
    DateTime? StartsAt,
    DateTime? EndsAt
) : IRequest<Result<AdvertisementDto>>;

public class CreateAdvertisementCommandHandler
    : IRequestHandler<CreateAdvertisementCommand, Result<AdvertisementDto>>
{
    private readonly IAdvertisementRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAdvertisementCommandHandler(
        IAdvertisementRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AdvertisementDto>> Handle(
        CreateAdvertisementCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            Advertisement entity = AdvertisementMapper.ToEntity(request);

            await _repository.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<AdvertisementDto>.Success(AdvertisementMapper.ToDto(entity));
        }
        catch (ArgumentException ex)
        {
            return Result<AdvertisementDto>.Failure(ex.Message);
        }
    }
}
