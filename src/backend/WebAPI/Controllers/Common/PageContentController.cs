using Application.Common;
using Application.Features.Common.PageContents.Commands;
using Application.Features.Common.PageContents.DTOs;
using Application.Features.Common.PageContents.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common
{
    /// <summary>
    /// Controller for managing page content
    /// </summary>
    [ApiController]
    [Route("api/common/page-content")]
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
        /// <param name="slug">The slug of the page</param>
        /// <returns>The page content</returns>
        [HttpGet("{slug}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PageContentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PageContentDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PageContentDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PageContentDto>>> GetPageContentBySlug(string slug)
        {
            _logger.LogInformation("Getting page content for slug: {Slug}", slug);

            var query = new GetPageContentBySlugQuery(slug);
            Result<PageContentDto> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<PageContentDto>.SuccessResponse(result.Data, "Page content retrieved successfully"));
            }

            if (result.Error?.Contains("not found", System.StringComparison.OrdinalIgnoreCase) == true)
            {
                return NotFound(ApiResponse<PageContentDto>.ErrorResponse($"Page content with slug '{slug}' not found."));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, ApiResponse<PageContentDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Update or create page content
        /// </summary>
        /// <param name="slug">The slug of the page</param>
        /// <param name="dto">The page content update data</param>
        /// <returns>The updated page content</returns>
        [HttpPut("{slug}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<PageContentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PageContentDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<PageContentDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<PageContentDto>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<PageContentDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PageContentDto>>> UpdatePageContent(string slug, [FromBody] PageContentUpdateDto dto)
        {
            _logger.LogInformation("Updating page content for slug: {Slug}", slug);

            // Get the current user's email or name
            string user = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "Admin";

            var command = new UpdatePageContentCommand(slug, dto.Title, dto.ContentHtml, user);
            Result<PageContentDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<PageContentDto>.SuccessResponse(result.Data, "Page content updated successfully"));
            }

            if (result.Error?.Contains("not found") == true)
            {
                return BadRequest(ApiResponse<PageContentDto>.ErrorResponse("Updating page content failed."));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, ApiResponse<PageContentDto>.ErrorResponse(errorMessage));
        }
    }
}
