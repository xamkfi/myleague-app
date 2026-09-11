using Domain.Constants;
using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.InfoPageContent.Commands;
using Application.Features.Common.InfoPageContent.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common;

/// <summary>
/// Controller for managing MAHL info page content
/// </summary>
[Route("api/[controller]")]
public class InfoPageContentController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<InfoPageContentController> _logger;

    /// <summary>
    /// Initializes a new instance of the InfoPageContentController class
    /// </summary>
    public InfoPageContentController(
        IMediator mediator,
        ILogger<InfoPageContentController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all info page contents
    /// </summary>
    [HttpGet]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<InfoPageContentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InfoPageContentDto>>>> GetAllInfoPageContents()
    {
        _logger.LogInformation("Getting all info page contents");

        Result<IReadOnlyList<InfoPageContentDto>> result = await _mediator.Send(
            new GetAllInfoPageContentsQuery());

        return HandleResult(result, "Info page contents retrieved successfully", "Failed to retrieve info page contents");
    }

    /// <summary>
    /// Get info page content by slug
    /// </summary>
    [HttpGet("{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<InfoPageContentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InfoPageContentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InfoPageContentDto>>> GetInfoPageContentBySlug(string slug)
    {
        _logger.LogInformation("Getting info page content by slug: {Slug}", SanitizeForLog(slug));

        Result<InfoPageContentDto> result = await _mediator.Send(new GetInfoPageContentBySlugQuery(slug));

        return HandleResult(result, "Info page content retrieved successfully", "Failed to retrieve info page content");
    }

    /// <summary>
    /// Update info page content by slug (upsert)
    /// </summary>
    [HttpPut("{slug}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<InfoPageContentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InfoPageContentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<InfoPageContentDto>>> UpdateInfoPageContent(
        string slug,
        [FromBody] UpdateInfoPageContentRequest request)
    {
        _logger.LogInformation("Updating info page content with slug: {Slug}", SanitizeForLog(slug));

        UpdateInfoPageContentCommand command = new(
            slug,
            request.Title,
            request.ContentHtml,
            User?.Identity?.Name);

        Result<InfoPageContentDto> result = await _mediator.Send(command);

        return HandleResult(result, "Info page content updated successfully", "Failed to update info page content");
    }
}
