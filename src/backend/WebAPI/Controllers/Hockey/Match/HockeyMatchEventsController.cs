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
/// Records, corrects, deletes and imports hockey match events (goals, penalties, shots, video reviews, periods, faceoffs, stoppages, timeouts, goalie changes, shootout attempts, coach-challenge penalties).
/// Every hockey match controller shares the <c>api/HockeyMatch</c> route prefix.
/// </summary>
[Route("api/HockeyMatch")]
public class HockeyMatchEventsController : BaseApiController
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Creates a new <see cref="HockeyMatchEventsController"/>.
    /// </summary>
    public HockeyMatchEventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Records a goal.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/events/goals")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordGoal(
        Guid matchId,
        [FromBody] RecordHockeyGoalRequest request,
        CancellationToken cancellationToken = default)
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
            request.Description), cancellationToken);

        return HandleResult(result, "Goal recorded successfully", "Failed to record goal");
    }

    /// <summary>
    /// Deletes a goal event (live-ops undo).
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpDelete("{matchId:guid}/events/goals/{eventId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> DeleteGoal(Guid matchId, Guid eventId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new DeleteHockeyGoalCommand(matchId, eventId), cancellationToken);
        return HandleResult(result, "Goal deleted successfully", "Failed to delete goal");
    }

    /// <summary>
    /// Corrects a goal event (live-ops modify).
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPut("{matchId:guid}/events/goals/{eventId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> UpdateGoal(
        Guid matchId,
        Guid eventId,
        [FromBody] UpdateHockeyGoalRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new UpdateHockeyGoalCommand(
            matchId,
            eventId,
            request.ScoringMatchTeamId,
            request.ScorerActivePlayerId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.GoalStrength,
            request.PrimaryAssistActivePlayerId,
            request.SecondaryAssistActivePlayerId,
            request.GoalieActivePlayerId,
            request.WasEmptyNet,
            request.Description), cancellationToken);

        return HandleResult(result, "Goal updated successfully", "Failed to update goal");
    }

    /// <summary>
    /// Records a penalty.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/events/penalties")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordPenalty(
        Guid matchId,
        [FromBody] RecordHockeyPenaltyRequest request,
        CancellationToken cancellationToken = default)
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
            request.Description), cancellationToken);

        return HandleResult(result, "Penalty recorded successfully", "Failed to record penalty");
    }

    /// <summary>
    /// Imports a batch of historical goals and penalties onto an already-started match
    /// in one request. Failed individual events are listed in <c>eventErrors</c>;
    /// successful events are still saved. Season statistics are rebuilt on finish.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/events/import")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchEventsImportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyMatchEventsImportDto>>> ImportEvents(
        Guid matchId,
        [FromBody] ImportHockeyMatchEventsRequest request,
        CancellationToken cancellationToken = default)
    {
        List<ImportHockeyMatchEventItem> events = request.Events
            .Select(item => new ImportHockeyMatchEventItem(
                item.EventType,
                item.MatchTeamId,
                item.ActivePlayerId,
                item.PrimaryAssistActivePlayerId,
                item.SecondaryAssistActivePlayerId,
                item.GoalieActivePlayerId,
                item.PeriodNumber,
                item.TimeInSeconds,
                item.GoalStrength,
                item.WasEmptyNet,
                item.Description,
                item.Severity,
                item.Offence,
                item.PenaltyMinutes,
                item.ServedByActivePlayerId,
                item.IsBenchPenalty))
            .ToList();

        Result<HockeyMatchEventsImportDto> result = await _mediator.Send(
            new ImportHockeyMatchEventsCommand(matchId, events), cancellationToken);

        return HandleResult(result, "Match events imported", "Failed to import match events");
    }

    /// <summary>
    /// Deletes a penalty event (live-ops undo).
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpDelete("{matchId:guid}/events/penalties/{eventId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> DeletePenalty(Guid matchId, Guid eventId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new DeleteHockeyPenaltyCommand(matchId, eventId), cancellationToken);
        return HandleResult(result, "Penalty deleted successfully", "Failed to delete penalty");
    }

    /// <summary>
    /// Corrects a penalty event (live-ops modify).
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPut("{matchId:guid}/events/penalties/{eventId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> UpdatePenalty(
        Guid matchId,
        Guid eventId,
        [FromBody] UpdateHockeyPenaltyRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new UpdateHockeyPenaltyCommand(
            matchId,
            eventId,
            request.PenaltyMatchTeamId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.Severity,
            request.Offence,
            request.PenaltyMinutes,
            request.PenalizedActivePlayerId,
            request.ServedByActivePlayerId,
            request.IsBenchPenalty,
            request.Description), cancellationToken);

        return HandleResult(result, "Penalty updated successfully", "Failed to update penalty");
    }

    /// <summary>
    /// Records a shot. Set <c>count</c> above 1 to record that many saves at once.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/events/shots")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordShot(
        Guid matchId,
        [FromBody] RecordHockeyShotRequest request,
        CancellationToken cancellationToken = default)
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
            request.Description,
            request.Count), cancellationToken);

        return HandleResult(result, "Shot recorded successfully", "Failed to record shot");
    }

    /// <summary>
    /// Deletes a shot event (live-ops undo).
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpDelete("{matchId:guid}/events/shots/{eventId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> DeleteShot(Guid matchId, Guid eventId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new DeleteHockeyShotCommand(matchId, eventId), cancellationToken);
        return HandleResult(result, "Shot deleted successfully", "Failed to delete shot");
    }

    /// <summary>
    /// Corrects a shot event (live-ops modify).
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPut("{matchId:guid}/events/shots/{eventId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> UpdateShot(
        Guid matchId,
        Guid eventId,
        [FromBody] UpdateHockeyShotRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new UpdateHockeyShotCommand(
            matchId,
            eventId,
            request.ShootingMatchTeamId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.ShotResult,
            request.CountsAsShotOnGoal,
            request.ShooterActivePlayerId,
            request.GoalieActivePlayerId,
            request.Description), cancellationToken);

        return HandleResult(result, "Shot updated successfully", "Failed to update shot");
    }

    /// <summary>
    /// Records a video review.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/events/video-reviews")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordVideoReview(
        Guid matchId,
        [FromBody] RecordHockeyVideoReviewRequest request,
        CancellationToken cancellationToken = default)
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
            request.Description), cancellationToken);

        return HandleResult(result, "Video review recorded successfully", "Failed to record video review");
    }

    /// <summary>
    /// Records a period start/end event.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/events/periods")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordPeriodEvent(
        Guid matchId,
        [FromBody] RecordHockeyPeriodEventRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new RecordHockeyPeriodEventCommand(
            matchId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.Action,
            request.Description), cancellationToken);
        return HandleResult(result, "Period event recorded successfully", "Failed to record period event");
    }

    /// <summary>
    /// Records a faceoff.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/events/faceoffs")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordFaceoff(
        Guid matchId,
        [FromBody] RecordHockeyFaceoffRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new RecordHockeyFaceoffCommand(
            matchId,
            request.WinningMatchTeamId,
            request.LosingMatchTeamId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.Zone,
            request.Spot,
            request.WinningActivePlayerId,
            request.LosingActivePlayerId,
            request.Description), cancellationToken);
        return HandleResult(result, "Faceoff recorded successfully", "Failed to record faceoff");
    }

    /// <summary>
    /// Records a stoppage.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/events/stoppages")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordStoppage(
        Guid matchId,
        [FromBody] RecordHockeyStoppageRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new RecordHockeyStoppageCommand(
            matchId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.Reason,
            request.ResponsibleMatchTeamId,
            request.ResponsibleActivePlayerId,
            request.NextFaceoffZone,
            request.NextFaceoffSpot,
            request.RuleReference,
            request.Description), cancellationToken);
        return HandleResult(result, "Stoppage recorded successfully", "Failed to record stoppage");
    }

    /// <summary>
    /// Records a timeout.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/events/timeouts")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordTimeout(
        Guid matchId,
        [FromBody] RecordHockeyTimeoutRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new RecordHockeyTimeoutCommand(
            matchId,
            request.MatchTeamId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.Description), cancellationToken);
        return HandleResult(result, "Timeout recorded successfully", "Failed to record timeout");
    }

    /// <summary>
    /// Records a goalie change.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/events/goalie-changes")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordGoalieChange(
        Guid matchId,
        [FromBody] RecordHockeyGoalieChangeRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new RecordHockeyGoalieChangeCommand(
            matchId,
            request.MatchTeamId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.OutgoingGoalieActivePlayerId,
            request.IncomingGoalieActivePlayerId,
            request.Reason,
            request.Description), cancellationToken);
        return HandleResult(result, "Goalie change recorded successfully", "Failed to record goalie change");
    }

    /// <summary>
    /// Records a shootout attempt.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/events/shootout-attempts")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordShootoutAttempt(
        Guid matchId,
        [FromBody] RecordHockeyShootoutAttemptRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new RecordHockeyShootoutAttemptCommand(
            matchId,
            request.MatchTeamId,
            request.ShooterActivePlayerId,
            request.GoalieActivePlayerId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.ShotOrder,
            request.Result,
            request.Description), cancellationToken);
        return HandleResult(result, "Shootout attempt recorded successfully", "Failed to record shootout attempt");
    }

    /// <summary>
    /// Records a failed coach-challenge penalty linked to a video review.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/events/failed-coach-challenge-penalties")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordFailedCoachChallengePenalty(
        Guid matchId,
        [FromBody] RecordHockeyFailedCoachChallengePenaltyRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new RecordHockeyFailedCoachChallengePenaltyCommand(
            matchId,
            request.VideoReviewId,
            request.PenaltyMatchTeamId,
            request.Enabled,
            request.MaxChallengesPerTeam,
            request.LoseChallengeAfterFailed,
            request.PenaltyForFailedChallenge,
            request.FailedChallengePenaltyMinutes,
            request.FailedChallengePenaltyOffence,
            request.FailedChallengePenaltySeverity,
            request.AllowChallengeInOvertime,
            request.AllowChallengeInShootout), cancellationToken);
        return HandleResult(
            result,
            "Failed coach-challenge penalty recorded successfully",
            "Failed to record failed coach-challenge penalty");
    }
}
