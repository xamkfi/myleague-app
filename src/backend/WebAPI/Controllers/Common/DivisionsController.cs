using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Commands.Common;
using Application.Queries.Common;
using Application.DTOs.Common;
using Application.Common;
using Domain.Enums.Common;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common;

/// <summary>
/// Controller for managing divisions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DivisionsController : ControllerBase
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
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<DivisionDto>>>> GetAllDivisions()
    {
        _logger.LogInformation("Getting all divisions");
        
        GetAllDivisionsQuery query = new GetAllDivisionsQuery();
        Result<IEnumerable<DivisionDto>> result = await _mediator.Send(query);

        if (result.IsSuccess && result.Data != null)
        {
            List<DivisionDto> divisionsList = result.Data.ToList();
            return Ok(ApiResponse<List<DivisionDto>>.SuccessResponse(divisionsList, "Divisions retrieved successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        return StatusCode(500, ApiResponse<List<DivisionDto>>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Get a division by ID
    /// </summary>
    /// <param name="id">Division ID</param>
    /// <returns>Division details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DivisionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DivisionDto>>> GetDivisionById(Guid id)
    {
        _logger.LogInformation("Getting division with ID: {DivisionId}", id);
        
        GetDivisionByIdQuery query = new GetDivisionByIdQuery(id);
        Result<DivisionDto> result = await _mediator.Send(query);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<DivisionDto>.SuccessResponse(result.Data, "Division retrieved successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        return NotFound(ApiResponse<DivisionDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Get divisions by sport type
    /// </summary>
    /// <param name="sportType">Sport type to filter by</param>
    /// <param name="activeOnly">Whether to return only active divisions (default: false)</param>
    /// <returns>List of divisions for the specified sport type</returns>
    [HttpGet("sport/{sportType}")]
    [ProducesResponseType(typeof(ApiResponse<List<DivisionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<DivisionDto>>>> GetDivisionsBySportType(
        string sportType, 
        [FromQuery] bool activeOnly = false)
    {
        if (!TryParseSportType(sportType, out SportsCategory parsedSportType, out string? parseError))
        {
            return BadRequest(ApiResponse<List<DivisionDto>>.ErrorResponse(parseError ?? "Invalid sport type."));
        }

        _logger.LogInformation("Getting divisions for sport type: {SportType}, ActiveOnly: {ActiveOnly}", parsedSportType, activeOnly);
        
        GetDivisionsBySportTypeQuery query = new GetDivisionsBySportTypeQuery(parsedSportType, activeOnly);
        Result<IEnumerable<DivisionDto>> result = await _mediator.Send(query);

        if (result.IsSuccess && result.Data != null)
        {
            List<DivisionDto> divisionsList = result.Data.ToList();
            return Ok(ApiResponse<List<DivisionDto>>.SuccessResponse(divisionsList, $"Divisions for {sportType} retrieved successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        
        if (errorMessage.Contains("required", StringComparison.OrdinalIgnoreCase) || 
            errorMessage.Contains("invalid", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(ApiResponse<List<DivisionDto>>.ErrorResponse(errorMessage));
        }

        return StatusCode(500, ApiResponse<List<DivisionDto>>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Create a new division
    /// </summary>
    /// <param name="request">Division creation request</param>
    /// <returns>Created division details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DivisionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DivisionDto>>> CreateDivision([FromBody] CreateDivisionRequest request)
    {
        if (request.SportType == SportsCategory.None)
        {
            const string message = "Sport type cannot be None.";
            List<string> errors = new() { message };
            return BadRequest(ApiResponse<DivisionDto>.ErrorResponse(message, errors));
        }

        _logger.LogInformation("Creating new division: {DivisionName} for {SportType}", request.Name, request.SportType);

        CreateDivisionCommand command = new CreateDivisionCommand(
            request.Name,
            request.Description,
            request.Level,
            request.SportType
        );

        Result<DivisionDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetDivisionById),
                new { id = result.Data.Id },
                ApiResponse<DivisionDto>.SuccessResponse(result.Data, "Division created successfully")
            );
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        List<string> errorList = result.ValidationFailures.Select(x => x.ErrorMessage).ToList();

        return BadRequest(ApiResponse<DivisionDto>.ErrorResponse(errorMessage, errorList));
    }

    /// <summary>
    /// Update an existing division
    /// </summary>
    /// <param name="id">Division ID</param>
    /// <param name="request">Division update request</param>
    /// <returns>Updated division details</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DivisionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DivisionDto>>> UpdateDivision(Guid id, [FromBody] UpdateDivisionRequest request)
    {
        _logger.LogInformation("Updating division with ID: {DivisionId}", id);

        UpdateDivisionCommand command = new UpdateDivisionCommand(
            id,
            request.Name,
            request.Description,
            request.Level
        );

        Result<DivisionDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<DivisionDto>.SuccessResponse(result.Data, "Division updated successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        
        // Check if it's a not found error
        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse<DivisionDto>.ErrorResponse(errorMessage));
        }

        return BadRequest(ApiResponse<DivisionDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Activate a division
    /// </summary>
    /// <param name="id">Division ID</param>
    /// <returns>Success confirmation</returns>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> ActivateDivision(Guid id)
    {
        _logger.LogInformation("Activating division with ID: {DivisionId}", id);

        ActivateDivisionCommand command = new ActivateDivisionCommand(id);
        Result<bool> result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse.SuccessResponse("Division activated successfully"));
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
    /// Deactivate a division
    /// </summary>
    /// <param name="id">Division ID</param>
    /// <returns>Success confirmation</returns>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> DeactivateDivision(Guid id)
    {
        _logger.LogInformation("Deactivating division with ID: {DivisionId}", id);

        DeactivateDivisionCommand command = new DeactivateDivisionCommand(id);
        Result<bool> result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse.SuccessResponse("Division deactivated successfully"));
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
    /// Delete a division
    /// </summary>
    /// <param name="id">Division ID</param>
    /// <returns>Success confirmation</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> DeleteDivision(Guid id)
    {
        _logger.LogInformation("Deleting division with ID: {DivisionId}", id);

        DeleteDivisionCommand command = new DeleteDivisionCommand(id);
        Result<bool> result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse.SuccessResponse("Division deleted successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();

        // Check if it's a not found error
        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse.ErrorResponse(errorMessage));
        }

        return StatusCode(500, ApiResponse.ErrorResponse(errorMessage));
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
