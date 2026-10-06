using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.Organization.Divisions.Commands;
using Application.Features.Common.CrossCutting.MatchTimer.Commands;
using Application.Features.Common.Content.Images.Commands;
using Application.Common;
using Application.Interfaces.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Content.Images.Handlers
{
    public class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, Result<Uri>>
    {
        private static readonly IReadOnlyDictionary<string, string[]> AllowedContentTypesByExtension =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [".jpg"] = new[] { "image/jpeg", "image/jpg" },
                [".jpeg"] = new[] { "image/jpeg", "image/jpg" },
                [".png"] = new[] { "image/png" },
                [".gif"] = new[] { "image/gif" },
                [".webp"] = new[] { "image/webp" },
            };

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
            string fileExtension = Path.GetExtension(request.FileName ?? string.Empty).ToLowerInvariant();
            if (!AllowedContentTypesByExtension.TryGetValue(fileExtension, out string[]? allowedContentTypes))
            {
                return Result<Uri>.ValidationFailure(new[]
                {
                    "Invalid file extension. Allowed extensions: " + string.Join(", ", AllowedContentTypesByExtension.Keys)
                });
            }

            if (request.ContentType is null
                || !allowedContentTypes.Contains(request.ContentType.ToLowerInvariant()))
            {
                return Result<Uri>.ValidationFailure(new[]
                {
                    "The file content type does not match its extension."
                });
            }

            try
            {
                _logger.LogInformation("Processing image upload: {FileName}", request.FileName);

                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                Uri imageUrl = await _imageStorageService.SaveImage(
                    request.imageStream,
                    uniqueFileName,
                    cancellationToken);

                _logger.LogInformation("Image upload completed: {ImageUrl}", imageUrl);

                return Result<Uri>.Success(imageUrl);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image: {FileName}", request.FileName);
                return Result<Uri>.Failure("Failed to upload image to storage");
            }
        }
    }
}
