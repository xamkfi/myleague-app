using System;
using System.Threading.Tasks;
using System.Linq;
using Application.Commands.Floorball.TeamManager;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAPI.Models.Floorball;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Floorball
{
    /// <summary>
    /// Controller for managing floorball team managers
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FloorballTeamManagerController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FloorballTeamManagerController> _logger;

        /// <summary>
        /// Initializes new instance of FloorballTeamManagerController class
        /// </summary>
        /// <param name="mediator">Mediator instance for handling commands and queries</param>
        /// <param name="logger">Logger instance for logging</param>
        public FloorballTeamManagerController(IMediator mediator, ILogger<FloorballTeamManagerController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new floorball team manager
        /// </summary>
        /// <param name="request">Create team manager request</param>
        /// <returns>Created team manager details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamManagerDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamManagerDto>>> CreateTeamManager([FromBody] FloorballTeamManagerRequest request)
        {
            _logger.LogInformation("Creating floorball team manager for person: {personId}", request.PersonId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for team manager creation: {errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ApiResponse<FloorballTeamManagerDto>.ErrorResponse(ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()));
            }

            CreateFloorballTeamManagerCommand command = new CreateFloorballTeamManagerCommand(
                request.PersonId,
                request.TeamId);

            Result<FloorballTeamManagerDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTeamManagerDto>.SuccessResponse(result.Data, "Floorball team manager created successfully"));
            }

            string errorMessage = result.Error ?? "Failed to create floorball team manager";
            List<string> errorList = result.ValidationFailures.Select(x => x.ErrorMessage).ToList();

            return BadRequest(ApiResponse<FloorballTeamManagerDto>.ErrorResponse(errorMessage, errorList));
        }

        /// <summary>
        /// Updates an existing floorball team manager
        /// </summary>
        /// <param name="id">Team manager ID</param>
        /// <param name="request">Update team manager request</param>
        /// <returns>Updated team manager details</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamManagerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamManagerDto>>> UpdateTeamManager(Guid id, [FromBody] FloorballTeamManagerRequest request)
        {
            _logger.LogInformation("Updating floorball team manager with ID: {id}", id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for team manager update: {errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ApiResponse<FloorballTeamManagerDto>.ErrorResponse(ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()));
            }

            UpdateFloorballTeamManagerCommand command = new UpdateFloorballTeamManagerCommand(
                id,
                request.IsActive);

            Result<FloorballTeamManagerDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTeamManagerDto>.SuccessResponse(result.Data, "Floorball team manager updated successfully"));
            }

            string errorMessage = result.Error ?? "Failed to update floorball team manager";
            List<string> errorList = result.ValidationFailures.Select(x => x.ErrorMessage).ToList();

            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballTeamManagerDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballTeamManagerDto>.ErrorResponse(errorMessage, errorList));
        }

        /// <summary>
        /// Deletes a floorball team manager
        /// </summary>
        /// <param name="id">Team manager ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteTeamManager(Guid id)
        {
            _logger.LogInformation("Deleting floorball team manager with ID: {id}", id);

            DeleteFloorballTeamManagerCommand command = new DeleteFloorballTeamManagerCommand(id);
            Result<FloorballTeamManagerDto> result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse.SuccessResponse("Floorball team manager deleted successfully"));
            }

            string errorMessage = result.Error ?? "Failed to delete floorball team manager";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse.ErrorResponse(errorMessage));
        }
    }
} 
