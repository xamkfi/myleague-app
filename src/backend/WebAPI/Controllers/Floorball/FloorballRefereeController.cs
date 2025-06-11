using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Application.Commands.Floorball.Referee;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;
using WebAPI.Models.Floorball;

namespace WebAPI.Controllers.Floorball
{
    /// <summary>
    /// Controller for managing floorball referees
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FloorballRefereeController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FloorballRefereeController> _logger;

        /// <summary>
        /// Initializes new instance of FloorballRefereeController class
        /// </summary>
        /// <param name="mediator">Mediator instance for handling commands and queries</param>
        /// <param name="logger">Logger instance for logging</param>
        public FloorballRefereeController(IMediator mediator, ILogger<FloorballRefereeController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new floorball referee
        /// </summary>
        /// <param name="request">Create referee request</param>
        /// <returns>Created referee details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<FloorballRefereeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballRefereeDto>>> CreateReferee([FromBody] CreateFloorballRefereeRequest request)
        {
            _logger.LogInformation("Creating floorball referee for person ID: {personId}", request.PersonId);

            CreateFloorballRefereeCommand command = new CreateFloorballRefereeCommand(
                request.PersonId,
                request.LicenseIssueDate,
                request.LicenseExpiryDate);

            Result<FloorballRefereeDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballRefereeDto>.SuccessResponse(result.Data, "Floorball referee created successfully"));
            }

            string errorMessage = result.Error ?? "Failed to create floorball referee";
            return BadRequest(ApiResponse<FloorballRefereeDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Updates an existing floorball referee
        /// </summary>
        /// <param name="id">Referee ID</param>
        /// <param name="request">Update referee request</param>
        /// <returns>Updated referee details</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballRefereeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballRefereeDto>>> UpdateReferee(Guid id, [FromBody] UpdateFloorballRefereeRequest request)
        {
            _logger.LogInformation("Updating floorball referee with ID: {id}", id);

            UpdateFloorballRefereeCommand command = new UpdateFloorballRefereeCommand(
                id,
                request.LicenseIssueDate,
                request.LicenseExpiryDate,
                request.LicenseLevel,
                request.IsActive);

            Result<FloorballRefereeDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballRefereeDto>.SuccessResponse(result.Data, "Floorball referee updated successfully"));
            }

            string errorMessage = result.Error ?? "Failed to update floorball referee";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballRefereeDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballRefereeDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Deletes a floorball referee
        /// </summary>
        /// <param name="id">Referee ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteReferee(Guid id)
        {
            _logger.LogInformation("Deleting floorball referee with ID: {id}", id);

            DeleteFloorballRefereeCommand command = new DeleteFloorballRefereeCommand(id);
            Result<FloorballRefereeDto> result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse.SuccessResponse("Floorball referee deleted successfully"));
            }

            string errorMessage = result.Error ?? "Failed to delete floorball referee";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse.ErrorResponse(errorMessage));
        }
    }
} 
