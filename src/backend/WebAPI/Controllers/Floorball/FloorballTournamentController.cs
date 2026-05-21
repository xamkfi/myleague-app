using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Application.Common;
using Application.Features.Floorball.Tournaments.Commands;
using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Tournaments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;
using WebAPI.Models.Floorball;

namespace WebAPI.Controllers.Floorball
{
    /// <summary>
    /// Controller for managing floorball tournaments
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FloorballTournamentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FloorballTournamentController> _logger;

        /// <summary>
        /// Initializes new instance of FloorballTournamentController class
        /// </summary>
        /// <param name="mediator">Mediator instance for handling commands and queries</param>
        /// <param name="logger">Logger instance for logging</param>
        public FloorballTournamentController(IMediator mediator, ILogger<FloorballTournamentController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // Converts the WebAPI request shape into the application-layer input. Returns null when
        // the caller did not send a schedule field at all (so the update handler can distinguish
        // "no change" from "clear the schedule" — see UpdateFloorballTournamentHandler).
        private static IReadOnlyList<PlayoffScheduleSlotInput>? MapPlayoffSchedule(List<PlayoffScheduleSlotRequest>? slots)
        {
            if (slots == null)
            {
                return null;
            }
            return slots
                .Select(s => new PlayoffScheduleSlotInput(s.Round, s.Order, s.ScheduledDateTime, s.Venue))
                .ToList();
        }

        /// <summary>
        /// Gets all floorball tournaments
        /// </summary>
        /// <returns>List of all floorball tournaments</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballTournamentDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballTournamentDto>>>> GetAllTournaments()
        {
            _logger.LogInformation("Getting all floorball tournaments");

            GetAllFloorballTournamentsQuery query = new GetAllFloorballTournamentsQuery();
            Result<List<FloorballTournamentDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<List<FloorballTournamentDto>>.SuccessResponse(result.Data.ToList(), "Floorball tournaments retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve floorball tournaments";
            return BadRequest(ApiResponse<List<FloorballTournamentDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Gets all active floorball tournaments
        /// </summary>
        /// <returns>List of active floorball tournaments</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballTournamentDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballTournamentDto>>>> GetActiveTournaments()
        {
            _logger.LogInformation("Getting active floorball tournaments");

            GetActiveFloorballTournamentsQuery query = new GetActiveFloorballTournamentsQuery();
            Result<List<FloorballTournamentDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<List<FloorballTournamentDto>>.SuccessResponse(result.Data.ToList(), "Active floorball tournaments retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve active floorball tournaments";
            return BadRequest(ApiResponse<List<FloorballTournamentDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Gets a floorball tournament by ID
        /// </summary>
        /// <param name="competitionId">Tournament ID</param>
        /// <returns>Tournament details</returns>
        [HttpGet("{competitionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballTournamentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTournamentDto>>> GetTournamentById(Guid competitionId)
        {
            _logger.LogInformation("Getting floorball tournament with ID: {competitionId}", competitionId);

            GetFloorballTournamentByIdQuery query = new GetFloorballTournamentByIdQuery(competitionId);
            Result<FloorballTournamentDto> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTournamentDto>.SuccessResponse(result.Data, "Floorball tournament retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve floorball tournament";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Creates a new floorball tournament
        /// </summary>
        /// <param name="request">Create tournament request</param>
        /// <returns>Created tournament details</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballTournamentDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTournamentDto>>> CreateTournament([FromBody] CreateFloorballTournamentRequest request)
        {
            string sanitizedTournamentNameForLog = (request.Name ?? string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);
            _logger.LogInformation("Creating floorball tournament: {name}", sanitizedTournamentNameForLog);

            IReadOnlyList<PlayoffScheduleSlotInput>? scheduleSlots = MapPlayoffSchedule(request.PlayoffSchedule);

            CreateFloorballTournamentCommand command = new CreateFloorballTournamentCommand(
                request.Name,
                request.StartDate,
                request.EndDate,
                request.Venue,
                request.ContentHtml,
                request.GroupStageNumberOfPeriods,
                request.GroupStagePeriodDurationMinutes,
                request.GroupStageAllowOvertime,
                request.GroupStageOvertimeDurationMinutes,
                request.GroupStageAllowShootout,
                request.PlayoffNumberOfPeriods,
                request.PlayoffPeriodDurationMinutes,
                request.PlayoffAllowOvertime,
                request.PlayoffOvertimeDurationMinutes,
                request.PlayoffAllowShootout,
                request.TeamsAdvancingPerGroup,
                request.HasPlayoffStage,
                request.HasThirdPlaceMatch,
                scheduleSlots
            );

            Result<FloorballTournamentDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetTournamentById),
                    new { competitionId = result.Data.Id },
                    ApiResponse<FloorballTournamentDto>.SuccessResponse(result.Data, "Floorball tournament created successfully")
                );
            }

            string errorMessage = result.Error ?? "Failed to create floorball tournament";
            List<string> errorList = result.GetAllErrors().ToList();

            return BadRequest(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage, errorList));
        }

        /// <summary>
        /// Updates an existing floorball tournament
        /// </summary>
        /// <param name="competitionId">Tournament ID</param>
        /// <param name="request">Update tournament request</param>
        /// <returns>Updated tournament details</returns>
        [HttpPut("{competitionId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballTournamentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTournamentDto>>> UpdateTournament(Guid competitionId, [FromBody] UpdateFloorballTournamentRequest request)
        {
            _logger.LogInformation("Updating floorball tournament with ID: {competitionId}", competitionId);

            IReadOnlyList<PlayoffScheduleSlotInput>? scheduleSlots = MapPlayoffSchedule(request.PlayoffSchedule);

            UpdateFloorballTournamentCommand command = new UpdateFloorballTournamentCommand(
                competitionId,
                request.Name,
                request.StartDate,
                request.EndDate,
                request.Venue,
                request.ContentHtml,
                request.GroupStageNumberOfPeriods,
                request.GroupStagePeriodDurationMinutes,
                request.GroupStageAllowOvertime,
                request.GroupStageOvertimeDurationMinutes,
                request.GroupStageAllowShootout,
                request.PlayoffNumberOfPeriods,
                request.PlayoffPeriodDurationMinutes,
                request.PlayoffAllowOvertime,
                request.PlayoffOvertimeDurationMinutes,
                request.PlayoffAllowShootout,
                request.TeamsAdvancingPerGroup,
                request.HasPlayoffStage,
                request.HasThirdPlaceMatch,
                scheduleSlots
            );

            Result<FloorballTournamentDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTournamentDto>.SuccessResponse(result.Data, "Floorball tournament updated successfully"));
            }

            string errorMessage = result.Error ?? "Failed to update floorball tournament";
            List<string> errorList = result.GetAllErrors().ToList();

            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage, errorList));
        }

        /// <summary>
        /// Deletes a floorball tournament
        /// </summary>
        /// <param name="competitionId">Tournament ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{competitionId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteTournament(Guid competitionId)
        {
            _logger.LogInformation("Deleting floorball tournament with ID: {competitionId}", competitionId);

            DeleteFloorballTournamentCommand command = new DeleteFloorballTournamentCommand(competitionId);
            Result result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse.SuccessResponse("Floorball tournament deleted successfully"));
            }

            string errorMessage = result.Error ?? "Failed to delete floorball tournament";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Starts the group stage of a floorball tournament
        /// </summary>
        /// <param name="competitionId">Tournament ID</param>
        /// <returns>Updated tournament details</returns>
        [HttpPost("{competitionId:guid}/start-group-stage")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballTournamentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTournamentDto>>> StartTournamentGroupStage(Guid competitionId)
        {
            _logger.LogInformation("Starting group stage for floorball tournament with ID: {competitionId}", competitionId);

            StartTournamentGroupStageCommand command = new StartTournamentGroupStageCommand(competitionId);
            Result<FloorballTournamentDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTournamentDto>.SuccessResponse(result.Data, "Tournament group stage started successfully"));
            }

            string errorMessage = result.Error ?? "Failed to start tournament group stage";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Starts the playoff stage of a floorball tournament
        /// </summary>
        /// <param name="competitionId">Tournament ID</param>
        /// <returns>Updated tournament details</returns>
        [HttpPost("{competitionId:guid}/start-playoff-stage")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballTournamentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTournamentDto>>> StartTournamentPlayoffStage(Guid competitionId)
        {
            _logger.LogInformation("Starting playoff stage for floorball tournament with ID: {competitionId}", competitionId);

            StartTournamentPlayoffStageCommand command = new StartTournamentPlayoffStageCommand(competitionId);
            Result<FloorballTournamentDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTournamentDto>.SuccessResponse(result.Data, "Tournament playoff stage started successfully"));
            }

            string errorMessage = result.Error ?? "Failed to start tournament playoff stage";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Gets the playoff bracket for a tournament. Returns an empty Rounds list when no
        /// playoff matches have been generated yet (e.g. tournament is still in GroupStage).
        /// </summary>
        /// <param name="competitionId">Tournament ID</param>
        /// <returns>Bracket grouped by round (Quarterfinals -> Semifinals -> [3rd place] -> Final), with optional champion.</returns>
        [HttpGet("{competitionId:guid}/playoff-bracket")]
        [ProducesResponseType(typeof(ApiResponse<FloorballPlayoffBracketDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballPlayoffBracketDto>>> GetTournamentPlayoffBracket(Guid competitionId)
        {
            _logger.LogInformation("Getting playoff bracket for tournament: {competitionId}", competitionId);

            GetTournamentPlayoffBracketQuery query = new GetTournamentPlayoffBracketQuery(competitionId);
            Result<FloorballPlayoffBracketDto> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballPlayoffBracketDto>.SuccessResponse(result.Data, "Tournament playoff bracket retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve tournament playoff bracket";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballPlayoffBracketDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballPlayoffBracketDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Completes a floorball tournament
        /// </summary>
        /// <param name="competitionId">Tournament ID</param>
        /// <returns>Updated tournament details</returns>
        [HttpPost("{competitionId:guid}/complete")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballTournamentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTournamentDto>>> CompleteTournament(Guid competitionId)
        {
            _logger.LogInformation("Completing floorball tournament with ID: {competitionId}", competitionId);

            CompleteTournamentCommand command = new CompleteTournamentCommand(competitionId);
            Result<FloorballTournamentDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTournamentDto>.SuccessResponse(result.Data, "Floorball tournament completed successfully"));
            }

            string errorMessage = result.Error ?? "Failed to complete floorball tournament";
            List<string> errorList = result.GetAllErrors().ToList();

            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage, errorList));
        }

        /// <summary>
        /// Cancels a floorball tournament
        /// </summary>
        /// <param name="competitionId">Tournament ID</param>
        /// <returns>Updated tournament details</returns>
        [HttpPost("{competitionId:guid}/cancel")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballTournamentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTournamentDto>>> CancelTournament(Guid competitionId)
        {
            _logger.LogInformation("Cancelling floorball tournament with ID: {competitionId}", competitionId);

            CancelTournamentCommand command = new CancelTournamentCommand(competitionId);
            Result<FloorballTournamentDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTournamentDto>.SuccessResponse(result.Data, "Floorball tournament cancelled successfully"));
            }

            string errorMessage = result.Error ?? "Failed to cancel floorball tournament";
            List<string> errorList = result.GetAllErrors().ToList();

            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage, errorList));
        }

        /// <summary>
        /// Adds a group to a floorball tournament
        /// </summary>
        /// <param name="competitionId">Tournament ID</param>
        /// <param name="request">Add group request</param>
        /// <returns>Success message</returns>
        [HttpPost("{competitionId:guid}/groups")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballTournamentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTournamentDto>>> AddGroupToTournament(Guid competitionId, [FromBody] AddGroupToTournamentRequest request)
        {
            _logger.LogInformation("Adding group '{groupName}' to floorball tournament with ID: {competitionId}", request.GroupName, competitionId);

            AddGroupToTournamentCommand command = new AddGroupToTournamentCommand(competitionId, request.GroupName);
            Result<FloorballTournamentDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTournamentDto>.SuccessResponse(result.Data, "Group added to tournament successfully"));
            }

            string errorMessage = result.Error ?? "Failed to add group to tournament";
            List<string> errorList = result.GetAllErrors().ToList();

            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage, errorList));
        }

        /// <summary>
        /// Removes a group from a floorball tournament
        /// </summary>
        /// <param name="competitionId">Tournament ID</param>
        /// <param name="groupId">Group ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{competitionId:guid}/groups/{groupId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballTournamentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTournamentDto>>> RemoveGroupFromTournament(Guid competitionId, Guid groupId)
        {
            _logger.LogInformation("Removing group {groupId} from floorball tournament with ID: {competitionId}", groupId, competitionId);

            RemoveGroupFromTournamentCommand command = new RemoveGroupFromTournamentCommand(competitionId, groupId);
            Result<FloorballTournamentDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTournamentDto>.SuccessResponse(result.Data, "Group removed from tournament successfully"));
            }

            string errorMessage = result.Error ?? "Failed to remove group from tournament";
            List<string> errorList = result.GetAllErrors().ToList();

            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage, errorList));
        }

        /// <summary>
        /// Adds a team to a tournament group
        /// </summary>
        /// <param name="competitionId">Tournament ID</param>
        /// <param name="groupId">Group ID</param>
        /// <param name="request">Add team request</param>
        /// <returns>Success message</returns>
        [HttpPost("{competitionId:guid}/groups/{groupId:guid}/teams")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballTournamentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTournamentDto>>> AddTeamToTournamentGroup(Guid competitionId, Guid groupId, [FromBody] AddTeamToTournamentGroupRequest request)
        {
            _logger.LogInformation("Adding team {teamId} to group {groupId} in tournament {competitionId}", request.TeamId, groupId, competitionId);

            AddTeamToTournamentGroupCommand command = new AddTeamToTournamentGroupCommand(competitionId, groupId, request.TeamId);
            Result<FloorballTournamentDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTournamentDto>.SuccessResponse(result.Data, "Team added to tournament group successfully"));
            }

            string errorMessage = result.Error ?? "Failed to add team to tournament group";
            List<string> errorList = result.GetAllErrors().ToList();

            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage, errorList));
        }

        /// <summary>
        /// Removes a team from a tournament group
        /// </summary>
        /// <param name="competitionId">Tournament ID</param>
        /// <param name="groupId">Group ID</param>
        /// <param name="teamId">Team ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{competitionId:guid}/groups/{groupId:guid}/teams/{teamId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballTournamentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTournamentDto>>> RemoveTeamFromTournamentGroup(Guid competitionId, Guid groupId, Guid teamId)
        {
            _logger.LogInformation("Removing team {teamId} from group {groupId} in tournament {competitionId}", teamId, groupId, competitionId);

            RemoveTeamFromTournamentGroupCommand command = new RemoveTeamFromTournamentGroupCommand(competitionId, groupId, teamId);
            Result<FloorballTournamentDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTournamentDto>.SuccessResponse(result.Data, "Team removed from tournament group successfully"));
            }

            string errorMessage = result.Error ?? "Failed to remove team from tournament group";
            List<string> errorList = result.GetAllErrors().ToList();

            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage, errorList));
        }
    }
}
