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
/// Hockey match lifecycle: start, finish, status, result type, period, overtime / shootout flags, team goal corrections and period scores.
/// Every hockey match controller shares the <c>api/HockeyMatch</c> route prefix.
/// </summary>
[Route("api/HockeyMatch")]
public class HockeyMatchLifecycleController : BaseApiController
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Creates a new <see cref="HockeyMatchLifecycleController"/>.
    /// </summary>
    public HockeyMatchLifecycleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Marks a hockey match as started.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/start")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> Start(
        Guid matchId,
        [FromBody] MarkHockeyMatchStartedRequest? request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new MarkHockeyMatchStartedCommand(matchId, request?.ActualStartTime), cancellationToken);
        return HandleResult(result, "Hockey match started successfully", "Failed to start hockey match");
    }

    /// <summary>
    /// Marks a hockey match as finished.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/finish")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> Finish(
        Guid matchId,
        [FromBody] MarkHockeyMatchFinishedRequest? request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new MarkHockeyMatchFinishedCommand(matchId, request?.ActualEndTime, request?.ResultType), cancellationToken);
        return HandleResult(result, "Hockey match finished successfully", "Failed to finish hockey match");
    }

    /// <summary>
    /// Sets match status.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPatch("{matchId:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> SetStatus(
        Guid matchId,
        [FromBody] SetHockeyMatchStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new SetHockeyMatchStatusCommand(matchId, request.Status), cancellationToken);
        return HandleResult(result, "Hockey match status updated successfully", "Failed to update status");
    }

    /// <summary>
    /// Sets match result type.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPatch("{matchId:guid}/result")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> SetResultType(
        Guid matchId,
        [FromBody] SetHockeyMatchResultTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new SetHockeyMatchResultTypeCommand(matchId, request.ResultType), cancellationToken);
        return HandleResult(result, "Hockey match result type updated successfully", "Failed to update result type");
    }

    /// <summary>
    /// Sets the current period number.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPatch("{matchId:guid}/period")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> SetCurrentPeriod(
        Guid matchId,
        [FromBody] SetHockeyMatchCurrentPeriodRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new SetHockeyMatchCurrentPeriodCommand(matchId, request.PeriodNumber), cancellationToken);
        return HandleResult(result, "Hockey match period updated successfully", "Failed to update period");
    }

    /// <summary>
    /// Sets whether the match went to overtime.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPatch("{matchId:guid}/went-to-overtime")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> SetWentToOvertime(
        Guid matchId,
        [FromBody] SetHockeyMatchBooleanFlagRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new SetHockeyMatchWentToOvertimeCommand(matchId, request.Value), cancellationToken);
        return HandleResult(result, "Hockey match overtime flag updated successfully", "Failed to update overtime flag");
    }

    /// <summary>
    /// Sets whether the match went to shootout.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPatch("{matchId:guid}/went-to-shootout")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> SetWentToShootout(
        Guid matchId,
        [FromBody] SetHockeyMatchBooleanFlagRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new SetHockeyMatchWentToShootoutCommand(matchId, request.Value), cancellationToken);
        return HandleResult(result, "Hockey match shootout flag updated successfully", "Failed to update shootout flag");
    }

    /// <summary>
    /// Corrects goals for one match side.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPut("{matchId:guid}/team-goals")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> SetTeamGoals(
        Guid matchId,
        [FromBody] SetHockeyMatchTeamGoalsRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new SetHockeyMatchTeamGoalsCommand(matchId, request.TeamSlot, request.Goals), cancellationToken);
        return HandleResult(result, "Hockey match team goals updated successfully", "Failed to update team goals");
    }

    /// <summary>
    /// Creates a period score row.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/period-scores")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> AddPeriodScore(
        Guid matchId,
        [FromBody] AddHockeyPeriodScoreRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new AddHockeyPeriodScoreCommand(
            matchId,
            request.PeriodNumber,
            request.PeriodType), cancellationToken);
        return HandleResult(result, "Period score added successfully", "Failed to add period score");
    }
}
