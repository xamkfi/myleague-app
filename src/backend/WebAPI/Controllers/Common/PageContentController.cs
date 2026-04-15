// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.PageContent.Commands;
using Application.Features.Common.PageContent.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common
{
    /// <summary>
    /// Controller for managing page content
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PageContentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PageContentController> _logger;

        /// <summary>
        /// Initializes a new instance of the PageContentController class
        /// </summary>
        /// <param name="mediator">The mediator for handling commands and queries</param>
        /// <param name="logger">The logger for this controller</param>
        public PageContentController(IMediator mediator, ILogger<PageContentController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get page content by slug
        /// </summary>
        /// <param name="slug">The page slug</param>
        /// <returns>The page content details</returns>
        [HttpGet("{slug}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PageContentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PageContentDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PageContentDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PageContentDto>>> GetPageContentBySlug(string slug)
        {
            _logger.LogInformation("Getting page content by slug: {Slug}", slug);

            var query = new GetPageContentBySlugQuery(slug);
            Result<PageContentDto> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<PageContentDto>.SuccessResponse(result.Data, "Page content retrieved successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();

            if (!string.IsNullOrWhiteSpace(errorMessage) && errorMessage.Contains("not found"))
            {
                return NotFound(ApiResponse<PageContentDto>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<PageContentDto>.ErrorResponse(
                string.IsNullOrWhiteSpace(errorMessage) ? "An unexpected error occurred." : errorMessage));
        }

        /// <summary>
        /// Create or update page content by slug
        /// </summary>
        /// <param name="slug">The page slug</param>
        /// <param name="request">The page content update data</param>
        /// <returns>The created or updated page content</returns>
        [HttpPut("{slug}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<PageContentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PageContentDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<PageContentDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PageContentDto>>> UpdatePageContent(string slug, [FromBody] UpdatePageContentRequest request)
        {
            _logger.LogInformation("Updating page content with slug: {Slug}", slug);

            var command = new UpdatePageContentCommand(
                slug,
                request.Title,
                request.ContentHtml,
                User?.Identity?.Name
            );

            Result<PageContentDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<PageContentDto>.SuccessResponse(result.Data, "Page content updated successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            List<string> errorList = result.ValidationFailures.Select(x => x.ErrorMessage).ToList();

            return BadRequest(ApiResponse<PageContentDto>.ErrorResponse(errorMessage, errorList));
        }

        [HttpPut("{slug}/rules/{ruleId}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<PageContentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PageContentDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<PageContentDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PageContentDto>>> UpdateRule(
    string slug,
    string ruleId,
    [FromBody] UpdatePageRuleRequest request)
        {
            _logger.LogInformation("Updating rule {RuleId} on page {Slug}", ruleId, slug);

            var command = new UpdatePageRuleCommand(slug, ruleId, request.RuleHtml);
            Result<PageContentDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<PageContentDto>.SuccessResponse(result.Data, "Rule updated successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();

            if (!string.IsNullOrWhiteSpace(errorMessage) && errorMessage.Contains("not found"))
            {
                return NotFound(ApiResponse<PageContentDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<PageContentDto>.ErrorResponse(errorMessage));
        }

        [HttpDelete("{slug}/rules/{ruleId}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<PageContentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PageContentDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PageContentDto>>> DeleteRule(
            string slug,
            string ruleId)
        {
            _logger.LogInformation("Deleting rule {RuleId} from page {Slug}", ruleId, slug);

            var command = new DeletePageRuleCommand(slug, ruleId);
            Result<PageContentDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<PageContentDto>.SuccessResponse(result.Data, "Rule deleted successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();

            if (!string.IsNullOrWhiteSpace(errorMessage) && errorMessage.Contains("not found"))
            {
                return NotFound(ApiResponse<PageContentDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<PageContentDto>.ErrorResponse(errorMessage));
        }
    }
}
