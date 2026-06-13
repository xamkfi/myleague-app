// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Common;
using Application.Features.Common.FeedbackToggle.Commands;
using Application.Features.Common.FeedbackToggle.DTOs;
using Application.Features.Common.FeedbackToggle.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common
{
    /// <summary>
    /// Controller for managing the FeedbackToggle
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FeedbackToggleController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FeedbackToggleController> _logger;

        /// <summary>
        /// Initializes a new instance of the FeedbackToggle controller
        /// </summary>
        /// <param name="mediator">The mediator</param>
        /// <param name="logger">The logger</param>
        public FeedbackToggleController(IMediator mediator, ILogger<FeedbackToggleController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Gets the feedback toggle
        /// </summary>
        /// <returns>FeedbackToggle data transfer object</returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FeedbackToggleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<FeedbackToggleDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<FeedbackToggleDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FeedbackToggleDto>>> GetFeedbackToggle()
        {
            _logger.LogInformation("Retrieving the FeedbackToggle");

            var query = new GetFeedbackToggleQuery();

            Result<FeedbackToggleDto> results = await _mediator.Send(query);

            if(results.IsSuccess && results.Data != null)
            {
                return Ok(ApiResponse<FeedbackToggleDto>.SuccessResponse(results.Data, "Successfully retrieved the FeedbackToggle"));
            }

            if(results.Error?.Contains("not found") == true)
            {
                return NotFound(ApiResponse<FeedbackToggleDto>.ErrorResponse("FeedbackToggle not found"));
            }

            string errorMessage = results.Error ??  results.GetErrorsString();
            return StatusCode(500, ApiResponse<FeedbackToggleDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Saves the updated state of the feedbacktoggle
        /// </summary>
        /// <param name="request">Request body with the new toggle state</param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(ActionResult<ApiResponse<FeedbackToggleDto>>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ActionResult<ApiResponse<FeedbackToggleDto>>),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ActionResult<ApiResponse<FeedbackToggleDto>>),StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FeedbackToggleDto>>> SaveFeedbackToggle([FromBody] SaveFeedbackToggleCommand request)
        {
            _logger.LogInformation("Saving FeedbackToggle state");

            var command = new SaveFeedbackToggleCommand(request.IsEnabled);

            Result<FeedbackToggleDto> result = await _mediator.Send(command);

            if(result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FeedbackToggleDto>.SuccessResponse(result.Data));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return BadRequest(ApiResponse<FeedbackToggleDto>.ErrorResponse(errorMessage));
        }
    }
}
