using Domain.Constants;
using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.SeasonContentBlocks.Commands;
using Application.Features.Common.SeasonContentBlocks.Queries;
using Domain.Enums.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common;

/// <summary>
/// Controller for managing season-specific content blocks on sport landing pages
/// </summary>
[Route("api/[controller]")]
public class SeasonContentBlockController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<SeasonContentBlockController> _logger;

    /// <summary>
    /// Initializes a new instance of the SeasonContentBlockController class
    /// </summary>
    public SeasonContentBlockController(
        IMediator mediator,
        ILogger<SeasonContentBlockController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get season content blocks by competition or by sport and season year
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SeasonContentBlockDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SeasonContentBlockDto>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SeasonContentBlockDto>>>> GetSeasonContentBlocks(
        [FromQuery] Guid? competitionId,
        [FromQuery] SportsCategory? sport,
        [FromQuery] string? seasonYear)
    {
        _logger.LogInformation(
            "Getting season content blocks for competition {CompetitionId}, sport {Sport}, year {SeasonYear}",
            competitionId,
            sport,
            SanitizeForLog(seasonYear));

        Result<IReadOnlyList<SeasonContentBlockDto>> result = await _mediator.Send(
            new GetAllSeasonContentBlocksQuery(competitionId, sport, seasonYear));

        return HandleResult(result, "Season content blocks retrieved successfully", "Failed to retrieve season content blocks");
    }

    /// <summary>
    /// Get a season content block by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SeasonContentBlockDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SeasonContentBlockDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SeasonContentBlockDto>>> GetSeasonContentBlockById(Guid id)
    {
        _logger.LogInformation("Getting season content block {BlockId}", id);

        Result<SeasonContentBlockDto> result = await _mediator.Send(new GetSeasonContentBlockByIdQuery(id));

        return HandleResult(result, "Season content block retrieved successfully", "Season content block not found");
    }

    /// <summary>
    /// Create a season content block
    /// </summary>
    [HttpPost]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<SeasonContentBlockDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<SeasonContentBlockDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SeasonContentBlockDto>>> CreateSeasonContentBlock(
        [FromBody] CreateSeasonContentBlockRequest request)
    {
        _logger.LogInformation(
            "Creating season content block {Title} for competition {CompetitionId}",
            SanitizeForLog(request.Title),
            request.CompetitionId);

        CreateSeasonContentBlockCommand command = new(
            request.Sport,
            request.CompetitionId,
            request.SeasonYear,
            request.Title,
            request.ContentHtml,
            request.SortOrder,
            User?.Identity?.Name);

        Result<SeasonContentBlockDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data is not null)
        {
            return CreatedAtAction(
                nameof(GetSeasonContentBlockById),
                new { id = result.Data.Id },
                ApiResponse<SeasonContentBlockDto>.SuccessResponse(result.Data, "Season content block created successfully"));
        }

        return ToErrorResponse(result, "Failed to create season content block");
    }

    /// <summary>
    /// Update a season content block
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<SeasonContentBlockDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SeasonContentBlockDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SeasonContentBlockDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SeasonContentBlockDto>>> UpdateSeasonContentBlock(
        Guid id,
        [FromBody] UpdateSeasonContentBlockRequest request)
    {
        _logger.LogInformation("Updating season content block {BlockId}", id);

        UpdateSeasonContentBlockCommand command = new(
            id,
            request.Title,
            request.ContentHtml,
            request.SortOrder,
            User?.Identity?.Name);

        Result<SeasonContentBlockDto> result = await _mediator.Send(command);

        return HandleResult(result, "Season content block updated successfully", "Failed to update season content block");
    }

    /// <summary>
    /// Reorder season content blocks
    /// </summary>
    [HttpPut("reorder")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SeasonContentBlockDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SeasonContentBlockDto>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SeasonContentBlockDto>>>> ReorderSeasonContentBlocks(
        [FromBody] ReorderSeasonContentBlocksRequest request)
    {
        _logger.LogInformation("Reordering {Count} season content blocks", request.OrderedIds.Count);

        Result<IReadOnlyList<SeasonContentBlockDto>> result = await _mediator.Send(
            new ReorderSeasonContentBlocksCommand(request.OrderedIds, User?.Identity?.Name));

        return HandleResult(result, "Season content blocks reordered successfully", "Failed to reorder season content blocks");
    }

    /// <summary>
    /// Delete a season content block
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSeasonContentBlock(Guid id)
    {
        _logger.LogInformation("Deleting season content block {BlockId}", id);

        Result<bool> result = await _mediator.Send(new DeleteSeasonContentBlockCommand(id));

        return HandleResult(result, "Season content block deleted successfully", "Failed to delete season content block");
    }
}
