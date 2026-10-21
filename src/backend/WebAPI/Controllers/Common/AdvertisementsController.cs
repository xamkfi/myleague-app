using Application.Common;
using Application.Features.Common.Content.Advertisements.Commands;
using Application.Features.Common.Content.Advertisements.DTOs;
using Application.Features.Common.Content.Advertisements.Queries;
using Application.Features.Common.Content.Images.Commands;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common;

/// <summary>
/// Controller for home page advertisements (ad slots).
/// </summary>
[Route("api/[controller]")]
public class AdvertisementsController : BaseApiController
{
    private static readonly string[] AllowedImageContentTypes =
        { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };

    private const long MaxImageFileSize = 10 * 1024 * 1024;

    private readonly IMediator _mediator;
    private readonly ILogger<AdvertisementsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdvertisementsController"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR mediator.</param>
    /// <param name="logger">The logger.</param>
    public AdvertisementsController(
        IMediator mediator,
        ILogger<AdvertisementsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get the advertisements that are active and scheduled to show right now.
    /// </summary>
    [HttpGet("visible")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PublicAdvertisementDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PublicAdvertisementDto>>>> GetVisible()
    {
        Result<IReadOnlyList<PublicAdvertisementDto>> result =
            await _mediator.Send(new GetVisibleAdvertisementsQuery());
        return HandleResult(result, "Advertisements retrieved successfully", "Failed to retrieve advertisements");
    }

    /// <summary>
    /// Get every advertisement, including inactive and expired ones.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdvertisementDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdvertisementDto>>>> GetAll()
    {
        Result<IReadOnlyList<AdvertisementDto>> result =
            await _mediator.Send(new GetAllAdvertisementsQuery());
        return HandleResult(result, "Advertisements retrieved successfully", "Failed to retrieve advertisements");
    }

    /// <summary>
    /// Create an advertisement.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<AdvertisementDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AdvertisementDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AdvertisementDto>>> Create(
        [FromBody] AdvertisementRequest request)
    {
        _logger.LogInformation("Creating advertisement {Name}", SanitizeForLog(request.Name));

        CreateAdvertisementCommand command = new(
            request.Name,
            request.ImageUrl,
            request.LinkUrl,
            request.AltText,
            request.Slot,
            request.IsActive,
            request.SortOrder,
            request.StartsAt,
            request.EndsAt);

        Result<AdvertisementDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<AdvertisementDto>.SuccessResponse(result.Data, "Advertisement created successfully"));
        }

        return ToErrorResponse(result, "Failed to create advertisement");
    }

    /// <summary>
    /// Update an advertisement. A replaced image is removed from storage.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<AdvertisementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AdvertisementDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<AdvertisementDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AdvertisementDto>>> Update(
        Guid id,
        [FromBody] AdvertisementRequest request)
    {
        _logger.LogInformation("Updating advertisement {AdvertisementId}", id);

        UpdateAdvertisementCommand command = new(
            id,
            request.Name,
            request.ImageUrl,
            request.LinkUrl,
            request.AltText,
            request.Slot,
            request.IsActive,
            request.SortOrder,
            request.StartsAt,
            request.EndsAt);

        Result<AdvertisementDto> result = await _mediator.Send(command);
        return HandleResult(result, "Advertisement updated successfully", "Failed to update advertisement");
    }

    /// <summary>
    /// Delete an advertisement and its image.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        _logger.LogInformation("Deleting advertisement {AdvertisementId}", id);
        Result<bool> result = await _mediator.Send(new DeleteAdvertisementCommand(id));
        return HandleResult(result, "Advertisement deleted successfully", "Failed to delete advertisement");
    }

    /// <summary>
    /// Upload an advertisement image and get its URL.
    /// </summary>
    /// <param name="file">The image file to upload.</param>
    [HttpPost("upload-image")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<string>>> UploadImage([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse("No file provided"));
        }

        if (!AllowedImageContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            _logger.LogWarning("Advertisement image upload failed: Invalid file type {ContentType}", SanitizeForLog(file.ContentType));
            return BadRequest(ApiResponse<string>.ErrorResponse(
                $"Invalid file type. Allowed types: {string.Join(", ", AllowedImageContentTypes)}"));
        }

        if (file.Length > MaxImageFileSize)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(
                $"File too large. Maximum size is {MaxImageFileSize / (1024 * 1024)}MB"));
        }

        await using Stream stream = file.OpenReadStream();
        Result<Uri> result = await _mediator.Send(new UploadImageCommand(stream, file.FileName, file.ContentType));

        if (result.IsSuccess && result.Data is not null)
        {
            return Ok(ApiResponse<string>.SuccessResponse(result.Data.ToString(), "Image uploaded successfully"));
        }

        if (result.ErrorKind == ResultErrorKind.Validation)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse("Invalid image file", result.GetAllErrors().ToList()));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        _logger.LogError("Advertisement image upload failed: {Error}", errorMessage);
        return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.ErrorResponse(errorMessage));
    }
}
