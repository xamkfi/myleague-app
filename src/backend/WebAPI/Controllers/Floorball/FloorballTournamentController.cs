using System.Collections.Generic;
using System.Linq;
using Application.Common;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Tournaments.Commands;
using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Tournaments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;
using WebAPI.Models.Floorball;

namespace WebAPI.Controllers.Floorball;

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
    /// Initializes a new instance of the <see cref="FloorballTournamentController"/> class
    /// </summary>
    /// <param name="mediator">Mediator instance for handling commands and queries</param>
    /// <param name="logger">Logger instance for logging</param>
    public FloorballTournamentController(IMediator mediator, ILogger<FloorballTournamentController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets all floorball tournaments, optionally filtered by status
    /// </summary>
    /// <param name="status">Optional status filter (e.g. Draft, Active, InProgress, Completed, Cancelled)</param>
    /// <returns>List of floorball tournaments</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<FloorballTournamentSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<FloorballTournamentSummaryDto>>>> GetAllTournaments([FromQuery] string? status = null)
    {
        _logger.LogInformation("Getting all floorball tournaments with status filter: {status}", status);

        GetAllFloorballTournamentsQuery query = new(status);
        Result<IReadOnlyCollection<FloorballTournamentSummaryDto>> result = await _mediator.Send(query);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<List<FloorballTournamentSummaryDto>>.SuccessResponse(
                result.Data.ToList(), "Floorball tournaments retrieved successfully"));
        }

        string errorMessage = result.Error ?? "Failed to retrieve floorball tournaments";
        return BadRequest(ApiResponse<List<FloorballTournamentSummaryDto>>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Gets a floorball tournament by ID
    /// </summary>
    /// <param name="id">Tournament ID</param>
    /// <returns>Tournament details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FloorballTournamentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FloorballTournamentDto>>> GetTournamentById(Guid id)
    {
        _logger.LogInformation("Getting floorball tournament with ID: {id}", id);

        GetFloorballTournamentByIdQuery query = new(id);
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
        _logger.LogInformation("Creating floorball tournament: {name}", request.Name);

        if (!DateTime.TryParse(request.StartDate, out DateTime startDate) || !DateTime.TryParse(request.EndDate, out DateTime endDate))
        {
            return BadRequest(ApiResponse<FloorballTournamentDto>.ErrorResponse("Invalid date format"));
        }

        CreateFloorballTournamentCommand command = new(
            request.Name,
            startDate,
            endDate,
            request.Location,
            request.DescriptionHtml,
            request.NumberOfPeriods,
            request.PeriodDurationMinutes,
            request.AllowOvertime,
            request.OvertimeDurationMinutes,
            request.AllowShootout,
            request.PlayoffFormat,
            request.GroupStageAdvancingCount
        );

        Result<FloorballTournamentDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetTournamentById),
                new { id = result.Data.Id },
                ApiResponse<FloorballTournamentDto>.SuccessResponse(result.Data, "Floorball tournament created successfully")
            );
        }

        string errorMessage = result.Error ?? "Failed to create floorball tournament";
        List<string> errorList = result.ValidationFailures.Select(x => x.ErrorMessage).ToList();
        return BadRequest(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage, errorList));
    }

    /// <summary>
    /// Updates an existing floorball tournament
    /// </summary>
    /// <param name="id">Tournament ID</param>
    /// <param name="request">Update tournament request</param>
    /// <returns>Updated tournament details</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<FloorballTournamentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FloorballTournamentDto>>> UpdateTournament(Guid id, [FromBody] UpdateFloorballTournamentRequest request)
    {
        _logger.LogInformation("Updating floorball tournament with ID: {id}", id);

        if (!DateTime.TryParse(request.StartDate, out DateTime startDate) || !DateTime.TryParse(request.EndDate, out DateTime endDate))
        {
            return BadRequest(ApiResponse<FloorballTournamentDto>.ErrorResponse("Invalid date format"));
        }

        UpdateFloorballTournamentCommand command = new(
            id,
            request.Name,
            startDate,
            endDate,
            request.Location,
            request.DescriptionHtml,
            request.NumberOfPeriods,
            request.PeriodDurationMinutes,
            request.AllowOvertime,
            request.OvertimeDurationMinutes,
            request.AllowShootout,
            request.PlayoffFormat,
            request.GroupStageAdvancingCount
        );

        Result<FloorballTournamentDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<FloorballTournamentDto>.SuccessResponse(result.Data, "Floorball tournament updated successfully"));
        }

        string errorMessage = result.Error ?? "Failed to update floorball tournament";
        List<string> errorList = result.ValidationFailures.Select(x => x.ErrorMessage).ToList();

        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage));
        }

        return BadRequest(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage, errorList));
    }

    /// <summary>
    /// Deletes a floorball tournament
    /// </summary>
    /// <param name="id">Tournament ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> DeleteTournament(Guid id)
    {
        _logger.LogInformation("Deleting floorball tournament with ID: {id}", id);

        DeleteFloorballTournamentCommand command = new(id);
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
    /// Changes the status of a floorball tournament
    /// </summary>
    /// <param name="id">Tournament ID</param>
    /// <param name="request">Status change request with action (e.g. activate, start, complete, cancel)</param>
    /// <returns>Updated tournament details</returns>
    [HttpPut("{id:guid}/status")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<FloorballTournamentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FloorballTournamentDto>>> ChangeTournamentStatus(Guid id, [FromBody] ChangeFloorballTournamentStatusRequest request)
    {
        _logger.LogInformation("Changing status for floorball tournament {id} with action: {action}", id, request.Action);

        ChangeFloorballTournamentStatusCommand command = new(id, request.Action);
        Result<FloorballTournamentDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<FloorballTournamentDto>.SuccessResponse(result.Data, "Floorball tournament status changed successfully"));
        }

        string errorMessage = result.Error ?? "Failed to change tournament status";
        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage));
        }

        return BadRequest(ApiResponse<FloorballTournamentDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Adds a group to a floorball tournament
    /// </summary>
    /// <param name="id">Tournament ID</param>
    /// <param name="request">Add group request</param>
    /// <returns>Created group details</returns>
    [HttpPost("{id:guid}/groups")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<FloorballTournamentGroupDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FloorballTournamentGroupDto>>> AddGroupToTournament(Guid id, [FromBody] AddGroupToTournamentRequest request)
    {
        _logger.LogInformation("Adding group '{name}' to tournament {id}", request.Name, id);

        AddGroupToTournamentCommand command = new(id, request.Name, request.Phase, request.SortOrder);
        Result<FloorballTournamentGroupDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetTournamentById),
                new { id },
                ApiResponse<FloorballTournamentGroupDto>.SuccessResponse(result.Data, "Group added to tournament successfully")
            );
        }

        string errorMessage = result.Error ?? "Failed to add group to tournament";
        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse<FloorballTournamentGroupDto>.ErrorResponse(errorMessage));
        }

        return BadRequest(ApiResponse<FloorballTournamentGroupDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Removes a group from a floorball tournament
    /// </summary>
    /// <param name="id">Tournament ID</param>
    /// <param name="groupId">Group ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id:guid}/groups/{groupId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> RemoveGroupFromTournament(Guid id, Guid groupId)
    {
        _logger.LogInformation("Removing group {groupId} from tournament {id}", groupId, id);

        RemoveGroupFromTournamentCommand command = new(id, groupId);
        Result result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse.SuccessResponse("Group removed from tournament successfully"));
        }

        string errorMessage = result.Error ?? "Failed to remove group from tournament";
        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse.ErrorResponse(errorMessage));
        }

        return BadRequest(ApiResponse.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Adds a team to a group in a floorball tournament
    /// </summary>
    /// <param name="id">Tournament ID</param>
    /// <param name="groupId">Group ID</param>
    /// <param name="teamId">Team ID</param>
    /// <returns>Created group-team association</returns>
    [HttpPost("{id:guid}/groups/{groupId:guid}/teams/{teamId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<FloorballTournamentGroupTeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FloorballTournamentGroupTeamDto>>> AddTeamToGroup(Guid id, Guid groupId, Guid teamId)
    {
        _logger.LogInformation("Adding team {teamId} to group {groupId} in tournament {id}", teamId, groupId, id);

        AddTeamToTournamentGroupCommand command = new(id, groupId, teamId);
        Result<FloorballTournamentGroupTeamDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<FloorballTournamentGroupTeamDto>.SuccessResponse(result.Data, "Team added to group successfully"));
        }

        string errorMessage = result.Error ?? "Failed to add team to group";
        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse<FloorballTournamentGroupTeamDto>.ErrorResponse(errorMessage));
        }

        return BadRequest(ApiResponse<FloorballTournamentGroupTeamDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Removes a team from a group in a floorball tournament
    /// </summary>
    /// <param name="id">Tournament ID</param>
    /// <param name="groupId">Group ID</param>
    /// <param name="teamId">Team ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id:guid}/groups/{groupId:guid}/teams/{teamId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> RemoveTeamFromGroup(Guid id, Guid groupId, Guid teamId)
    {
        _logger.LogInformation("Removing team {teamId} from group {groupId} in tournament {id}", teamId, groupId, id);

        RemoveTeamFromTournamentGroupCommand command = new(id, groupId, teamId);
        Result result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse.SuccessResponse("Team removed from group successfully"));
        }

        string errorMessage = result.Error ?? "Failed to remove team from group";
        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse.ErrorResponse(errorMessage));
        }

        return BadRequest(ApiResponse.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Gets the standings for a group in a floorball tournament
    /// </summary>
    /// <param name="id">Tournament ID</param>
    /// <param name="groupId">Group ID</param>
    /// <returns>Group standings with ranked teams</returns>
    [HttpGet("{id:guid}/groups/{groupId:guid}/standings")]
    [ProducesResponseType(typeof(ApiResponse<FloorballTournamentGroupStandingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FloorballTournamentGroupStandingsDto>>> GetGroupStandings(Guid id, Guid groupId)
    {
        _logger.LogInformation("Getting standings for group {groupId} in tournament {id}", groupId, id);

        GetTournamentGroupStandingsQuery query = new(id, groupId);
        Result<FloorballTournamentGroupStandingsDto> result = await _mediator.Send(query);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<FloorballTournamentGroupStandingsDto>.SuccessResponse(result.Data, "Group standings retrieved successfully"));
        }

        string errorMessage = result.Error ?? "Failed to retrieve group standings";
        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse<FloorballTournamentGroupStandingsDto>.ErrorResponse(errorMessage));
        }

        return BadRequest(ApiResponse<FloorballTournamentGroupStandingsDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Creates a match within a floorball tournament
    /// </summary>
    /// <param name="id">Tournament ID</param>
    /// <param name="request">Create tournament match request</param>
    /// <returns>Created match details</returns>
    [HttpPost("{id:guid}/matches")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> CreateTournamentMatch(Guid id, [FromBody] CreateTournamentMatchRequest request)
    {
        _logger.LogInformation("Creating match in tournament {id}", id);

        if (!DateTime.TryParse(request.ScheduledDateTime, out DateTime scheduledDateTime))
        {
            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse("Invalid date format for ScheduledDateTime"));
        }

        CreateFloorballTournamentMatchCommand command = new(
            id,
            request.HomeTeamId,
            request.AwayTeamId,
            scheduledDateTime,
            request.Venue,
            request.GroupId,
            request.TournamentRound,
            request.RefereeId
        );

        Result<FloorballMatchDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetTournamentById),
                new { id },
                ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Tournament match created successfully")
            );
        }

        string errorMessage = result.Error ?? "Failed to create tournament match";
        List<string> errorList = result.ValidationFailures.Select(x => x.ErrorMessage).ToList();

        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage, errorList));
    }
}
