using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Commands.Common;
using Application.Common;
using Application.Interfaces.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Common
{
    public class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, Result<Uri>>
    {
        private readonly IImageStorageService _imageStorageService;
        private readonly ILogger<UploadImageCommandHandler> _logger;

        public UploadImageCommandHandler(
            IImageStorageService imageStorageService,
            ILogger<UploadImageCommandHandler> logger)
        {
            _imageStorageService = imageStorageService;
            _logger = logger;
        }

        public async Task<Result<Uri>> Handle(UploadImageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing image upload: {FileName}", request.FileName);

                // Generate unique file name for image
                string fileExtension = Path.GetExtension(request.FileName);
                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                Uri imageUrl = await _imageStorageService.SaveImage(
                    request.imageStream,
                    uniqueFileName,
                    cancellationToken);

                _logger.LogInformation("Image upload completed: {ImageUrl}", imageUrl);

                return Result<Uri>.Success(imageUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image: {FileName}", request.FileName);
                return Result<Uri>.Failure("Failed to upload image to storage");
            }
        }
    }
}
