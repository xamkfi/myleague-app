using Application.Commands.Floorball.Match;
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
        /// Creates a Event Create match
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("create")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> CreateMatch([FromBody] CreateFloorballMatchRequest request)
        {
            _logger.LogInformation("Creating eventsourcedmatch event");

            if (!DateTime.TryParse(request.ScheduledDateTime, out DateTime scheduledDateTime))
                return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse("Invalid scheduled date and time format"));

            Guid id = Guid.NewGuid();
            CreateEventSourcedFloorballMatchCommand command = new CreateEventSourcedFloorballMatchCommand(id, request.SeasonId, request.HomeTeamId, request.AwayTeamId, scheduledDateTime.ToUniversalTime(), request.Venue);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Match created event successfully"));
            }

            string errorMessage = result.Error ?? "Failed to create match event";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Starts a floorball match
        /// </summary>
        /// <param name="id">Match ID</param>
        /// <returns>Started match details</returns>
        [HttpPut("start-match/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> StartMatch(Guid id)
        {
            _logger.LogInformation("Starting floorball match with ID: {id}", id);

            StartEventSourcedFloorballMatchCommand command = new StartEventSourcedFloorballMatchCommand(id);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Floorball match started successfully"));
            }

            string errorMessage = result.Error ?? "Failed to start floorball match";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Postpones a floorball match.
        /// </summary>
        /// <param name="id">The ID of the match to postpone.</param>
        /// <returns>The postponed match details.</returns>
        [HttpPut("postpone/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> PostponeMatch(Guid id)
        {
            PostponeEventSourcedFloorballMatchCommand command = new PostponeEventSourcedFloorballMatchCommand(id);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Match postponed successfully."));
            }
            return BadRequest(ApiResponse.ErrorResponse(result.Error ?? "Failed to postpone match."));
        }

        /// <summary>
        /// Reschedules a floorball match.
        /// </summary>
        /// <param name="id">The ID of the match to reschedule.</param>
        /// <param name="request">The reschedule request details.</param>
        /// <returns>The rescheduled match details.</returns>
        [HttpPut("reschedule/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RescheduleMatch(Guid id, [FromBody] RescheduleFloorballMatchRequest request)
        {
            if (!DateTime.TryParse(request.NewScheduledDateTime, out DateTime newDateTime))
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid new scheduled date and time format."));
            }

            RescheduleEventSourcedFloorballMatchCommand command = new RescheduleEventSourcedFloorballMatchCommand(id, newDateTime.ToUniversalTime(), request.NewVenue);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Match rescheduled successfully."));
            }
            return BadRequest(ApiResponse.ErrorResponse(result.Error ?? "Failed to reschedule match."));
        }
        
        /// <summary>
        /// Records that the match went to overtime.
        /// </summary>
        /// <param name="id">The match ID.</param>
        /// <returns>The updated match details.</returns>
        [HttpPut("record-overtime/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RecordOvertime(Guid id)
        {
            RecordOvertimeEventCommand command = new RecordOvertimeEventCommand(id);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Overtime recorded successfully."));
            }
            return BadRequest(ApiResponse.ErrorResponse(result.Error ?? "Failed to record overtime."));
        }

        /// <summary>
        /// Records that the match went to shootout.
        /// </summary>
        /// <param name="id">The match ID.</param>
        /// <returns>The updated match details.</returns>
        [HttpPut("record-shootout/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RecordShootout(Guid id)
        {
            RecordShootoutEventCommand command = new RecordShootoutEventCommand(id);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Shootout recorded successfully."));
            }
            return BadRequest(ApiResponse.ErrorResponse(result.Error ?? "Failed to record shootout."));
        }

        /// <summary>
        /// Completes a floorball match.
        /// </summary>
        /// <param name="id">The match ID.</param>
        /// <returns>The completed match details.</returns>
        [HttpPut("complete/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> CompleteMatch(Guid id)
        {
            CompleteEventSourcedFloorballMatchCommand command = new CompleteEventSourcedFloorballMatchCommand(id);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Match completed successfully."));
            }
            return BadRequest(ApiResponse.ErrorResponse(result.Error ?? "Failed to complete match."));
        }
        
        /// <summary>
        /// Cancels a floorball match.
        /// </summary>
        /// <param name="id">The match ID.</param>
        /// <returns>The cancelled match details.</returns>
        [HttpPut("cancel/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> CancelMatch(Guid id)
        {
            CancelEventSourcedFloorballMatchCommand command = new CancelEventSourcedFloorballMatchCommand(id);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Match cancelled successfully."));
            }
            return BadRequest(ApiResponse.ErrorResponse(result.Error ?? "Failed to cancel match."));
        }

        /// <summary>
        /// Add referee to a match
        /// </summary>
        /// <param name="matchId"></param>
        /// <param name="refereeId"></param>
        /// <returns></returns>
        [HttpPut("add-official/{matchId:guid}/{refereeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> AddReferee(Guid matchId, Guid refereeId)
        {
            _logger.LogInformation("Adding official {refereeId} to match {matchId}", refereeId, matchId);

            AddOfficialToEventSourcedMatchCommand command = new AddOfficialToEventSourcedMatchCommand(matchId, refereeId);
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
    }
} 
