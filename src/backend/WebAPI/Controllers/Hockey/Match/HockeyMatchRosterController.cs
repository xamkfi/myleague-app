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
/// Hockey match-side rosters: roster confirmation, player deactivation and the active goalie.
/// Every hockey match controller shares the <c>api/HockeyMatch</c> route prefix.
/// </summary>
[Route("api/HockeyMatch")]
public class HockeyMatchRosterController : BaseApiController
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Creates a new <see cref="HockeyMatchRosterController"/>.
    /// </summary>
    public HockeyMatchRosterController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Sets and confirms the roster for one match side.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/roster/confirm")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> ConfirmRoster(
        Guid matchId,
        [FromBody] ConfirmHockeyMatchRosterRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new ConfirmHockeyMatchRosterCommand(
            matchId,
            request.MatchTeamId,
            request.TeamPlayerIds,
            request.ConfirmedByUserId,
            request.Source), cancellationToken);

        return HandleResult(result, "Hockey match roster confirmed successfully", "Failed to confirm roster");
    }

    /// <summary>
    /// Deactivates a dressed roster player.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/roster/deactivate-player")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> DeactivateRosterPlayer(
        Guid matchId,
        [FromBody] HockeyMatchTeamPlayerRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new DeactivateHockeyMatchRosterPlayerCommand(
            matchId,
            request.MatchTeamId,
            request.MatchActivePlayerId), cancellationToken);
        return HandleResult(result, "Roster player deactivated successfully", "Failed to deactivate roster player");
    }

    /// <summary>
    /// Sets the active goalie for a match side.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPut("{matchId:guid}/active-goalie")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> SetActiveGoalie(
        Guid matchId,
        [FromBody] HockeyMatchTeamPlayerRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new SetHockeyMatchActiveGoalieCommand(
            matchId,
            request.MatchTeamId,
            request.MatchActivePlayerId), cancellationToken);
        return HandleResult(result, "Active goalie set successfully", "Failed to set active goalie");
    }

    /// <summary>
    /// Clears the active goalie for a match side.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpDelete("{matchId:guid}/active-goalie")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> ClearActiveGoalie(
        Guid matchId,
        [FromQuery] Guid matchTeamId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new ClearHockeyMatchActiveGoalieCommand(matchId, matchTeamId), cancellationToken);
        return HandleResult(result, "Active goalie cleared successfully", "Failed to clear active goalie");
    }
}
