using Domain.Constants;
using Application.Common;
using Application.Features.Football.Tournaments.Commands;
using Application.Features.Football.Tournaments.DTOs;
using Application.Features.Football.Tournaments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Football;

namespace WebAPI.Controllers.Football;

/// <summary>
/// Controller for managing football tournaments
/// </summary>
[Route("api/[controller]")]
public class FootballTournamentController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<FootballTournamentController> _logger;

    /// <summary>
    /// Initializes a new instance of the FootballTournamentController class
    /// </summary>
    public FootballTournamentController(IMediator mediator, ILogger<FootballTournamentController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private static IReadOnlyList<FootballPlayoffScheduleSlotInput>? MapPlayoffSchedule(List<PlayoffScheduleSlotRequest>? slots)
    {
        if (slots == null)
        {
            return null;
        }
        return slots
            .Select(s => new FootballPlayoffScheduleSlotInput(s.Round, s.Order, s.ScheduledDateTime, s.Venue))
            .ToList();
    }

    /// <summary>
    /// Get all tournaments
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<FootballTournamentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<FootballTournamentDto>>>> GetAllTournaments(
        [FromQuery] Domain.Enums.Common.TeamCategory? teamCategory = null)
    {
        _logger.LogInformation("Getting all football tournaments");

        GetAllFootballTournamentsQuery query = new GetAllFootballTournamentsQuery(teamCategory);
        Result<List<FootballTournamentDto>> result = await _mediator.Send(query);

        return HandleResult(result, "Football tournaments retrieved successfully", "Failed to retrieve football tournaments");
    }

    /// <summary>
    /// Get active tournaments
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<List<FootballTournamentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<FootballTournamentDto>>>> GetActiveTournaments(
        [FromQuery] Domain.Enums.Common.TeamCategory? teamCategory = null)
    {
        _logger.LogInformation("Getting active football tournaments");

        GetActiveFootballTournamentsQuery query = new GetActiveFootballTournamentsQuery(teamCategory);
        Result<List<FootballTournamentDto>> result = await _mediator.Send(query);

        return HandleResult(result, "Active football tournaments retrieved successfully", "Failed to retrieve active football tournaments");
    }

    /// <summary>
    /// Get tournament by id
    /// </summary>
    [HttpGet("{competitionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FootballTournamentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballTournamentDto>>> GetTournamentById(Guid competitionId)
    {
        _logger.LogInformation("Getting football tournament with ID: {competitionId}", competitionId);

        GetFootballTournamentByIdQuery query = new GetFootballTournamentByIdQuery(competitionId);
        Result<FootballTournamentDto> result = await _mediator.Send(query);

        return HandleResult(result, "Football tournament retrieved successfully", "Failed to retrieve football tournament");
    }

    /// <summary>
    /// Create tournament
    /// </summary>
    [HttpPost]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballTournamentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballTournamentDto>>> CreateTournament([FromBody] CreateFootballTournamentRequest request)
    {
        _logger.LogInformation("Creating football tournament: {name}", SanitizeForLog(request.Name));

        IReadOnlyList<FootballPlayoffScheduleSlotInput>? scheduleSlots = MapPlayoffSchedule(request.PlayoffSchedule);

        CreateFootballTournamentCommand command = new CreateFootballTournamentCommand(
            request.Name,
            request.StartDate,
            request.EndDate,
            request.Venue,
            request.ContentHtml,
            request.GroupStageNumberOfHalves,
            request.GroupStageHalfDurationMinutes,
            request.GroupStagePlayersOnField,
            request.GroupStageRequireGoalkeeper,
            request.GroupStageMaxSubstitutions,
            request.GroupStageRequireOfficialsToStart,
            request.GroupStageAllowExtraTime,
            request.GroupStageExtraTimeHalfCount,
            request.GroupStageExtraTimeHalfDurationMinutes,
            request.GroupStageAllowPenaltyShootout,
            request.PlayoffNumberOfHalves,
            request.PlayoffHalfDurationMinutes,
            request.PlayoffPlayersOnField,
            request.PlayoffRequireGoalkeeper,
            request.PlayoffMaxSubstitutions,
            request.PlayoffRequireOfficialsToStart,
            request.PlayoffAllowExtraTime,
            request.PlayoffExtraTimeHalfCount,
            request.PlayoffExtraTimeHalfDurationMinutes,
            request.PlayoffAllowPenaltyShootout,
            request.TeamsAdvancingPerGroup,
            request.HasPlayoffStage,
            request.HasThirdPlaceMatch,
            scheduleSlots,
            request.TeamCategory ?? Domain.Enums.Common.TeamCategory.Adult
        );

        Result<FootballTournamentDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data is not null)
        {
            return CreatedAtAction(
                nameof(GetTournamentById),
                new { competitionId = result.Data.Id },
                ApiResponse<FootballTournamentDto>.SuccessResponse(result.Data, "Football tournament created successfully")
            );
        }

        return ToErrorResponse(result, "Failed to create football tournament");
    }

    /// <summary>
    /// Update tournament
    /// </summary>
    [HttpPut("{competitionId:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballTournamentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballTournamentDto>>> UpdateTournament(Guid competitionId, [FromBody] UpdateFootballTournamentRequest request)
    {
        _logger.LogInformation("Updating football tournament with ID: {competitionId}", competitionId);

        IReadOnlyList<FootballPlayoffScheduleSlotInput>? scheduleSlots = MapPlayoffSchedule(request.PlayoffSchedule);

        UpdateFootballTournamentCommand command = new UpdateFootballTournamentCommand(
            competitionId,
            request.Name,
            request.StartDate,
            request.EndDate,
            request.Venue,
            request.ContentHtml,
            request.GroupStageNumberOfHalves,
            request.GroupStageHalfDurationMinutes,
            request.GroupStagePlayersOnField,
            request.GroupStageRequireGoalkeeper,
            request.GroupStageMaxSubstitutions,
            request.GroupStageRequireOfficialsToStart,
            request.GroupStageAllowExtraTime,
            request.GroupStageExtraTimeHalfCount,
            request.GroupStageExtraTimeHalfDurationMinutes,
            request.GroupStageAllowPenaltyShootout,
            request.PlayoffNumberOfHalves,
            request.PlayoffHalfDurationMinutes,
            request.PlayoffPlayersOnField,
            request.PlayoffRequireGoalkeeper,
            request.PlayoffMaxSubstitutions,
            request.PlayoffRequireOfficialsToStart,
            request.PlayoffAllowExtraTime,
            request.PlayoffExtraTimeHalfCount,
            request.PlayoffExtraTimeHalfDurationMinutes,
            request.PlayoffAllowPenaltyShootout,
            request.TeamsAdvancingPerGroup,
            request.HasPlayoffStage,
            request.HasThirdPlaceMatch,
            scheduleSlots,
            request.TeamCategory
        );

        Result<FootballTournamentDto> result = await _mediator.Send(command);

        return HandleResult(result, "Football tournament updated successfully", "Failed to update football tournament");
    }

    /// <summary>
    /// Delete tournament
    /// </summary>
    [HttpDelete("{competitionId:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> DeleteTournament(Guid competitionId)
    {
        _logger.LogInformation("Deleting football tournament with ID: {competitionId}", competitionId);

        DeleteFootballTournamentCommand command = new DeleteFootballTournamentCommand(competitionId);
        Result result = await _mediator.Send(command);

        return HandleVoidResult(result, "Football tournament deleted successfully", "Failed to delete football tournament");
    }

    /// <summary>
    /// Start tournament group stage
    /// </summary>
    [HttpPost("{competitionId:guid}/start-group-stage")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballTournamentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballTournamentDto>>> StartTournamentGroupStage(Guid competitionId)
    {
        _logger.LogInformation("Starting group stage for football tournament with ID: {competitionId}", competitionId);

        StartTournamentGroupStageCommand command = new StartTournamentGroupStageCommand(competitionId);
        Result<FootballTournamentDto> result = await _mediator.Send(command);

        return HandleResult(result, "Tournament group stage started successfully", "Failed to start tournament group stage");
    }

    /// <summary>
    /// Update playoff schedule
    /// </summary>
    [HttpPut("{competitionId:guid}/playoff-schedule")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballTournamentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballTournamentDto>>> UpdatePlayoffSchedule(
        Guid competitionId,
        [FromBody] UpdatePlayoffScheduleRequest request)
    {
        _logger.LogInformation(
            "Updating playoff schedule for tournament {competitionId} with {slotCount} slot(s)",
            competitionId,
            request?.Slots?.Count ?? 0);

        IReadOnlyList<FootballPlayoffScheduleSlotInput> slots =
            MapPlayoffSchedule(request?.Slots) ?? Array.Empty<FootballPlayoffScheduleSlotInput>();

        UpdateTournamentPlayoffScheduleCommand command =
            new UpdateTournamentPlayoffScheduleCommand(competitionId, slots);
        Result<FootballTournamentDto> result = await _mediator.Send(command);

        return HandleResult(result, "Tournament playoff schedule updated successfully", "Failed to update tournament playoff schedule");
    }

    /// <summary>
    /// Start tournament playoff stage
    /// </summary>
    [HttpPost("{competitionId:guid}/start-playoff-stage")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballTournamentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballTournamentDto>>> StartTournamentPlayoffStage(Guid competitionId)
    {
        _logger.LogInformation("Starting playoff stage for football tournament with ID: {competitionId}", competitionId);

        StartTournamentPlayoffStageCommand command = new StartTournamentPlayoffStageCommand(competitionId);
        Result<FootballTournamentDto> result = await _mediator.Send(command);

        return HandleResult(result, "Tournament playoff stage started successfully", "Failed to start tournament playoff stage");
    }

    /// <summary>
    /// Get tournament playoff bracket
    /// </summary>
    [HttpGet("{competitionId:guid}/playoff-bracket")]
    [ProducesResponseType(typeof(ApiResponse<FootballPlayoffBracketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballPlayoffBracketDto>>> GetTournamentPlayoffBracket(Guid competitionId)
    {
        _logger.LogInformation("Getting playoff bracket for tournament: {competitionId}", competitionId);

        GetTournamentPlayoffBracketQuery query = new GetTournamentPlayoffBracketQuery(competitionId);
        Result<FootballPlayoffBracketDto> result = await _mediator.Send(query);

        return HandleResult(result, "Tournament playoff bracket retrieved successfully", "Failed to retrieve tournament playoff bracket");
    }

    /// <summary>
    /// Complete tournament
    /// </summary>
    [HttpPost("{competitionId:guid}/complete")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballTournamentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballTournamentDto>>> CompleteTournament(Guid competitionId)
    {
        _logger.LogInformation("Completing football tournament with ID: {competitionId}", competitionId);

        CompleteTournamentCommand command = new CompleteTournamentCommand(competitionId);
        Result<FootballTournamentDto> result = await _mediator.Send(command);

        return HandleResult(result, "Football tournament completed successfully", "Failed to complete football tournament");
    }

    /// <summary>
    /// Cancel tournament
    /// </summary>
    [HttpPost("{competitionId:guid}/cancel")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballTournamentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballTournamentDto>>> CancelTournament(Guid competitionId)
    {
        _logger.LogInformation("Cancelling football tournament with ID: {competitionId}", competitionId);

        CancelTournamentCommand command = new CancelTournamentCommand(competitionId);
        Result<FootballTournamentDto> result = await _mediator.Send(command);

        return HandleResult(result, "Football tournament cancelled successfully", "Failed to cancel football tournament");
    }

    /// <summary>
    /// Add group to tournament
    /// </summary>
    [HttpPost("{competitionId:guid}/groups")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballTournamentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballTournamentDto>>> AddGroupToTournament(Guid competitionId, [FromBody] AddGroupToTournamentRequest request)
    {
        _logger.LogInformation(
            "Adding group '{groupName}' to football tournament with ID: {competitionId}",
            SanitizeForLog(request.GroupName),
            competitionId);

        AddGroupToTournamentCommand command = new AddGroupToTournamentCommand(competitionId, request.GroupName);
        Result<FootballTournamentDto> result = await _mediator.Send(command);

        return HandleResult(result, "Group added to tournament successfully", "Failed to add group to tournament");
    }

    /// <summary>
    /// Remove group from tournament
    /// </summary>
    [HttpDelete("{competitionId:guid}/groups/{groupId:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballTournamentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballTournamentDto>>> RemoveGroupFromTournament(Guid competitionId, Guid groupId)
    {
        _logger.LogInformation("Removing group {groupId} from football tournament with ID: {competitionId}", groupId, competitionId);

        RemoveGroupFromTournamentCommand command = new RemoveGroupFromTournamentCommand(competitionId, groupId);
        Result<FootballTournamentDto> result = await _mediator.Send(command);

        return HandleResult(result, "Group removed from tournament successfully", "Failed to remove group from tournament");
    }

    /// <summary>
    /// Add team to tournament group
    /// </summary>
    [HttpPost("{competitionId:guid}/groups/{groupId:guid}/teams")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballTournamentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballTournamentDto>>> AddTeamToTournamentGroup(Guid competitionId, Guid groupId, [FromBody] AddTeamToTournamentGroupRequest request)
    {
        _logger.LogInformation("Adding team {teamId} to group {groupId} in tournament {competitionId}", request.TeamId, groupId, competitionId);

        AddTeamToTournamentGroupCommand command = new AddTeamToTournamentGroupCommand(competitionId, groupId, request.TeamId);
        Result<FootballTournamentDto> result = await _mediator.Send(command);

        return HandleResult(result, "Team added to tournament group successfully", "Failed to add team to tournament group");
    }

    /// <summary>
    /// Remove team from tournament group
    /// </summary>
    [HttpDelete("{competitionId:guid}/groups/{groupId:guid}/teams/{teamId:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballTournamentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballTournamentDto>>> RemoveTeamFromTournamentGroup(Guid competitionId, Guid groupId, Guid teamId)
    {
        _logger.LogInformation("Removing team {teamId} from group {groupId} in tournament {competitionId}", teamId, groupId, competitionId);

        RemoveTeamFromTournamentGroupCommand command = new RemoveTeamFromTournamentGroupCommand(competitionId, groupId, teamId);
        Result<FootballTournamentDto> result = await _mediator.Send(command);

        return HandleResult(result, "Team removed from tournament group successfully", "Failed to remove team from tournament group");
    }
}
