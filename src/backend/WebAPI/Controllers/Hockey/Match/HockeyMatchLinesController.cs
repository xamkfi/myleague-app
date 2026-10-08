using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Queries;
using Domain.Common;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using WebAPI.Models.Hockey;

namespace WebAPI.Controllers.Hockey;

/// <summary>
/// Manages hockey match lines and the players on them.
/// Every hockey match controller shares the <c>api/HockeyMatch</c> route prefix.
/// </summary>
[Route("api/HockeyMatch")]
public class HockeyMatchLinesController : BaseApiController
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Creates a new <see cref="HockeyMatchLinesController"/>.
    /// </summary>
    public HockeyMatchLinesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Adds a match line to one side.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/lines")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> AddLine(
        Guid matchId,
        [FromBody] AddHockeyMatchLineRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new AddHockeyMatchLineCommand(
            matchId,
            request.MatchTeamId,
            request.Name,
            request.LineType,
            request.LineNumber,
            request.Notes), cancellationToken);
        return HandleResult(result, "Match line added successfully", "Failed to add match line");
    }

    /// <summary>
    /// Removes a match line.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpDelete("{matchId:guid}/lines/{matchLineId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RemoveLine(
        Guid matchId,
        Guid matchLineId,
        [FromQuery] Guid matchTeamId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new RemoveHockeyMatchLineCommand(matchId, matchTeamId, matchLineId), cancellationToken);
        return HandleResult(result, "Match line removed successfully", "Failed to remove match line");
    }

    /// <summary>
    /// Adds a player to a match line.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/lines/{matchLineId:guid}/players")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> AddLinePlayer(
        Guid matchId,
        Guid matchLineId,
        [FromBody] AddHockeyMatchLinePlayerRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new AddHockeyMatchLinePlayerCommand(
            matchId,
            request.MatchTeamId,
            matchLineId,
            request.MatchActivePlayerId,
            request.Slot,
            request.Order), cancellationToken);
        return HandleResult(result, "Line player added successfully", "Failed to add line player");
    }

    /// <summary>
    /// Removes a player from a match line.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpDelete("{matchId:guid}/lines/{matchLineId:guid}/players/{matchActivePlayerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RemoveLinePlayer(
        Guid matchId,
        Guid matchLineId,
        Guid matchActivePlayerId,
        [FromQuery] Guid matchTeamId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new RemoveHockeyMatchLinePlayerCommand(
            matchId,
            matchTeamId,
            matchLineId,
            matchActivePlayerId), cancellationToken);
        return HandleResult(result, "Line player removed successfully", "Failed to remove line player");
    }

    /// <summary>
    /// Updates a match line name.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPut("{matchId:guid}/lines/{matchLineId:guid}/name")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> UpdateLineName(
        Guid matchId,
        Guid matchLineId,
        [FromBody] UpdateHockeyMatchLineNameRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new UpdateHockeyMatchLineNameCommand(
            matchId,
            request.MatchTeamId,
            matchLineId,
            request.Name), cancellationToken);
        return HandleResult(result, "Match line name updated successfully", "Failed to update line name");
    }

    /// <summary>
    /// Updates match line notes.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPut("{matchId:guid}/lines/{matchLineId:guid}/notes")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> UpdateLineNotes(
        Guid matchId,
        Guid matchLineId,
        [FromBody] UpdateHockeyMatchLineNotesRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new UpdateHockeyMatchLineNotesCommand(
            matchId,
            request.MatchTeamId,
            matchLineId,
            request.Notes), cancellationToken);
        return HandleResult(result, "Match line notes updated successfully", "Failed to update line notes");
    }

    /// <summary>
    /// Locks a match line.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/lines/{matchLineId:guid}/lock")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> LockLine(
        Guid matchId,
        Guid matchLineId,
        [FromQuery] Guid matchTeamId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new LockHockeyMatchLineCommand(matchId, matchTeamId, matchLineId), cancellationToken);
        return HandleResult(result, "Match line locked successfully", "Failed to lock match line");
    }

    /// <summary>
    /// Unlocks a match line.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/lines/{matchLineId:guid}/unlock")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> UnlockLine(
        Guid matchId,
        Guid matchLineId,
        [FromQuery] Guid matchTeamId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new UnlockHockeyMatchLineCommand(matchId, matchTeamId, matchLineId), cancellationToken);
        return HandleResult(result, "Match line unlocked successfully", "Failed to unlock match line");
    }

    /// <summary>
    /// Deactivates a match line.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/lines/{matchLineId:guid}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> DeactivateLine(
        Guid matchId,
        Guid matchLineId,
        [FromQuery] Guid matchTeamId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new DeactivateHockeyMatchLineCommand(matchId, matchTeamId, matchLineId), cancellationToken);
        return HandleResult(result, "Match line deactivated successfully", "Failed to deactivate match line");
    }
}
