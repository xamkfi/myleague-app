using Application.Common;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Football;
using WebAPI.Services;

namespace WebAPI.Controllers.Football;

[Route("api/football-matches/{matchId:guid}/events")]
[Authorize]
public class FootballMatchEventsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<FootballMatchEventsController> _logger;
    private readonly IMatchEventRateLimiter _rateLimiter;

    public FootballMatchEventsController(
        IMediator mediator,
        ILogger<FootballMatchEventsController> logger,
        IMatchEventRateLimiter rateLimiter)
    {
        _mediator = mediator;
        _logger = logger;
        _rateLimiter = rateLimiter;
    }

    [HttpPost("goal")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> RecordGoal(
        Guid matchId,
        [FromBody] RecordGoalRequest request,
        CancellationToken cancellationToken)
    {
        string rateKey = $"{matchId}:goal:{request.ScoringTeamId}:{request.ScoringPlayerId}";
        if (_rateLimiter.IsRateLimited(rateKey, MatchEventRateLimits.GoalWindow))
        {
            return StatusCode(
                StatusCodes.Status429TooManyRequests,
                ApiResponse<FootballMatchDto>.ErrorResponse("Too many goal events; please wait a moment."));
        }

        RecordGoalCommand command = new(
            matchId,
            request.ScoringTeamId,
            request.ScoringPlayerId,
            request.AssistingPlayerId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.Description,
            request.GoalType);

        Result<FootballMatchDto> result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result, "Goal recorded successfully", "Failed to record goal");
    }

    [HttpPost("card")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> RecordCard(
        Guid matchId,
        [FromBody] RecordCardEventRequest request,
        CancellationToken cancellationToken)
    {
        string rateKey = $"{matchId}:card:{request.TeamId}:{request.PlayerId}";
        if (_rateLimiter.IsRateLimited(rateKey, MatchEventRateLimits.PenaltyWindow))
        {
            return StatusCode(
                StatusCodes.Status429TooManyRequests,
                ApiResponse<FootballMatchDto>.ErrorResponse("Too many card events; please wait a moment."));
        }

        RecordCardCommand command = new(
            matchId,
            request.TeamId,
            request.PlayerId,
            request.CardType,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.Description);

        Result<FootballMatchDto> result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result, "Card recorded successfully", "Failed to record card");
    }

    [HttpPost("substitution")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> RecordSubstitution(
        Guid matchId,
        [FromBody] RecordSubstitutionEventRequest request,
        CancellationToken cancellationToken)
    {
        RecordSubstitutionCommand command = new(
            matchId,
            request.TeamId,
            request.PlayerOffId,
            request.PlayerOnId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.Description);

        Result<FootballMatchDto> result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result, "Substitution recorded successfully", "Failed to record substitution");
    }

    [HttpDelete("goal/{goalEventId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> DeleteGoal(
        Guid matchId,
        Guid goalEventId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(new DeleteGoalCommand(matchId, goalEventId), cancellationToken);
        return HandleResult(result, "Goal deleted successfully", "Failed to delete goal");
    }

    [HttpDelete("card/{cardEventId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> DeleteCard(
        Guid matchId,
        Guid cardEventId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(new DeleteCardCommand(matchId, cardEventId), cancellationToken);
        return HandleResult(result, "Card deleted successfully", "Failed to delete card");
    }

    [HttpDelete("substitution/{substitutionEventId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> DeleteSubstitution(
        Guid matchId,
        Guid substitutionEventId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result =
            await _mediator.Send(new DeleteSubstitutionCommand(matchId, substitutionEventId), cancellationToken);
        return HandleResult(result, "Substitution deleted successfully", "Failed to delete substitution");
    }

    [HttpPost("extra-time")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> RecordExtraTime(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(new RecordExtraTimeCommand(matchId), cancellationToken);
        return HandleResult(result, "Extra time recorded successfully", "Failed to record extra time");
    }

    [HttpPost("penalty-shootout")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> RecordPenaltyShootout(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(new RecordPenaltyShootoutCommand(matchId), cancellationToken);
        return HandleResult(result, "Penalty shootout recorded successfully", "Failed to record penalty shootout");
    }

    [HttpPost("periods/{periodNumber:int}/start")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> StartPeriod(
        Guid matchId,
        int periodNumber,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(new StartPeriodCommand(matchId, periodNumber), cancellationToken);
        return HandleResult(result, "Period started successfully", "Failed to start period");
    }

    [HttpPost("periods/{periodNumber:int}/end")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> EndPeriod(
        Guid matchId,
        int periodNumber,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(new EndPeriodCommand(matchId, periodNumber), cancellationToken);
        return HandleResult(result, "Period ended successfully", "Failed to end period");
    }
}
