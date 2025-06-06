using System;
using System.Threading.Tasks;
using Application.Commands.Floorball.Coach;
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
    /// Controller for managing floorball coaches
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FloorballCoachController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FloorballCoachController> _logger;

        /// <summary>
        /// Initializes new instance of FloorballCoachController class
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public FloorballCoachController(IMediator mediator, ILogger<FloorballCoachController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new floorball coach
        /// </summary>
        /// <param name="request">Create coach request</param>
        /// <returns>Created coach details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<FloorballCoachDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballCoachDto>>> CreateCoach([FromBody] CreateFloorballCoachRequest request)
        {
            _logger.LogInformation("Creating floorball coach for person ID: {personId}", request.PersonId);

            CreateFloorballCoachCommand command = new CreateFloorballCoachCommand(
                request.PersonId,
                request.YearsOfExperience,
                request.CertificationLevel,
                request.Specialization);

            Result<FloorballCoachDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballCoachDto>.SuccessResponse(result.Data, "Floorball coach created successfully"));
            }

            string errorMessage = result.Error ?? "Failed to create floorball coach";
            return BadRequest(ApiResponse<FloorballCoachDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Updates an existing floorball coach
        /// </summary>
        /// <param name="id">Coach ID</param>
        /// <param name="request">Update coach request</param>
        /// <returns>Updated coach details</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballCoachDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballCoachDto>>> UpdateCoach(Guid id, [FromBody] UpdateFloorballCoachRequest request)
        {
            _logger.LogInformation("Updating floorball coach with ID: {id}", id);

            UpdateFloorballCoachCommand command = new UpdateFloorballCoachCommand(
                id,
                request.IsActive,
                request.YearsOfExperience,
                request.CertificationLevel,
                request.Specialization);

            Result<FloorballCoachDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballCoachDto>.SuccessResponse(result.Data, "Floorball coach updated successfully"));
            }

            string errorMessage = result.Error ?? "Failed to update floorball coach";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballCoachDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballCoachDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Deletes a floorball coach
        /// </summary>
        /// <param name="id">Coach ID</param>
        /// <returns>Deleted coach details</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballCoachDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballCoachDto>>> DeleteCoach(Guid id)
        {
            _logger.LogInformation("Deleting floorball coach with ID: {id}", id);

            DeleteFloorballCoachCommand command = new DeleteFloorballCoachCommand(id);
            Result<FloorballCoachDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballCoachDto>.SuccessResponse(result.Data, "Floorball coach deleted successfully"));
            }

            string errorMessage = result.Error ?? "Failed to delete floorball coach";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballCoachDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballCoachDto>.ErrorResponse(errorMessage));
        }
    }
} 