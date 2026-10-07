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
/// Tracks which hockey players are on the ice.
/// Every hockey match controller shares the <c>api/HockeyMatch</c> route prefix.
/// </summary>
[Route("api/HockeyMatch")]
public class HockeyMatchOnIceController : BaseApiController
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Creates a new <see cref="HockeyMatchOnIceController"/>.
    /// </summary>
    public HockeyMatchOnIceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Enables on-ice tracking for a match side.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/on-ice/enable")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> EnableOnIce(
        Guid matchId,
        [FromBody] HockeyMatchTeamIdRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new EnableHockeyMatchOnIceTrackingCommand(matchId, request.MatchTeamId, request.UserId), cancellationToken);
        return HandleResult(result, "On-ice tracking enabled successfully", "Failed to enable on-ice tracking");
    }

    /// <summary>
    /// Disables on-ice tracking for a match side.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/on-ice/disable")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> DisableOnIce(
        Guid matchId,
        [FromBody] HockeyMatchTeamIdRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new DisableHockeyMatchOnIceTrackingCommand(matchId, request.MatchTeamId, request.UserId), cancellationToken);
        return HandleResult(result, "On-ice tracking disabled successfully", "Failed to disable on-ice tracking");
    }

    /// <summary>
    /// Puts a player on the ice.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/on-ice/players")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> AddPlayerToIce(
        Guid matchId,
        [FromBody] AddHockeyMatchPlayerToIceRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new AddHockeyMatchPlayerToIceCommand(
            matchId,
            request.MatchTeamId,
            request.MatchActivePlayerId,
            request.Slot,
            request.Order,
            request.IsGoalie,
            request.IsExtraAttacker,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.UserId), cancellationToken);
        return HandleResult(result, "Player added to ice successfully", "Failed to add player to ice");
    }

    /// <summary>
    /// Removes a player from the ice.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpDelete("{matchId:guid}/on-ice/players/{matchActivePlayerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RemovePlayerFromIce(
        Guid matchId,
        Guid matchActivePlayerId,
        [FromQuery] Guid matchTeamId,
        [FromQuery] int? periodNumber = null,
        [FromQuery] int? timeInSeconds = null,
        [FromQuery] Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new RemoveHockeyMatchPlayerFromIceCommand(
            matchId,
            matchTeamId,
            matchActivePlayerId,
            periodNumber,
            timeInSeconds,
            userId), cancellationToken);
        return HandleResult(result, "Player removed from ice successfully", "Failed to remove player from ice");
    }

    /// <summary>
    /// Clears all players from the ice.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/on-ice/clear")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> ClearIce(
        Guid matchId,
        [FromBody] HockeyMatchIceActionRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new ClearHockeyMatchIceCommand(
            matchId,
            request.MatchTeamId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.UserId), cancellationToken);
        return HandleResult(result, "Ice cleared successfully", "Failed to clear ice");
    }

    /// <summary>
    /// Applies a match line onto the ice.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/on-ice/apply-line/{matchLineId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> ApplyLineToIce(
        Guid matchId,
        Guid matchLineId,
        [FromBody] HockeyMatchIceActionRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new ApplyHockeyMatchLineToIceCommand(
            matchId,
            request.MatchTeamId,
            matchLineId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.UserId), cancellationToken);
        return HandleResult(result, "Line applied to ice successfully", "Failed to apply line to ice");
    }
}
