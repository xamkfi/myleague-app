using Application.Common;
using Application.Features.Hockey.Teams.Commands;
using Application.Features.Hockey.Teams.DTOs;
using Application.Features.Hockey.Teams.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Hockey;

namespace WebAPI.Controllers.Hockey;

/// <summary>
/// API endpoints for hockey teams.
/// </summary>
[Route("api/[controller]")]
public class HockeyTeamController : BaseApiController
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Creates a new <see cref="HockeyTeamController"/>.
    /// </summary>
    public HockeyTeamController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets all hockey teams.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<HockeyTeamDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HockeyTeamDto>>>> GetAllTeams()
    {
        Result<IEnumerable<HockeyTeamDto>> result = await _mediator.Send(new GetAllHockeyTeamsQuery());
        return HandleListResult(result, "Hockey teams retrieved successfully", "Failed to retrieve hockey teams");
    }

    /// <summary>
    /// Gets hockey teams for a club.
    /// </summary>
    [HttpGet("club/{clubId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<HockeyTeamDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HockeyTeamDto>>>> GetTeamsByClub(Guid clubId)
    {
        Result<IEnumerable<HockeyTeamDto>> result = await _mediator.Send(new GetHockeyTeamsByClubQuery(clubId));
        return HandleListResult(result, "Hockey teams retrieved successfully", "Failed to retrieve hockey teams");
    }

    /// <summary>
    /// Gets a hockey team by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyTeamDto>>> GetTeamById(Guid id)
    {
        Result<HockeyTeamDto> result = await _mediator.Send(new GetHockeyTeamByIdQuery(id));
        return HandleResult(result, "Hockey team retrieved successfully", "Hockey team not found");
    }

    /// <summary>
    /// Creates a new hockey team.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HockeyTeamDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HockeyTeamDto>>> CreateTeam([FromBody] CreateHockeyTeamRequest request)
    {
        CreateHockeyTeamCommand command = new(
            request.Name,
            request.ClubId,
            request.TeamCategory,
            request.DivisionId,
            request.HomeArena,
            request.PrimaryJerseyColor,
            request.SecondaryJerseyColor,
            request.ShortName);

        Result<HockeyTeamDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data is not null)
        {
            return CreatedAtAction(
                nameof(GetTeamById),
                new { id = result.Data.Id },
                ApiResponse<HockeyTeamDto>.SuccessResponse(result.Data, "Hockey team created successfully"));
        }

        return HandleResult(result, "Hockey team created successfully", "Failed to create hockey team");
    }

    /// <summary>
    /// Updates hockey team details.
    /// </summary>
    [HttpPut("{teamId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTeamDto>>> UpdateTeam(
        Guid teamId,
        [FromBody] UpdateHockeyTeamRequest request)
    {
        Result<HockeyTeamDto> result = await _mediator.Send(new UpdateHockeyTeamCommand(
            teamId,
            request.Name,
            request.ShortName,
            request.TeamCategory,
            request.DivisionId,
            request.HomeArena,
            request.PrimaryJerseyColor,
            request.SecondaryJerseyColor));

        return HandleResult(result, "Hockey team updated successfully", "Failed to update hockey team");
    }

    /// <summary>
    /// Sets whether the hockey team is active.
    /// </summary>
    [HttpPut("{teamId:guid}/active")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTeamDto>>> SetActiveStatus(
        Guid teamId,
        [FromBody] SetHockeyTeamActiveStatusRequest request) =>
        HandleResult(
            await _mediator.Send(new SetHockeyTeamActiveStatusCommand(teamId, request.IsActive)),
            "Hockey team active status updated successfully",
            "Failed to update hockey team active status");

    /// <summary>
    /// Updates the hockey team logo URL.
    /// </summary>
    [HttpPut("{teamId:guid}/logo")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTeamDto>>> UpdateLogo(
        Guid teamId,
        [FromBody] UpdateHockeyTeamLogoRequest request) =>
        HandleResult(
            await _mediator.Send(new UpdateHockeyTeamLogoCommand(teamId, request.LogoUrl)),
            "Hockey team logo updated successfully",
            "Failed to update hockey team logo");

    /// <summary>
    /// Adds a player to the team roster.
    /// </summary>
    [HttpPost("{teamId:guid}/players")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTeamDto>>> AddPlayer(
        Guid teamId,
        [FromBody] AddPlayerToHockeyTeamRequest request) =>
        HandleResult(
            await _mediator.Send(new AddPlayerToHockeyTeamCommand(
                teamId,
                request.PlayerId,
                request.Position,
                request.CompetitionId,
                request.JerseyNumber,
                request.RequestedJerseyNumber,
                request.RosterStatus)),
            "Player added to hockey team successfully",
            "Failed to add player to hockey team");

    /// <summary>
    /// Removes a player from the team roster.
    /// </summary>
    [HttpDelete("{teamId:guid}/players/{playerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTeamDto>>> RemovePlayer(
        Guid teamId,
        Guid playerId,
        [FromQuery] Guid? competitionId = null) =>
        HandleResult(
            await _mediator.Send(new RemovePlayerFromHockeyTeamCommand(teamId, playerId, competitionId)),
            "Player removed from hockey team successfully",
            "Failed to remove player from hockey team");

    /// <summary>
    /// Updates a roster membership.
    /// </summary>
    [HttpPut("{teamId:guid}/players/{playerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTeamDto>>> UpdatePlayer(
        Guid teamId,
        Guid playerId,
        [FromBody] UpdateHockeyTeamPlayerRequest request) =>
        HandleResult(
            await _mediator.Send(new UpdateHockeyTeamPlayerCommand(
                teamId,
                playerId,
                request.Position,
                request.JerseyNumber,
                request.RosterStatus,
                request.CaptainRole,
                request.CompetitionId)),
            "Hockey team player updated successfully",
            "Failed to update hockey team player");

    /// <summary>
    /// Adds a line to the team.
    /// </summary>
    [HttpPost("{teamId:guid}/lines")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTeamDto>>> AddLine(
        Guid teamId,
        [FromBody] AddHockeyLineRequest request) =>
        HandleResult(
            await _mediator.Send(new AddHockeyLineCommand(
                teamId,
                request.Name,
                request.LineNumber,
                request.LineType,
                request.CompetitionId)),
            "Hockey line added successfully",
            "Failed to add hockey line");

    /// <summary>
    /// Deactivates a line on the team.
    /// </summary>
    [HttpDelete("{teamId:guid}/lines/{lineId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTeamDto>>> RemoveLine(Guid teamId, Guid lineId) =>
        HandleResult(
            await _mediator.Send(new RemoveHockeyLineCommand(teamId, lineId)),
            "Hockey line removed successfully",
            "Failed to remove hockey line");

    /// <summary>
    /// Places a team player onto a line.
    /// </summary>
    [HttpPost("{teamId:guid}/lines/{lineId:guid}/players")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTeamDto>>> AddPlayerToLine(
        Guid teamId,
        Guid lineId,
        [FromBody] AddPlayerToHockeyLineRequest request) =>
        HandleResult(
            await _mediator.Send(new AddPlayerToHockeyLineCommand(
                teamId,
                lineId,
                request.TeamPlayerId,
                request.Slot,
                request.Order)),
            "Player added to hockey line successfully",
            "Failed to add player to hockey line");

    /// <summary>
    /// Removes a team player from a line.
    /// </summary>
    [HttpDelete("{teamId:guid}/lines/{lineId:guid}/players/{teamPlayerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTeamDto>>> RemovePlayerFromLine(
        Guid teamId,
        Guid lineId,
        Guid teamPlayerId) =>
        HandleResult(
            await _mediator.Send(new RemovePlayerFromHockeyLineCommand(teamId, lineId, teamPlayerId)),
            "Player removed from hockey line successfully",
            "Failed to remove player from hockey line");

    /// <summary>
    /// Adds staff to the team.
    /// </summary>
    [HttpPost("{teamId:guid}/staff")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTeamDto>>> AddStaff(
        Guid teamId,
        [FromBody] AddHockeyTeamStaffRequest request) =>
        HandleResult(
            await _mediator.Send(new AddHockeyTeamStaffCommand(
                teamId,
                request.PersonId,
                request.Role,
                request.CompetitionId)),
            "Staff added to hockey team successfully",
            "Failed to add staff to hockey team");

    /// <summary>
    /// Removes staff from the team.
    /// </summary>
    [HttpDelete("{teamId:guid}/staff/{staffId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTeamDto>>> RemoveStaff(Guid teamId, Guid staffId) =>
        HandleResult(
            await _mediator.Send(new RemoveHockeyTeamStaffCommand(teamId, staffId)),
            "Staff removed from hockey team successfully",
            "Failed to remove staff from hockey team");
}
