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
    public class DeleteImageCommandHandler : IRequestHandler<DeleteImageCommand, Result<bool>>
    {
        private readonly IImageStorageService _imageStorageService;
        private readonly ILogger<DeleteImageCommandHandler> _logger;

        public DeleteImageCommandHandler(
            IImageStorageService imageStorageService,
            ILogger<DeleteImageCommandHandler> logger)
        {
            _imageStorageService = imageStorageService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeleteImageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing image deletion from: {FileUrl}", request.url);

                bool delete = await _imageStorageService.DeleteImage(request.url);

                _logger.LogInformation("Image delete completed: {ImageUrl}", delete);

                return Result<bool>.Success(delete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image: {FileName}", request.url);
                return Result<bool>.Failure("Failed to delete image");
            }
        }
    }
}
