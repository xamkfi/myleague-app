using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common;
using Application.Features.Common.Divisions.Commands;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.Divisions.Queries;
using Domain.Enums.Common;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common;

/// <summary>
/// Controller for managing divisions
/// </summary>
[Route("api/[controller]")]
public class DivisionsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<DivisionsController> _logger;

    /// <summary>
    /// Initializes a new instance of the DivisionsController class
    /// </summary>
    /// <param name="mediator">The mediator for handling commands and queries</param>
    /// <param name="logger">The logger for this controller</param>
    public DivisionsController(IMediator mediator, ILogger<DivisionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all divisions
    /// </summary>
    /// <returns>List of all divisions</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<DivisionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<DivisionDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<DivisionDto>>>> GetAllDivisions()
    {
        _logger.LogInformation("Getting all divisions");

        Result<IEnumerable<DivisionDto>> result = await _mediator.Send(new GetAllDivisionsQuery());

        return HandleListResult(result, "Divisions retrieved successfully", "Failed to retrieve divisions");
    }

    /// <summary>
    /// Get a division by ID
    /// </summary>
    /// <param name="id">Division ID</param>
    /// <returns>Division details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DivisionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DivisionDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<DivisionDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DivisionDto>>> GetDivisionById(Guid id)
    {
        _logger.LogInformation("Getting division with ID: {DivisionId}", id);

        Result<DivisionDto> result = await _mediator.Send(new GetDivisionByIdQuery(id));

        return HandleResult(result, "Division retrieved successfully", "Division not found");
    }

    /// <summary>
    /// Get divisions by sport type
    /// </summary>
    /// <param name="sportType">Sport type to filter by</param>
    /// <param name="activeOnly">Whether to return only active divisions (default: false)</param>
    /// <returns>List of divisions for the specified sport type</returns>
    [HttpGet("sport/{sportType}")]
    [ProducesResponseType(typeof(ApiResponse<List<DivisionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<DivisionDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<List<DivisionDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<DivisionDto>>>> GetDivisionsBySportType(
        string sportType,
        [FromQuery] bool activeOnly = false)
    {
        if (!TryParseSportType(sportType, out SportsCategory parsedSportType, out string? parseError))
        {
            return BadRequest(ApiResponse<List<DivisionDto>>.ErrorResponse(parseError ?? "Invalid sport type."));
        }

        _logger.LogInformation("Getting divisions for sport type: {SportType}, ActiveOnly: {ActiveOnly}", parsedSportType, activeOnly);

        Result<IEnumerable<DivisionDto>> result = await _mediator.Send(
            new GetDivisionsBySportTypeQuery(parsedSportType, activeOnly));

        return HandleListResult(result, $"Divisions for {sportType} retrieved successfully", "Failed to retrieve divisions");
    }

    /// <summary>
    /// Create a new division
    /// </summary>
    /// <param name="request">Division creation request</param>
    /// <returns>Created division details</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<DivisionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<DivisionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<DivisionDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DivisionDto>>> CreateDivision([FromBody] CreateDivisionRequest request)
    {
        if (request.SportType == SportsCategory.None)
        {
            const string message = "Sport type cannot be None.";
            return BadRequest(ApiResponse<DivisionDto>.ErrorResponse(message, new List<string> { message }));
        }

        _logger.LogInformation(
            "Creating new division: {DivisionName} for {SportType}",
            SanitizeForLog(request.Name),
            SanitizeForLog(request.SportType));

        Result<DivisionDto> result = await _mediator.Send(new CreateDivisionCommand(
            request.Name,
            request.Description,
            request.Level,
            request.SportType));

        if (result.IsSuccess && result.Data is not null)
        {
            return CreatedAtAction(
                nameof(GetDivisionById),
                new { id = result.Data.Id },
                ApiResponse<DivisionDto>.SuccessResponse(result.Data, "Division created successfully"));
        }

        return ToErrorResponse(result, "Failed to create division");
    }

    /// <summary>
    /// Update an existing division
    /// </summary>
    /// <param name="id">Division ID</param>
    /// <param name="request">Division update request</param>
    /// <returns>Updated division details</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<DivisionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DivisionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<DivisionDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<DivisionDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DivisionDto>>> UpdateDivision(Guid id, [FromBody] UpdateDivisionRequest request)
    {
        _logger.LogInformation("Updating division with ID: {DivisionId}", id);

        Result<DivisionDto> result = await _mediator.Send(new UpdateDivisionCommand(
            id,
            request.Name,
            request.Description,
            request.Level));

        return HandleResult(result, "Division updated successfully", "Failed to update division");
    }

    /// <summary>
    /// Activate a division
    /// </summary>
    /// <param name="id">Division ID</param>
    /// <returns>Success confirmation</returns>
    [HttpPatch("{id:guid}/activate")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> ActivateDivision(Guid id)
    {
        _logger.LogInformation("Activating division with ID: {DivisionId}", id);

        Result<bool> result = await _mediator.Send(new ActivateDivisionCommand(id));

        return HandleVoidResult(result, "Division activated successfully", "Failed to activate division");
    }

    /// <summary>
    /// Deactivate a division
    /// </summary>
    /// <param name="id">Division ID</param>
    /// <returns>Success confirmation</returns>
    [HttpPatch("{id:guid}/deactivate")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> DeactivateDivision(Guid id)
    {
        _logger.LogInformation("Deactivating division with ID: {DivisionId}", id);

        Result<bool> result = await _mediator.Send(new DeactivateDivisionCommand(id));

        return HandleVoidResult(result, "Division deactivated successfully", "Failed to deactivate division");
    }

    /// <summary>
    /// Delete a division
    /// </summary>
    /// <param name="id">Division ID</param>
    /// <returns>Success confirmation</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> DeleteDivision(Guid id)
    {
        _logger.LogInformation("Deleting division with ID: {DivisionId}", id);

        Result<bool> result = await _mediator.Send(new DeleteDivisionCommand(id));

        return HandleVoidResult(result, "Division deleted successfully", "Failed to delete division");
    }

    private static bool TryParseSportType(string? value, out SportsCategory sportType, out string? errorMessage)
    {
        sportType = SportsCategory.None;

        if (string.IsNullOrWhiteSpace(value))
        {
            errorMessage = "Sport type is required.";
            return false;
        }

        if (!Enum.TryParse(value, true, out sportType) || sportType == SportsCategory.None)
        {
            errorMessage = $"Invalid sport type '{value}'.";
            sportType = SportsCategory.None;
            return false;
        }

        errorMessage = null;
        return true;
    }
}
