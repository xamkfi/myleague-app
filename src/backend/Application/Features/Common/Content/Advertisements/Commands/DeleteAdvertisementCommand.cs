using Application.Common;
using Application.Interfaces.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Content.Advertisements.Commands;

public record DeleteAdvertisementCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteAdvertisementCommandHandler
    : IRequestHandler<DeleteAdvertisementCommand, Result<bool>>
{
    private readonly IAdvertisementRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<DeleteAdvertisementCommandHandler> _logger;

    public DeleteAdvertisementCommandHandler(
        IAdvertisementRepository repository,
        IUnitOfWork unitOfWork,
        IImageStorageService imageStorageService,
        ILogger<DeleteAdvertisementCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeleteAdvertisementCommand request,
        CancellationToken cancellationToken)
    {
        Advertisement? entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
        {
            return Result<bool>.NotFound("Advertisement", request.Id);
        }

        Uri image = entity.ImageUrl;

        await _repository.RemoveAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await AdvertisementImageCleanup.TryDeleteAsync(_imageStorageService, _logger, image, cancellationToken);

        return Result<bool>.Success(true);
    }
}
