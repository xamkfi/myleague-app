using Domain.Constants;
using Application.Common;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Football;

namespace WebAPI.Controllers.Football.Match;

/// <summary>
/// Controller for football match lifecycle operations
/// </summary>
[Route("api/football-matches/{matchId:guid}")]
[Authorize(Roles = AuthRoles.AdminOnly)]
public class FootballMatchLifecycleController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<FootballMatchLifecycleController> _logger;

    /// <summary>
    /// Initializes a new instance of the FootballMatchLifecycleController class
    /// </summary>
    public FootballMatchLifecycleController(
        IMediator mediator,
        ILogger<FootballMatchLifecycleController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Start match
    /// </summary>
    [HttpPut("start")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> StartMatch(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(new StartFootballMatchCommand(matchId), cancellationToken);
        return HandleResult(result, "Football match started successfully", "Failed to start football match");
    }

    /// <summary>
    /// Complete match
    /// </summary>
    [HttpPut("complete")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> CompleteMatch(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(new CompleteFootballMatchCommand(matchId), cancellationToken);
        return HandleResult(result, "Completed football match successfully", "Failed to complete football match");
    }

    /// <summary>
    /// Reopen match
    /// </summary>
    [HttpPut("reopen")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> ReopenMatch(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(new ReopenFootballMatchCommand(matchId), cancellationToken);
        return HandleResult(result, "Reopened football match successfully", "Failed to reopen football match");
    }

    /// <summary>
    /// Postpone
    /// </summary>
    [HttpPost("postpone")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> Postpone(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(new PostponeMatchCommand(matchId), cancellationToken);
        return HandleResult(result, "Match postponed successfully", "Failed to postpone match");
    }

    /// <summary>
    /// Cancel
    /// </summary>
    [HttpPost("cancel")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> Cancel(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(new CancelMatchCommand(matchId), cancellationToken);
        return HandleResult(result, "Match canceled successfully", "Failed to cancel match");
    }

    /// <summary>
    /// Reactivate
    /// </summary>
    [HttpPost("reactivate")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> Reactivate(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(new ReactivateMatchCommand(matchId), cancellationToken);
        return HandleResult(result, "Match reactivated successfully", "Failed to reactivate match");
    }

    /// <summary>
    /// Assign teams
    /// </summary>
    [HttpPut("teams")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> AssignTeams(
        Guid matchId,
        [FromBody] AssignMatchTeamsRequest request,
        CancellationToken cancellationToken)
    {
        AssignMatchTeamsCommand command = new(matchId, request.HomeTeamId, request.AwayTeamId);
        Result<FootballMatchDto> result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result, "Match teams updated successfully", "Failed to update match teams");
    }
}
