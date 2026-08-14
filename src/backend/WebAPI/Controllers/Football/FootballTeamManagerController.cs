using Domain.Constants;
using Application.Common;
using Application.Features.Football.TeamManagers.Commands;
using Application.Features.Football.TeamManagers.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Football;

namespace WebAPI.Controllers.Football
{
    /// <summary>
    /// Controller for managing football team managers
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [Route("api/[controller]")]
    public class FootballTeamManagerController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FootballTeamManagerController> _logger;

        /// <summary>
        /// Initializes new instance of FootballTeamManagerController class
        /// </summary>
        /// <param name="mediator">Mediator instance for handling commands and queries</param>
        /// <param name="logger">Logger instance for logging</param>
        public FootballTeamManagerController(IMediator mediator, ILogger<FootballTeamManagerController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new football team manager
        /// </summary>
        /// <param name="request">Create team manager request</param>
        /// <returns>Created team manager details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<FootballTeamManagerDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FootballTeamManagerDto>>> CreateTeamManager([FromBody] FootballTeamManagerRequest request)
        {
            _logger.LogInformation("Creating football team manager for person: {personId}", request.PersonId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Invalid model state for team manager creation: {errors}",
                    SanitizeForLog(string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
                return BadRequest(ApiResponse<FootballTeamManagerDto>.ErrorResponse(ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()));
            }

            Result<FootballTeamManagerDto> result = await _mediator.Send(
                new CreateFootballTeamManagerCommand(request.PersonId, request.TeamId));

            return HandleResult(result, "Football team manager created successfully", "Failed to create football team manager");
        }

        /// <summary>
        /// Updates an existing football team manager
        /// </summary>
        /// <param name="id">Team manager ID</param>
        /// <param name="request">Update team manager request</param>
        /// <returns>Updated team manager details</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FootballTeamManagerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FootballTeamManagerDto>>> UpdateTeamManager(Guid id, [FromBody] FootballTeamManagerRequest request)
        {
            _logger.LogInformation("Updating football team manager with ID: {id}", id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Invalid model state for team manager update: {errors}",
                    SanitizeForLog(string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
                return BadRequest(ApiResponse<FootballTeamManagerDto>.ErrorResponse(ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()));
            }

            Result<FootballTeamManagerDto> result = await _mediator.Send(
                new UpdateFootballTeamManagerCommand(id, request.IsActive));

            return HandleResult(result, "Football team manager updated successfully", "Failed to update football team manager");
        }

        /// <summary>
        /// Deletes a football team manager
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
            _logger.LogInformation("Deleting football team manager with ID: {id}", id);

            Result<FootballTeamManagerDto> result = await _mediator.Send(new DeleteFootballTeamManagerCommand(id));

            return HandleVoidResult(result, "Football team manager deleted successfully", "Failed to delete football team manager");
        }
    }
}
