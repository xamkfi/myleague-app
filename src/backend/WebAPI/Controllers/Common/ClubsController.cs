using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common;
using Application.Features.Common.Clubs.Commands;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Clubs.Queries;
using Application.Features.Common.Images.Commands;
using Application.Services.Common;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using Domain.Common;

namespace WebAPI.Controllers.Club;

/// <summary>
/// Controller for managing clubs
/// </summary>
[Route("api/[controller]")]
public class ClubsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IClubAdminAccessService _clubAdminAccessService;
    private readonly ILogger<ClubsController> _logger;

    /// <summary>
    /// Initializes a new instance of the ClubsController class
    /// </summary>
    /// <param name="mediator">The mediator for handling commands and queries</param>
    /// <param name="clubAdminAccessService">Service for checking club admin access</param>
    /// <param name="logger">The logger for this controller</param>
    public ClubsController(
        IMediator mediator,
        IClubAdminAccessService clubAdminAccessService,
        ILogger<ClubsController> logger)
    {
        _mediator = mediator;
        _clubAdminAccessService = clubAdminAccessService;
        _logger = logger;
    }

    /// <summary>
    /// Get clubs with pagination
    /// </summary>
    /// <returns>Paginated list of clubs</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedApiResponse<ClubDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginatedApiResponse<ClubDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedApiResponse<ClubDto>>> GetAllClubs([FromQuery] GetClubsRequest request)
    {
        _logger.LogInformation("Getting clubs page {Page} size {PageSize}", request.Page, request.PageSize);
        
        GetAllClubsQuery query = new GetAllClubsQuery(request.Page, request.PageSize);
        Result<PagedResult<ClubDto>> result = await _mediator.Send(query);

        return HandlePaginatedResult(result, "Clubs retrieved successfully", "Failed to retrieve clubs");
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

        return HandleResult(result, "Club retrieved successfully", "Club not found");
    }

    /// <summary>
    /// Create a new club
    /// </summary>
    /// <param name="request">Club creation request</param>
    /// <returns>Created club details</returns>
    [HttpPost]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ClubDto>>> CreateClub([FromBody] CreateClubRequest request)
    {
        _logger.LogInformation("Creating new club: {ClubName}", SanitizeForLog(request.Name));

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

        if (result.IsSuccess && result.Data is not null)
        {
            return CreatedAtAction(
                nameof(GetClubById),
                new { id = result.Data.Id },
                ApiResponse<ClubDto>.SuccessResponse(result.Data, "Club created successfully")
            );
        }

        return ToErrorResponse(result, "Failed to create club");
    }

    /// <summary>
    /// Upload a club logo image and get its URL
    /// </summary>
    /// <param name="file">The image file to upload</param>
    /// <returns>The URL of the uploaded image</returns>
    [HttpPost("upload-image")]
    [Authorize(Roles = AuthRoles.ClubAdminOrAdmin)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<string>>> UploadImage([FromForm] IFormFile file)
    {
        _logger.LogInformation("Uploading club logo: {FileName}", SanitizeForLog(file?.FileName));

        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Club logo upload failed: No file provided");
            return BadRequest(ApiResponse<string>.ErrorResponse("No file provided"));
        }

        string[] allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
        {
            _logger.LogWarning("Club logo upload failed: Invalid file type {ContentType}", SanitizeForLog(file.ContentType));
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

            if (result.IsSuccess && result.Data is not null)
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
    /// Update an existing club. Site admins can update any club; club admins can only
    /// update clubs they actively manage.
    /// </summary>
    /// <param name="id">Club ID</param>
    /// <param name="request">Club update request</param>
    /// <returns>Updated club details</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = AuthRoles.ClubAdminOrAdmin)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ClubDto>>> UpdateClub(Guid id, [FromBody] UpdateClubRequest request)
    {
        ActionResult? accessError = await CheckClubAccessAsync(id);
        if (accessError != null)
        {
            return accessError;
        }

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

        return HandleResult(result, "Club updated successfully", "Failed to update club");
    }

    /// <summary>
    /// Delete a club. Only site administrators may delete clubs; club admins cannot.
    /// </summary>
    /// <param name="id">Club ID</param>
    /// <returns>Success confirmation</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> DeleteClub(Guid id)
    {
        _logger.LogInformation("Deleting club with ID: {ClubId}", id);

        DeleteClubCommand command = new DeleteClubCommand(id);
        Result result = await _mediator.Send(command);

        return HandleVoidResult(result, "Club deleted successfully", "Failed to delete club");
    }

    /// <summary>
    /// Update the logo of a club. Site admins can update any club; club admins can only
    /// update clubs they actively manage.
    /// </summary>
    /// <param name="id">Club ID</param>
    /// <param name="logoUrl">New logo URL</param>
    /// <returns>Updated club details</returns>
    [HttpPatch("{id:guid}/logo")]
    [Authorize(Roles = AuthRoles.ClubAdminOrAdmin)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ClubDto>>> UpdateClubLogo(Guid id, [FromBody] string? logoUrl)
    {
        ActionResult? accessError = await CheckClubAccessAsync(id);
        if (accessError != null)
        {
            return accessError;
        }

        _logger.LogInformation("Updating logo for club with ID: {ClubId}", id);

        UpdateClubLogoCommand command = new UpdateClubLogoCommand(id, logoUrl);
        Result<ClubDto> result = await _mediator.Send(command);

        return HandleResult(result, "Club logo updated successfully", "Failed to update club logo");
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

        _logger.LogInformation("Searching clubs by name: {Name}", SanitizeForLog(name));

        GetClubsByNameQuery query = new GetClubsByNameQuery(name);
        Result<IEnumerable<ClubDto>> result = await _mediator.Send(query);

        return HandleListResult(result, "Clubs found successfully", "Failed to search clubs");
    }

    /// <summary>
    /// Get the active club admins of a club
    /// </summary>
    /// <param name="id">Club ID</param>
    /// <returns>List of users administering the club</returns>
    [HttpGet("{id:guid}/admins")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<List<ClubAdminUserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<ClubAdminUserDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<ClubAdminUserDto>>>> GetClubAdmins(Guid id)
    {
        _logger.LogInformation("Getting club admins for club: {ClubId}", id);

        Result<IEnumerable<ClubAdminUserDto>> result = await _mediator.Send(new GetClubAdminsQuery(id));

        return HandleListResult(result, "Club admins retrieved successfully", "Failed to retrieve club admins");
    }

    /// <summary>
    /// Replace the set of club admins of a club. Users in the list get an active club manager
    /// link; existing links for users not in the list are deactivated.
    /// </summary>
    /// <param name="id">Club ID</param>
    /// <param name="request">The user IDs that should administer the club</param>
    /// <returns>Success confirmation</returns>
    [HttpPut("{id:guid}/admins")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> SetClubAdmins(Guid id, [FromBody] SetClubAdminsRequest request)
    {
        _logger.LogInformation("Setting club admins for club: {ClubId} ({AdminCount} admins)", id, request.UserIds?.Count ?? 0);

        SetClubAdminsCommand command = new SetClubAdminsCommand(id, request.UserIds ?? new List<Guid>());
        Result<bool> result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse.SuccessResponse("Club admins updated successfully"));
        }

        return BadRequest(ApiResponse.ErrorResponse(result.Error ?? "Failed to update club admins"));
    }

    private bool IsSystemAdmin => User.IsInRole(AuthRoles.SystemAdmin);

    private bool TryGetPersonId(out Guid personId)
    {
        string? personIdClaim = User.FindFirst("personId")?.Value;
        return Guid.TryParse(personIdClaim, out personId);
    }

    /// <summary>
    /// Returns null when the caller may manage the club, otherwise the error result.
    /// Site admins always pass; club admins must have an active club manager link.
    /// </summary>
    private async Task<ActionResult?> CheckClubAccessAsync(Guid clubId)
    {
        if (IsSystemAdmin)
        {
            return null;
        }

        if (!TryGetPersonId(out Guid personId))
        {
            return Unauthorized(ApiResponse.ErrorResponse("Invalid token."));
        }

        if (!await _clubAdminAccessService.CanManageClubAsync(personId, clubId))
        {
            _logger.LogWarning("Person {PersonId} attempted to manage club {ClubId} without access", personId, clubId);
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.ErrorResponse("You are not an admin of this club."));
        }

        return null;
    }
}
