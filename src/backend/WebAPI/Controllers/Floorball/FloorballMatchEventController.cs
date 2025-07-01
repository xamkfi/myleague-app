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

        /// <summary>
        /// Creates a new event-sourced floorball match
        /// </summary>
        /// <param name="request">Match creation details</param>
        /// <returns>Created match</returns>
        [HttpPost("match")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> CreateEventSourcedMatch([FromBody] CreateEventSourcedFloorballMatchRequest request)
        {
            Guid matchId = Guid.NewGuid();
            _logger.LogInformation("Creating event-sourced floorball match with ID {matchId}", matchId);

            if (!DateTime.TryParse(request.ScheduledDateTime, out DateTime scheduledDateTime))
                return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse("Invalid scheduled date and time format"));

            CreateEventSourcedFloorballMatchCommand command = new CreateEventSourcedFloorballMatchCommand(
                matchId,
                request.SeasonId,
                request.HomeTeamId,
                request.AwayTeamId,
                scheduledDateTime,
                request.Venue
            );

            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(CreateEventSourcedMatch),
                    new { id = matchId },
                    ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Event-sourced match created successfully")
                );
            }

            string errorMessage = result.Error ?? "Failed to create event-sourced match";
            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Starts an event-sourced floorball match
        /// </summary>
        /// <param name="matchId">ID of the match to start</param>
        /// <returns>Updated match</returns>
        [HttpPost("match/{matchId:guid}/start")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> StartEventSourcedMatch(Guid matchId)
        {
            _logger.LogInformation("Starting event-sourced floorball match {matchId}", matchId);

            StartEventSourcedFloorballMatchCommand command = new StartEventSourcedFloorballMatchCommand(matchId);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Match started successfully"));
            }

            string errorMessage = result.Error ?? "Failed to start match";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Adds an official to an event-sourced floorball match
        /// </summary>
        /// <param name="request">Official assignment details</param>
        /// <returns>Updated match</returns>
        [HttpPost("match/official")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> AddOfficialToMatch([FromBody] AddOfficialToMatchRequest request)
        {
            _logger.LogInformation("Adding official {refereeId} to match {matchId}", request.RefereeId, request.MatchId);

            AddOfficialToEventSourcedMatchCommand command = new AddOfficialToEventSourcedMatchCommand(
                request.MatchId,
                request.RefereeId
            );

            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Official added to match successfully"));
            }

            string errorMessage = result.Error ?? "Failed to add official to match";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Records shootout in an event-sourced floorball match
        /// </summary>
        /// <param name="matchId">ID of the match to record shootout</param>
        /// <returns>Updated match</returns>
        [HttpPost("match/{matchId:guid}/shootout")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RecordShootout(Guid matchId)
        {
            _logger.LogInformation("Recording shootout for match {matchId}", matchId);

            RecordShootoutEventCommand command = new RecordShootoutEventCommand(matchId);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Shootout recorded successfully"));
            }

            string errorMessage = result.Error ?? "Failed to record shootout";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Records overtime in an event-sourced floorball match
        /// </summary>
        /// <param name="matchId">ID of the match to record overtime</param>
        /// <returns>Updated match</returns>
        [HttpPost("match/{matchId:guid}/overtime")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RecordOvertime(Guid matchId)
        {
            _logger.LogInformation("Recording overtime for match {matchId}", matchId);

            RecordOvertimeEventCommand command = new RecordOvertimeEventCommand(matchId);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Overtime recorded successfully"));
            }

            string errorMessage = result.Error ?? "Failed to record overtime";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Postpones an event-sourced floorball match
        /// </summary>
        /// <param name="matchId">ID of the match to postpone</param>
        /// <returns>Updated match</returns>
        [HttpPost("match/{matchId:guid}/postpone")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> PostponeMatch(Guid matchId)
        {
            _logger.LogInformation("Postponing match {matchId}", matchId);

            PostponeEventSourcedFloorballMatchCommand command = new PostponeEventSourcedFloorballMatchCommand(matchId);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Match postponed successfully"));
            }

            string errorMessage = result.Error ?? "Failed to postpone match";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Cancels an event-sourced floorball match
        /// </summary>
        /// <param name="matchId">ID of the match to cancel</param>
        /// <returns>Updated match</returns>
        [HttpPost("match/{matchId:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> CancelMatch(Guid matchId)
        {
            _logger.LogInformation("Canceling match {matchId}", matchId);

            CancelEventSourcedFloorballMatchCommand command = new CancelEventSourcedFloorballMatchCommand(matchId);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Match canceled successfully"));
            }

            string errorMessage = result.Error ?? "Failed to cancel match";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Completes an event-sourced floorball match
        /// </summary>
        /// <param name="matchId">ID of the match to complete</param>
        /// <returns>Updated match</returns>
        [HttpPost("match/{matchId:guid}/complete")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> CompleteMatch(Guid matchId)
        {
            _logger.LogInformation("Completing match {matchId}", matchId);

            CompleteEventSourcedFloorballMatchCommand command = new CompleteEventSourcedFloorballMatchCommand(matchId);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Match completed successfully"));
            }

            string errorMessage = result.Error ?? "Failed to complete match";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Reschedules an event-sourced floorball match
        /// </summary>
        /// <param name="request">Reschedule details</param>
        /// <returns>Updated match</returns>
        [HttpPost("match/reschedule")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RescheduleMatch([FromBody] RescheduleEventSourcedFloorballMatchRequest request)
        {
            _logger.LogInformation("Rescheduling match {matchId} to {newDateTime}", request.MatchId, request.NewDateTime);

            RescheduleEventSourcedFloorballMatchCommand command = new RescheduleEventSourcedFloorballMatchCommand(
                request.MatchId,
                request.NewDateTime,
                request.NewVenue
            );

            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Match rescheduled successfully"));
            }

            string errorMessage = result.Error ?? "Failed to reschedule match";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }
    }
} 
