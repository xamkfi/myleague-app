using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.RulesSection.Commands;
using Application.Features.Common.RulesSection.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RulesSectionController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RulesSectionController> _logger;

    public RulesSectionController(IMediator mediator, ILogger<RulesSectionController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

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

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RulesSectionDto>>> GetSectionById(Guid id)
    {
        _logger.LogInformation("Getting rules section {SectionId}", id);

        Result<RulesSectionDto> result = await _mediator.Send(new GetRulesSectionByIdQuery(id));

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<RulesSectionDto>.SuccessResponse(
                result.Data,
                "Rules section retrieved successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();

        if (!string.IsNullOrWhiteSpace(errorMessage) && errorMessage.Contains("not found"))
        {
            return NotFound(ApiResponse<RulesSectionDto>.ErrorResponse(errorMessage));
        }

        return StatusCode(500, ApiResponse<RulesSectionDto>.ErrorResponse(errorMessage));
    }

    [HttpPost]
    [Authorize(Roles = "SystemAdmin,ClubAdmin")]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<RulesSectionDto>>> CreateSection(
        [FromBody] CreateRulesSectionRequest request)
    {
        _logger.LogInformation("Creating rules section {Title}", request.Title);

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

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SystemAdmin,ClubAdmin")]
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

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SystemAdmin,ClubAdmin")]
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

    [HttpPost("{id:guid}/rules")]
    [Authorize(Roles = "SystemAdmin,ClubAdmin")]
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

    [HttpPut("{id:guid}/rules/{ruleId}")]
    [Authorize(Roles = "SystemAdmin,ClubAdmin")]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RulesSectionDto>>> UpdateRule(
        Guid id,
        string ruleId,
        [FromBody] UpdateRulesSectionRuleRequest request)
    {
        _logger.LogInformation("Updating rule {RuleId} in section {SectionId}", ruleId, id);

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

    [HttpDelete("{id:guid}/rules/{ruleId}")]
    [Authorize(Roles = "SystemAdmin,ClubAdmin")]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RulesSectionDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RulesSectionDto>>> DeleteRule(Guid id, string ruleId)
    {
        _logger.LogInformation("Deleting rule {RuleId} from section {SectionId}", ruleId, id);

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
