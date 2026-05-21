// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Common;
using Application.Features.Common.Feedback.Commands;
using Application.Features.Common.Feedback.DTOs;
using Application.Features.Common.Feedback.Queries;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;

namespace WebAPI.Controllers.Common
{
    /// <summary>
    /// Controller for managing feedback
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FeedbackController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FeedbackController> _logger;

        /// <summary>
        /// Initializes a new instance of the FeedbackController class
        /// </summary>
        /// <param name="mediator">The mediator for controlling commands and queries</param  >
        /// <param name="logger">The logger for this controller</param>
        public FeedbackController(IMediator mediator, ILogger<FeedbackController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all feedback with pagination
        /// </summary>
        /// <param name="request">Query parameters for pagination</param>
        /// <returns>Paginated list of feedback</returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(PaginatedApiResponse<FeedbackListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FeedbackListDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FeedbackListDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FeedbackListDto>>> GetAllFeedback([FromQuery] GetFeedbackRequest request)
        {
            _logger.LogInformation("Getting all feedback with pagination - Page: {request.Page}, PageSize: {request.PageSize}", request.Page, request.PageSize);

            var query = new GetAllFeedbackQuery(request.Page, request.PageSize);

            Result<PagedResult<FeedbackListDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(PaginatedApiResponse<FeedbackListDto>.SuccessResponse(result.Data, "Feedback successfully retrieved"));
            }
            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, PaginatedApiResponse<FeedbackListDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Gets feedback with given ID
        /// </summary>
        /// <param name="id">The Id to get</param>
        /// <returns>Feedback with the requested id</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FeedbackDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<FeedbackDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<FeedbackDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FeedbackDto>>> GetFeedbackById(Guid id)
        {
            _logger.LogInformation("Getting news article by ID: {id}", id);

            var query = new GetFeedbackByIdQuery(id);

            Result<FeedbackDto> result = await _mediator.Send(query);

            if(result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FeedbackDto>.SuccessResponse(result.Data, "Successfully retrieved feedback"));
            }

            if( result.Error?.Contains("not found") == true)
            {
                return NotFound(ApiResponse<FeedbackDto>.ErrorResponse($"Feedback with ID: {id} not found."));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, ApiResponse<FeedbackDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Creates new feedback
        /// </summary>
        /// <param name="request">The feedback creation data</param>
        /// <returns>The created feedback</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FeedbackDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<FeedbackDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<FeedbackDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FeedbackDto>>> CreateFeedback([FromBody] CreateFeedbackRequest request)
        {
            _logger.LogInformation("Creating feedback with title: {request.Title}", request.Title);

            var command = new CreateFeedbackCommand(request.Title, request.FeedbackBody, request.Email);

            Result<FeedbackDto> result = await _mediator.Send(command);

            if(result.IsSuccess && result.Data != null)
            {
                return CreatedAtAction(nameof(GetFeedbackById), new { id = result.Data.Id},
                ApiResponse<FeedbackDto>.SuccessResponse(result.Data, "Feedback created successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            List<string> errorList = result.ValidationFailures.Select(x => x.ErrorMessage).ToList();

            return BadRequest(ApiResponse<FeedbackDto>.ErrorResponse(errorMessage, errorList));
        }

        /// <summary>
        /// Deletes feedback with the given id
        /// </summary>
        /// <param name="id">ID of the feedback to delete</param>
        /// <returns></returns>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteFeedback(Guid id)
        {
            _logger.LogInformation("Deleting feedback with ID: {id}", id);

            var command = new DeleteFeedbackCommand(id);
            Result<bool> result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(result.Data, "Feedback deleted successfully."));
            }

            if(result.Error?.Contains("not found") == true)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse($"Feedback with id {id} not found"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(errorMessage));
        }
    }
}
