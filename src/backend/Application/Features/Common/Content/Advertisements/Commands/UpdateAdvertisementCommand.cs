using Application.Common;
using Application.Features.Common.Content.Advertisements.DTOs;
using Application.Features.Common.Content.Advertisements.Mappings;
using Application.Interfaces.Common;
using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Content.Advertisements.Commands;

public record UpdateAdvertisementCommand(
    Guid Id,
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

public class UpdateAdvertisementCommandHandler
    : IRequestHandler<UpdateAdvertisementCommand, Result<AdvertisementDto>>
{
    private readonly IAdvertisementRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<UpdateAdvertisementCommandHandler> _logger;

    public UpdateAdvertisementCommandHandler(
        IAdvertisementRepository repository,
        IUnitOfWork unitOfWork,
        IImageStorageService imageStorageService,
        ILogger<UpdateAdvertisementCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    public async Task<Result<AdvertisementDto>> Handle(
        UpdateAdvertisementCommand request,
        CancellationToken cancellationToken)
    {
        Advertisement? entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
        {
            return Result<AdvertisementDto>.NotFound("Advertisement", request.Id);
        }

        Uri previousImage = entity.ImageUrl;

        try
        {
            AdvertisementMapper.UpdateFromCommand(entity, request);
        }
        catch (ArgumentException ex)
        {
            return Result<AdvertisementDto>.Failure(ex.Message);
        }

        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (previousImage != entity.ImageUrl)
        {
            await AdvertisementImageCleanup.TryDeleteAsync(
                _imageStorageService, _logger, previousImage, cancellationToken);
        }

        return Result<AdvertisementDto>.Success(AdvertisementMapper.ToDto(entity));
    }
}
