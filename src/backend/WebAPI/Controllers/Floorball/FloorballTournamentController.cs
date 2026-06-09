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
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Floorball;

namespace WebAPI.Controllers.Floorball
{
    /// <summary>
    /// Controller for managing floorball tournaments
    /// </summary>
    [Route("api/[controller]")]
    public class FloorballTournamentController : BaseApiController
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

            return HandleResult(result, "Floorball tournaments retrieved successfully", "Failed to retrieve floorball tournaments");
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

            return HandleResult(result, "Active floorball tournaments retrieved successfully", "Failed to retrieve active floorball tournaments");
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

            return HandleResult(result, "Floorball tournament retrieved successfully", "Failed to retrieve floorball tournament");
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

            if (result.IsSuccess && result.Data is not null)
            {
                return CreatedAtAction(
                    nameof(GetTournamentById),
                    new { competitionId = result.Data.Id },
                    ApiResponse<FloorballTournamentDto>.SuccessResponse(result.Data, "Floorball tournament created successfully")
                );
            }

            return ToErrorResponse(result, "Failed to create floorball tournament");
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

            return HandleResult(result, "Floorball tournament updated successfully", "Failed to update floorball tournament");
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

            return HandleVoidResult(result, "Floorball tournament deleted successfully", "Failed to delete floorball tournament");
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

            return HandleResult(result, "Tournament group stage started successfully", "Failed to start tournament group stage");
        }

        /// <summary>
        /// Replaces the tournament's pre-defined playoff schedule slots. Authoritative — slots
        /// not present in the request are removed. Send an empty list to clear the schedule.
        /// Rejected with 400 once the playoff stage has started (the domain entity owns the
        /// lifecycle rule; see <c>FloorballTournament.SetPlayoffSchedule</c>).
        /// </summary>
        /// <param name="competitionId">Tournament ID</param>
        /// <param name="request">Full replacement slot list</param>
        /// <returns>Updated tournament with the new schedule reflected in its DTO</returns>
        [HttpPut("{competitionId:guid}/playoff-schedule")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballTournamentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTournamentDto>>> UpdatePlayoffSchedule(
            Guid competitionId,
            [FromBody] UpdatePlayoffScheduleRequest request)
        {
            _logger.LogInformation(
                "Updating playoff schedule for tournament {competitionId} with {slotCount} slot(s)",
                competitionId,
                request?.Slots?.Count ?? 0);

            // Reuse the existing PlayoffScheduleSlotRequest → PlayoffScheduleSlotInput mapping
            // so the JSON shape stays consistent across create/update/schedule endpoints.
            IReadOnlyList<PlayoffScheduleSlotInput> slots =
                MapPlayoffSchedule(request?.Slots) ?? Array.Empty<PlayoffScheduleSlotInput>();

            UpdateTournamentPlayoffScheduleCommand command =
                new UpdateTournamentPlayoffScheduleCommand(competitionId, slots);
            Result<FloorballTournamentDto> result = await _mediator.Send(command);

            return HandleResult(result, "Tournament playoff schedule updated successfully", "Failed to update tournament playoff schedule");
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

            return HandleResult(result, "Tournament playoff stage started successfully", "Failed to start tournament playoff stage");
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

            return HandleResult(result, "Tournament playoff bracket retrieved successfully", "Failed to retrieve tournament playoff bracket");
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

            return HandleResult(result, "Floorball tournament completed successfully", "Failed to complete floorball tournament");
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

            return HandleResult(result, "Floorball tournament cancelled successfully", "Failed to cancel floorball tournament");
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

            return HandleResult(result, "Group added to tournament successfully", "Failed to add group to tournament");
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

            return HandleResult(result, "Group removed from tournament successfully", "Failed to remove group from tournament");
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

            return HandleResult(result, "Team added to tournament group successfully", "Failed to add team to tournament group");
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

            return HandleResult(result, "Team removed from tournament group successfully", "Failed to remove team from tournament group");
        }
    }
}
