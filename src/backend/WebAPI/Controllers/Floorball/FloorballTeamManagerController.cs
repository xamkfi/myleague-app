using Application.Common;
using Application.Features.Floorball.TeamManagers.Commands;
using Application.Features.Floorball.TeamManagers.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Floorball;

namespace WebAPI.Controllers.Floorball
{
    /// <summary>
    /// Controller for managing floorball team managers
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public class FloorballTeamManagerController : BaseApiController
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
                _logger.LogWarning(
                    "Invalid model state for team manager creation: {errors}",
                    SanitizeForLog(string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
                return BadRequest(ApiResponse<FloorballTeamManagerDto>.ErrorResponse(ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()));
            }

            Result<FloorballTeamManagerDto> result = await _mediator.Send(
                new CreateFloorballTeamManagerCommand(request.PersonId, request.TeamId));

            return HandleResult(result, "Floorball team manager created successfully", "Failed to create floorball team manager");
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
                _logger.LogWarning(
                    "Invalid model state for team manager update: {errors}",
                    SanitizeForLog(string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
                return BadRequest(ApiResponse<FloorballTeamManagerDto>.ErrorResponse(ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()));
            }

            Result<FloorballTeamManagerDto> result = await _mediator.Send(
                new UpdateFloorballTeamManagerCommand(id, request.IsActive));

            return HandleResult(result, "Floorball team manager updated successfully", "Failed to update floorball team manager");
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

            Result<FloorballTeamManagerDto> result = await _mediator.Send(new DeleteFloorballTeamManagerCommand(id));

            return HandleVoidResult(result, "Floorball team manager deleted successfully", "Failed to delete floorball team manager");
        }
    }
}
