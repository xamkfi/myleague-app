// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.InfoPageContent.Commands;
using Application.Features.Common.InfoPageContent.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InfoPageContentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<InfoPageContentController> _logger;

    public InfoPageContentController(
        IMediator mediator,
        ILogger<InfoPageContentController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin,ClubAdmin")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<InfoPageContentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InfoPageContentDto>>>> GetAllInfoPageContents()
    {
        _logger.LogInformation("Getting all info page contents");

        Result<IReadOnlyList<InfoPageContentDto>> result = await _mediator.Send(
            new GetAllInfoPageContentsQuery());

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<IReadOnlyList<InfoPageContentDto>>.SuccessResponse(
                result.Data,
                "Info page contents retrieved successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        return StatusCode(
            500,
            ApiResponse<IReadOnlyList<InfoPageContentDto>>.ErrorResponse(
                string.IsNullOrWhiteSpace(errorMessage)
                    ? "An unexpected error occurred."
                    : errorMessage));
    }

    [HttpGet("{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<InfoPageContentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InfoPageContentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InfoPageContentDto>>> GetInfoPageContentBySlug(string slug)
    {
        _logger.LogInformation("Getting info page content by slug: {Slug}", slug);

        Result<InfoPageContentDto> result = await _mediator.Send(new GetInfoPageContentBySlugQuery(slug));

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<InfoPageContentDto>.SuccessResponse(
                result.Data,
                "Info page content retrieved successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();

        if (!string.IsNullOrWhiteSpace(errorMessage) && errorMessage.Contains("not found"))
        {
            return NotFound(ApiResponse<InfoPageContentDto>.ErrorResponse(errorMessage));
        }

        return StatusCode(
            500,
            ApiResponse<InfoPageContentDto>.ErrorResponse(
                string.IsNullOrWhiteSpace(errorMessage)
                    ? "An unexpected error occurred."
                    : errorMessage));
    }

    [HttpPut("{slug}")]
    [Authorize(Roles = "SystemAdmin,ClubAdmin")]
    [ProducesResponseType(typeof(ApiResponse<InfoPageContentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InfoPageContentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<InfoPageContentDto>>> UpdateInfoPageContent(
        string slug,
        [FromBody] UpdateInfoPageContentRequest request)
    {
        _logger.LogInformation("Updating info page content with slug: {Slug}", slug);

        var command = new UpdateInfoPageContentCommand(
            slug,
            request.Title,
            request.ContentHtml,
            User?.Identity?.Name);

        Result<InfoPageContentDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<InfoPageContentDto>.SuccessResponse(
                result.Data,
                "Info page content updated successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        List<string> errorList = result.ValidationFailures.Select(x => x.ErrorMessage).ToList();

        return BadRequest(ApiResponse<InfoPageContentDto>.ErrorResponse(errorMessage, errorList));
    }
}
