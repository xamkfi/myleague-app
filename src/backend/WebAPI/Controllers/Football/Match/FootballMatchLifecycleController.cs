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

[Route("api/football-matches/{matchId:guid}")]
[Authorize]
public class FootballMatchLifecycleController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<FootballMatchLifecycleController> _logger;

    public FootballMatchLifecycleController(
        IMediator mediator,
        ILogger<FootballMatchLifecycleController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPut("start")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> StartMatch(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(new StartFootballMatchCommand(matchId), cancellationToken);
        return HandleResult(result, "Football match started successfully", "Failed to start football match");
    }

    [HttpPut("complete")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> CompleteMatch(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(new CompleteFootballMatchCommand(matchId), cancellationToken);
        return HandleResult(result, "Completed football match successfully", "Failed to complete football match");
    }

    [HttpPut("reopen")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> ReopenMatch(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(new ReopenFootballMatchCommand(matchId), cancellationToken);
        return HandleResult(result, "Reopened football match successfully", "Failed to reopen football match");
    }

    [HttpPost("postpone")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> Postpone(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(new PostponeMatchCommand(matchId), cancellationToken);
        return HandleResult(result, "Match postponed successfully", "Failed to postpone match");
    }

    [HttpPost("cancel")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> Cancel(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(new CancelMatchCommand(matchId), cancellationToken);
        return HandleResult(result, "Match canceled successfully", "Failed to cancel match");
    }

    [HttpPost("reactivate")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> Reactivate(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(new ReactivateMatchCommand(matchId), cancellationToken);
        return HandleResult(result, "Match reactivated successfully", "Failed to reactivate match");
    }

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
