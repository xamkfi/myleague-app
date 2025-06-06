using Application.Commands.Floorball.MatchEvent;
using Application.Common;
using Application.DTOs.Floorball;
using Domain.Enums.Floorball;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;
using WebAPI.Models.Floorball;

namespace WebAPI.Controllers.Floorball
{
    /// <summary>
    /// Controller for managing floorball match events (goals and penalties)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FloorballMatchEventController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FloorballMatchEventController> _logger;

        /// <summary>
        /// Initializes new instance of FloorballMatchEventController class
        /// </summary>
        /// <param name="mediator">Mediator for handling commands and queries</param>
        /// <param name="logger">Logger for the controller</param>
        public FloorballMatchEventController(IMediator mediator, ILogger<FloorballMatchEventController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Records a new goal event in a floorball match
        /// </summary>
        /// <param name="request">Goal event details</param>
        /// <returns>Created goal event</returns>
        [HttpPost("goal")]
        [ProducesResponseType(typeof(ApiResponse<FloorballGoalEventDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballGoalEventDto>>> RecordGoal([FromBody] RecordGoalEventRequest request)
        {
            _logger.LogInformation("Recording goal event for match {matchId}", request.MatchId);

            RecordGoalEventCommand command = new RecordGoalEventCommand(
                request.MatchId,
                request.TeamId,
                request.PlayerId,
                request.AssisterId,
                request.PeriodNumber,
                request.TimeInSeconds,
                request.WasInOvertime,
                request.WasInShootout
            );

            Result<FloorballGoalEventDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(RecordGoal),
                    new { matchId = request.MatchId },
                    ApiResponse<FloorballGoalEventDto>.SuccessResponse(result.Data, "Goal event recorded successfully")
                );
            }

            string errorMessage = result.Error ?? "Failed to record goal event";
            return BadRequest(ApiResponse<FloorballGoalEventDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Records a new penalty event in a floorball match
        /// </summary>
        /// <param name="request">Penalty event details</param>
        /// <returns>Created penalty event</returns>
        [HttpPost("penalty")]
        [ProducesResponseType(typeof(ApiResponse<FloorballPenaltyEventDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballPenaltyEventDto>>> RecordPenalty([FromBody] RecordPenaltyEventRequest request)
        {
            _logger.LogInformation("Recording penalty event for match {matchId}", request.MatchId);

            RecordPenaltyEventCommand command = new RecordPenaltyEventCommand(
                request.MatchId,
                request.TeamId,
                request.PlayerId,
                (FloorballPenaltyType)Enum.Parse(typeof(FloorballPenaltyType), request.PenaltyType),
                request.DurationMinutes,
                request.PeriodNumber,
                request.TimeInSeconds,
                string.Empty // Description is optional
            );

            Result<FloorballPenaltyEventDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(RecordPenalty),
                    new { matchId = request.MatchId },
                    ApiResponse<FloorballPenaltyEventDto>.SuccessResponse(result.Data, "Penalty event recorded successfully")
                );
            }

            string errorMessage = result.Error ?? "Failed to record penalty event";
            return BadRequest(ApiResponse<FloorballPenaltyEventDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Updates an existing goal event
        /// </summary>
        /// <param name="request">Updated goal event details</param>
        /// <returns>Updated goal event</returns>
        [HttpPut("goal")]
        [ProducesResponseType(typeof(ApiResponse<FloorballGoalEventDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballGoalEventDto>>> UpdateGoal([FromBody] UpdateGoalEventRequest request)
        {
            _logger.LogInformation("Updating goal event {eventId} for match {matchId}", request.EventId, request.MatchId);

            UpdateGoalEventCommand command = new UpdateGoalEventCommand(
                request.EventId,
                request.AssisterId,
                request.PeriodNumber,
                request.TimeInSeconds,
                request.WasInOvertime,
                request.WasInShootout
            );

            Result<FloorballGoalEventDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballGoalEventDto>.SuccessResponse(result.Data, "Goal event updated successfully"));
            }

            string errorMessage = result.Error ?? "Failed to update goal event";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballGoalEventDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballGoalEventDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Updates an existing penalty event
        /// </summary>
        /// <param name="request">Updated penalty event details</param>
        /// <returns>Updated penalty event</returns>
        [HttpPut("penalty")]
        [ProducesResponseType(typeof(ApiResponse<FloorballPenaltyEventDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballPenaltyEventDto>>> UpdatePenalty([FromBody] UpdatePenaltyEventRequest request)
        {
            _logger.LogInformation("Updating penalty event {eventId} for match {matchId}", request.EventId, request.MatchId);

            UpdatePenaltyEventCommand command = new UpdatePenaltyEventCommand(
                request.EventId,
                (FloorballPenaltyType)Enum.Parse(typeof(FloorballPenaltyType), request.PenaltyType),
                request.DurationMinutes,
                request.PeriodNumber,
                request.TimeInSeconds,
                string.Empty // Description is optional
            );

            Result<FloorballPenaltyEventDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballPenaltyEventDto>.SuccessResponse(result.Data, "Penalty event updated successfully"));
            }

            string errorMessage = result.Error ?? "Failed to update penalty event";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballPenaltyEventDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballPenaltyEventDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Deletes a goal event
        /// </summary>
        /// <param name="id">ID of the goal event to delete</param>
        /// <returns>Success response</returns>
        [HttpDelete("goal/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteGoal(Guid id)
        {
            _logger.LogInformation("Deleting goal event {id}", id);

            DeleteGoalEventCommand command = new DeleteGoalEventCommand(id);
            Result<FloorballGoalEventDto> result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse.SuccessResponse("Goal event deleted successfully"));
            }

            string errorMessage = result.Error ?? "Failed to delete goal event";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Deletes a penalty event
        /// </summary>
        /// <param name="id">ID of the penalty event to delete</param>
        /// <returns>Success response</returns>
        [HttpDelete("penalty/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeletePenalty(Guid id)
        {
            _logger.LogInformation("Deleting penalty event {id}", id);

            DeletePenaltyEventCommand command = new DeletePenaltyEventCommand(id);
            Result<FloorballPenaltyEventDto> result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse.SuccessResponse("Penalty event deleted successfully"));
            }

            string errorMessage = result.Error ?? "Failed to delete penalty event";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse.ErrorResponse(errorMessage));
        }
    }
} 