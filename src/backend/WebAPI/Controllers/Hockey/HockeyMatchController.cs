using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Hockey;

namespace WebAPI.Controllers.Hockey;

/// <summary>
/// API endpoints for hockey matches.
/// </summary>
[Route("api/[controller]")]
public class HockeyMatchController : BaseApiController
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Creates a new <see cref="HockeyMatchController"/>.
    /// </summary>
    public HockeyMatchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets a hockey match by id.
    /// </summary>
    [HttpGet("{matchId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> GetById(Guid matchId)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new GetHockeyMatchByIdQuery(matchId));
        return HandleResult(result, "Hockey match retrieved successfully", "Hockey match not found");
    }

    /// <summary>
    /// Creates a hockey match.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> Create([FromBody] CreateHockeyMatchRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new CreateHockeyMatchCommand(
            request.ScheduledStartTime,
            request.MatchType,
            request.CompetitionId,
            request.CompetitionDivisionId,
            request.TournamentGroupId,
            request.PlayoffSeriesId,
            request.Venue));

        if (result.IsSuccess && result.Data is not null)
        {
            return CreatedAtAction(
                nameof(GetById),
                new { matchId = result.Data.Id },
                ApiResponse<HockeyMatchDto>.SuccessResponse(result.Data, "Hockey match created successfully"));
        }

        return HandleResult(result, "Hockey match created successfully", "Failed to create hockey match");
    }

    /// <summary>
    /// Assigns home and away teams to a match.
    /// </summary>
    [HttpPut("{matchId:guid}/teams")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> AddHomeAwayTeams(
        Guid matchId,
        [FromBody] AddHomeAwayTeamsToHockeyMatchRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new AddHomeAwayTeamsToHockeyMatchCommand(
            matchId,
            request.HomeTeamId,
            request.AwayTeamId));

        return HandleResult(result, "Teams assigned to hockey match successfully", "Failed to assign teams");
    }

    /// <summary>
    /// Sets and confirms the roster for one match side.
    /// </summary>
    [HttpPost("{matchId:guid}/roster/confirm")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> ConfirmRoster(
        Guid matchId,
        [FromBody] ConfirmHockeyMatchRosterRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new ConfirmHockeyMatchRosterCommand(
            matchId,
            request.MatchTeamId,
            request.TeamPlayerIds,
            request.ConfirmedByUserId,
            request.Source));

        return HandleResult(result, "Hockey match roster confirmed successfully", "Failed to confirm roster");
    }

    /// <summary>
    /// Records a goal.
    /// </summary>
    [HttpPost("{matchId:guid}/events/goals")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordGoal(
        Guid matchId,
        [FromBody] RecordHockeyGoalRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new RecordHockeyGoalCommand(
            matchId,
            request.ScoringMatchTeamId,
            request.ScorerActivePlayerId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.GoalStrength,
            request.PrimaryAssistActivePlayerId,
            request.SecondaryAssistActivePlayerId,
            request.GoalieActivePlayerId,
            request.WasEmptyNet,
            request.Description));

        return HandleResult(result, "Goal recorded successfully", "Failed to record goal");
    }

    /// <summary>
    /// Records a penalty.
    /// </summary>
    [HttpPost("{matchId:guid}/events/penalties")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordPenalty(
        Guid matchId,
        [FromBody] RecordHockeyPenaltyRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new RecordHockeyPenaltyCommand(
            matchId,
            request.PenaltyMatchTeamId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.Severity,
            request.Offence,
            request.PenaltyMinutes,
            request.PenalizedActivePlayerId,
            request.ServedByActivePlayerId,
            request.IsBenchPenalty,
            request.Description));

        return HandleResult(result, "Penalty recorded successfully", "Failed to record penalty");
    }

    /// <summary>
    /// Records a shot.
    /// </summary>
    [HttpPost("{matchId:guid}/events/shots")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordShot(
        Guid matchId,
        [FromBody] RecordHockeyShotRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new RecordHockeyShotCommand(
            matchId,
            request.ShootingMatchTeamId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.ShotResult,
            request.CountsAsShotOnGoal,
            request.ShooterActivePlayerId,
            request.GoalieActivePlayerId,
            request.Description));

        return HandleResult(result, "Shot recorded successfully", "Failed to record shot");
    }

    /// <summary>
    /// Records a video review.
    /// </summary>
    [HttpPost("{matchId:guid}/events/video-reviews")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordVideoReview(
        Guid matchId,
        [FromBody] RecordHockeyVideoReviewRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new RecordHockeyVideoReviewCommand(
            matchId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.ReviewType,
            request.OriginalDecision,
            request.FinalDecision,
            request.IsCoachChallenge,
            request.WasSuccessful,
            request.RequestedByMatchTeamId,
            request.Description));

        return HandleResult(result, "Video review recorded successfully", "Failed to record video review");
    }
}
