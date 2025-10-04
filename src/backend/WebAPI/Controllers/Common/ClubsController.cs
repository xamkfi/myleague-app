using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Commands.Clubs;
using Application.Queries.Clubs;
using Application.DTOs.Common;
using Application.Common;
using WebAPI.Models.Common;

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
    /// Get all clubs
    /// </summary>
    /// <returns>List of all clubs</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ClubDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<ClubDto>>>> GetAllClubs()
    {
        _logger.LogInformation("Getting all clubs");
        
        GetAllClubsQuery query = new GetAllClubsQuery();
        Result<IEnumerable<ClubDto>> result = await _mediator.Send(query);

        if (result.IsSuccess && result.Data != null)
        {
            List<ClubDto> clubsList = result.Data.ToList();
            return Ok(ApiResponse<List<ClubDto>>.SuccessResponse(clubsList, "Clubs retrieved successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        return StatusCode(500, ApiResponse<List<ClubDto>>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Get a club by ID
    /// </summary>
    /// <param name="id">Club ID</param>
    /// <returns>Club details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
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
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
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
        return BadRequest(ApiResponse<ClubDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Update an existing club
    /// </summary>
    /// <param name="id">Club ID</param>
    /// <param name="request">Club update request</param>
    /// <returns>Updated club details</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
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
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
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
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
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
} 
