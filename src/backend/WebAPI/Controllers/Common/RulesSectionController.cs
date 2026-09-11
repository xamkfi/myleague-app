using Domain.Constants;
using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.RulesSection.Commands;
using Application.Features.Common.RulesSection.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common;

/// <summary>
/// Controller for managing rules sections and their individual rules
/// </summary>
[Route("api/[controller]")]
public class RulesSectionController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<RulesSectionController> _logger;

    /// <summary>
    /// Initializes a new instance of the RulesSectionController class
    /// </summary>
    /// <param name="mediator">The mediator for handling commands and queries</param>
    /// <param name="logger">The logger for this controller</param>
    public RulesSectionController(IMediator mediator, ILogger<RulesSectionController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all rules sections
    /// </summary>
    /// <returns>List of all rules sections</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RulesSectionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RulesSectionDto>>>> GetAllSections()
    {
        _logger.LogInformation("Getting all rules sections");

        Result<IReadOnlyList<RulesSectionDto>> result = await _mediator.Send(new GetAllRulesSectionsQuery());

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<IReadOnlyList<RulesSectionDto>>.SuccessResponse(
                result.Data,
                "Rules sections retrieved successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        return StatusCode(500, ApiResponse<IReadOnlyList<RulesSectionDto>>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Get a rules section by ID
    /// </summary>
    /// <param name="id">The rules section ID</param>
    /// <returns>Rules section details</returns>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RulesSectionDto>>> GetSectionById(Guid id)
    {
        _logger.LogInformation("Getting rules section {SectionId}", id);

        Result<RulesSectionDto> result = await _mediator.Send(new GetRulesSectionByIdQuery(id));

        return HandleResult(result, "Rules section retrieved successfully", "Failed to retrieve rules section");
    }

    /// <summary>
    /// Create a new rules section
    /// </summary>
    /// <param name="request">Rules section creation request</param>
    /// <returns>Created rules section details</returns>
    [HttpPost]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<RulesSectionDto>>> CreateSection(
        [FromBody] CreateRulesSectionRequest request)
    {
        _logger.LogInformation("Creating rules section {Title}", SanitizeForLog(request.Title));

        var command = new CreateRulesSectionCommand(
            request.Title,
            request.SortOrder,
            request.SectionType,
            request.ParentSectionId,
            User?.Identity?.Name);

        Result<RulesSectionDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<RulesSectionDto>.SuccessResponse(
                result.Data,
                "Rules section created successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        return BadRequest(ApiResponse<RulesSectionDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Update an existing rules section
    /// </summary>
    /// <param name="id">The rules section ID</param>
    /// <param name="request">Updated rules section data</param>
    /// <returns>Updated rules section details</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RulesSectionDto>>> UpdateSection(
        Guid id,
        [FromBody] UpdateRulesSectionRequest request)
    {
        _logger.LogInformation("Updating rules section {SectionId}", id);

        var command = new UpdateRulesSectionCommand(
            id,
            request.Title,
            request.SortOrder,
            request.SectionType,
            request.ParentSectionId,
            User?.Identity?.Name);

        Result<RulesSectionDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<RulesSectionDto>.SuccessResponse(
                result.Data,
                "Rules section updated successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();

        if (!string.IsNullOrWhiteSpace(errorMessage) && errorMessage.Contains("not found"))
        {
            return NotFound(ApiResponse<RulesSectionDto>.ErrorResponse(errorMessage));
        }

        return BadRequest(ApiResponse<RulesSectionDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Delete a rules section
    /// </summary>
    /// <param name="id">The rules section ID</param>
    /// <returns>True if the section was deleted</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSection(Guid id)
    {
        _logger.LogInformation("Deleting rules section {SectionId}", id);

        Result<bool> result = await _mediator.Send(new DeleteRulesSectionCommand(id));

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Rules section deleted successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();

        if (!string.IsNullOrWhiteSpace(errorMessage) && errorMessage.Contains("not found"))
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(errorMessage));
        }

        return BadRequest(ApiResponse<bool>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Add a rule to a rules section
    /// </summary>
    /// <param name="id">The rules section ID</param>
    /// <param name="request">Rule HTML content to add</param>
    /// <returns>Updated rules section details</returns>
    [HttpPost("{id:guid}/rules")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<RulesSectionDto>>> AddRule(
        Guid id,
        [FromBody] AddRulesSectionRuleRequest request)
    {
        _logger.LogInformation("Adding rule to section {SectionId}", id);

        var command = new AddRulesSectionRuleCommand(id, request.RuleHtml, User?.Identity?.Name);
        Result<RulesSectionDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<RulesSectionDto>.SuccessResponse(
                result.Data,
                "Rule added successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        return BadRequest(ApiResponse<RulesSectionDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Update a rule within a rules section
    /// </summary>
    /// <param name="id">The rules section ID</param>
    /// <param name="ruleId">The rule identifier</param>
    /// <param name="request">Updated rule HTML content</param>
    /// <returns>Updated rules section details</returns>
    [HttpPut("{id:guid}/rules/{ruleId}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RulesSectionDto>>> UpdateRule(
        Guid id,
        string ruleId,
        [FromBody] UpdateRulesSectionRuleRequest request)
    {
        _logger.LogInformation("Updating rule {RuleId} in section {SectionId}", SanitizeForLog(ruleId), id);

        var command = new UpdateRulesSectionRuleCommand(
            id,
            ruleId,
            request.RuleHtml,
            User?.Identity?.Name);

        Result<RulesSectionDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<RulesSectionDto>.SuccessResponse(
                result.Data,
                "Rule updated successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();

        if (!string.IsNullOrWhiteSpace(errorMessage) && errorMessage.Contains("not found"))
        {
            return NotFound(ApiResponse<RulesSectionDto>.ErrorResponse(errorMessage));
        }

        return BadRequest(ApiResponse<RulesSectionDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Delete a rule from a rules section
    /// </summary>
    /// <param name="id">The rules section ID</param>
    /// <param name="ruleId">The rule identifier</param>
    /// <returns>Updated rules section details</returns>
    [HttpDelete("{id:guid}/rules/{ruleId}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RulesSectionDto>>> DeleteRule(Guid id, string ruleId)
    {
        _logger.LogInformation("Deleting rule {RuleId} from section {SectionId}", SanitizeForLog(ruleId), id);

        var command = new DeleteRulesSectionRuleCommand(id, ruleId, User?.Identity?.Name);
        Result<RulesSectionDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<RulesSectionDto>.SuccessResponse(
                result.Data,
                "Rule deleted successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();

        if (!string.IsNullOrWhiteSpace(errorMessage) && errorMessage.Contains("not found"))
        {
            return NotFound(ApiResponse<RulesSectionDto>.ErrorResponse(errorMessage));
        }

        return BadRequest(ApiResponse<RulesSectionDto>.ErrorResponse(errorMessage));
    }
}
