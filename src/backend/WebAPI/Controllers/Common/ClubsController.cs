using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common;
using Application.Features.Common.Clubs.Commands;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Clubs.Queries;
using Application.Features.Common.Images.Commands;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using Domain.Common;
using System.Linq;

namespace WebAPI.Controllers.Club;

/// <summary>
/// Controller for managing clubs
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ClubsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ClubsController> _logger;

    /// <summary>
    /// Initializes a new instance of the ClubsController class
    /// </summary>
    /// <param name="mediator">The mediator for handling commands and queries</param>
    /// <param name="logger">The logger for this controller</param>
    public ClubsController(IMediator mediator, ILogger<ClubsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get clubs with pagination
    /// </summary>
    /// <returns>Paginated list of clubs</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedApiResponse<ClubDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginatedApiResponse<ClubDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ClubDto>>> GetAllClubs([FromQuery] GetClubsRequest request)
    {
        _logger.LogInformation("Getting clubs page {Page} size {PageSize}", request.Page, request.PageSize);
        
        GetAllClubsQuery query = new GetAllClubsQuery(request.Page, request.PageSize);
        Result<PagedResult<ClubDto>> result = await _mediator.Send(query);

        if (result.IsSuccess && result.Data != null)
        {
            PagedResult<ClubDto> paged = result.Data;
            return Ok(PaginatedApiResponse<ClubDto>.SuccessResponse(paged, "Clubs retrieved successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        return StatusCode(500, PaginatedApiResponse<ClubDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Get a club by ID
    /// </summary>
    /// <param name="id">Club ID</param>
    /// <returns>Club details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ClubDto>>> GetClubById(Guid id)
    {
        _logger.LogInformation("Getting club with ID: {ClubId}", id);
        
        GetClubByIdQuery query = new GetClubByIdQuery(id);
        Result<ClubDto> result = await _mediator.Send(query);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<ClubDto>.SuccessResponse(result.Data, "Club retrieved successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        return NotFound(ApiResponse<ClubDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Create a new club
    /// </summary>
    /// <param name="request">Club creation request</param>
    /// <returns>Created club details</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ClubDto>>> CreateClub([FromBody] CreateClubRequest request)
    {
        _logger.LogInformation("Creating new club: {ClubName}", request.Name);

        CreateClubCommand command = new CreateClubCommand(
            request.Name,
            request.City,
            request.Country,
            request.FoundingDate,
            request.WebsiteUrl,
            request.LogoUrl,
            request.ContactEmail
        );

        Result<ClubDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetClubById),
                new { id = result.Data.Id },
                ApiResponse<ClubDto>.SuccessResponse(result.Data, "Club created successfully")
            );
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        List<string> errorList = result.ValidationFailures.Select(x => x.ErrorMessage).ToList();

        return BadRequest(ApiResponse<ClubDto>.ErrorResponse(errorMessage, errorList));
    }

    /// <summary>
    /// Upload a club logo image and get its URL
    /// </summary>
    /// <param name="file">The image file to upload</param>
    /// <returns>The URL of the uploaded image</returns>
    [HttpPost("upload-image")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<string>>> UploadImage([FromForm] IFormFile file)
    {
        _logger.LogInformation("Uploading club logo: {FileName}", file?.FileName);

        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Club logo upload failed: No file provided");
            return BadRequest(ApiResponse<string>.ErrorResponse("No file provided"));
        }

        string[] allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
        {
            _logger.LogWarning("Club logo upload failed: Invalid file type {ContentType}", file.ContentType);
            return BadRequest(ApiResponse<string>.ErrorResponse($"Invalid file type. Allowed types: {string.Join(", ", allowedContentTypes)}"));
        }

        const long maxFileSize = 10 * 1024 * 1024; // 10MB
        if (file.Length > maxFileSize)
        {
            _logger.LogWarning("Club logo upload failed: File too large {FileSize} bytes", file.Length);
            return BadRequest(ApiResponse<string>.ErrorResponse($"File too large. Maximum size is {maxFileSize / (1024 * 1024)}MB"));
        }

        try
        {
            using Stream stream = file.OpenReadStream();

            UploadImageCommand command = new UploadImageCommand(
                stream,
                file.FileName,
                file.ContentType);

            Result<Uri> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                _logger.LogInformation("Club logo uploaded successfully: {ImageUrl}", result.Data);
                return Ok(ApiResponse<string>.SuccessResponse(result.Data.ToString(), "Image uploaded successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            _logger.LogError("Club logo upload failed: {Error}", errorMessage);
            return StatusCode(500, ApiResponse<string>.ErrorResponse(errorMessage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during club logo upload");
            return StatusCode(500, ApiResponse<string>.ErrorResponse("An unexpected error occurred during image upload"));
        }
    }

    /// <summary>
    /// Update an existing club
    /// </summary>
    /// <param name="id">Club ID</param>
    /// <param name="request">Club update request</param>
    /// <returns>Updated club details</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ClubDto>>> UpdateClub(Guid id, [FromBody] UpdateClubRequest request)
    {
        _logger.LogInformation("Updating club with ID: {ClubId}", id);

        UpdateClubCommand command = new UpdateClubCommand(
            id,
            request.Name,
            request.City,
            request.Country,
            request.FoundingDate,
            request.WebsiteUrl,
            request.LogoUrl,
            request.ContactEmail
        );

        Result<ClubDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<ClubDto>.SuccessResponse(result.Data, "Club updated successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        
        // Check if it's a not found error
        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse<ClubDto>.ErrorResponse(errorMessage));
        }

        return BadRequest(ApiResponse<ClubDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Delete a club
    /// </summary>
    /// <param name="id">Club ID</param>
    /// <returns>Success confirmation</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> DeleteClub(Guid id)
    {
        _logger.LogInformation("Deleting club with ID: {ClubId}", id);

        DeleteClubCommand command = new DeleteClubCommand(id);
        Result result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse.SuccessResponse("Club deleted successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();

        // Check if it's a not found error
        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse.ErrorResponse(errorMessage));
        }

        return StatusCode(500, ApiResponse.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Update the logo of a club
    /// </summary>
    /// <param name="id">Club ID</param>
    /// <param name="logoUrl">New logo URL</param>
    /// <returns>Updated club details</returns>
    [HttpPatch("{id:guid}/logo")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ClubDto>>> UpdateClubLogo(Guid id, [FromBody] string? logoUrl)
    {
        _logger.LogInformation("Updating logo for club with ID: {ClubId}", id);

        UpdateClubLogoCommand command = new UpdateClubLogoCommand(id, logoUrl);
        Result<ClubDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<ClubDto>.SuccessResponse(result.Data, "Club logo updated successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        
        // Check if it's a not found error
        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse<ClubDto>.ErrorResponse(errorMessage));
        }

        return BadRequest(ApiResponse<ClubDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Search clubs by name
    /// </summary>
    /// <param name="name">The name to search for</param>
    /// <returns>List of clubs matching the search term</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<List<ClubDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<ClubDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<List<ClubDto>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<List<ClubDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<ClubDto>>>> GetClubsByName([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(ApiResponse<List<ClubDto>>.ErrorResponse("Name parameter is required"));
        }

        _logger.LogInformation("Searching clubs by name: {Name}", name);

        GetClubsByNameQuery query = new GetClubsByNameQuery(name);
        Result<IEnumerable<ClubDto>> result = await _mediator.Send(query);

        if (result.IsSuccess && result.Data != null)
        {
            List<ClubDto> clubList = result.Data.ToList();
            return Ok(ApiResponse<List<ClubDto>>.SuccessResponse(clubList, "Clubs found successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        
        // Check if it's a not found error
        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse<List<ClubDto>>.ErrorResponse(errorMessage));
        }

        return BadRequest(ApiResponse<List<ClubDto>>.ErrorResponse(errorMessage));
    }
} 
