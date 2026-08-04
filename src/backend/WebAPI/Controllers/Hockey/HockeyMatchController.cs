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
    /// Marks a hockey match as started.
    /// </summary>
    [HttpPost("{matchId:guid}/start")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> Start(
        Guid matchId,
        [FromBody] MarkHockeyMatchStartedRequest? request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new MarkHockeyMatchStartedCommand(matchId, request?.ActualStartTime));
        return HandleResult(result, "Hockey match started successfully", "Failed to start hockey match");
    }

    /// <summary>
    /// Marks a hockey match as finished.
    /// </summary>
    [HttpPost("{matchId:guid}/finish")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> Finish(
        Guid matchId,
        [FromBody] MarkHockeyMatchFinishedRequest? request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new MarkHockeyMatchFinishedCommand(matchId, request?.ActualEndTime, request?.ResultType));
        return HandleResult(result, "Hockey match finished successfully", "Failed to finish hockey match");
    }

    /// <summary>
    /// Sets match status.
    /// </summary>
    [HttpPatch("{matchId:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> SetStatus(
        Guid matchId,
        [FromBody] SetHockeyMatchStatusRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new SetHockeyMatchStatusCommand(matchId, request.Status));
        return HandleResult(result, "Hockey match status updated successfully", "Failed to update status");
    }

    /// <summary>
    /// Sets match result type.
    /// </summary>
    [HttpPatch("{matchId:guid}/result")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> SetResultType(
        Guid matchId,
        [FromBody] SetHockeyMatchResultTypeRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new SetHockeyMatchResultTypeCommand(matchId, request.ResultType));
        return HandleResult(result, "Hockey match result type updated successfully", "Failed to update result type");
    }

    /// <summary>
    /// Sets the current period number.
    /// </summary>
    [HttpPatch("{matchId:guid}/period")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> SetCurrentPeriod(
        Guid matchId,
        [FromBody] SetHockeyMatchCurrentPeriodRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new SetHockeyMatchCurrentPeriodCommand(matchId, request.PeriodNumber));
        return HandleResult(result, "Hockey match period updated successfully", "Failed to update period");
    }

    /// <summary>
    /// Sets whether the match went to overtime.
    /// </summary>
    [HttpPatch("{matchId:guid}/went-to-overtime")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> SetWentToOvertime(
        Guid matchId,
        [FromBody] SetHockeyMatchBooleanFlagRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new SetHockeyMatchWentToOvertimeCommand(matchId, request.Value));
        return HandleResult(result, "Hockey match overtime flag updated successfully", "Failed to update overtime flag");
    }

    /// <summary>
    /// Sets whether the match went to shootout.
    /// </summary>
    [HttpPatch("{matchId:guid}/went-to-shootout")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> SetWentToShootout(
        Guid matchId,
        [FromBody] SetHockeyMatchBooleanFlagRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new SetHockeyMatchWentToShootoutCommand(matchId, request.Value));
        return HandleResult(result, "Hockey match shootout flag updated successfully", "Failed to update shootout flag");
    }

    /// <summary>
    /// Updates match venue.
    /// </summary>
    [HttpPut("{matchId:guid}/venue")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> UpdateVenue(
        Guid matchId,
        [FromBody] UpdateHockeyMatchVenueRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new UpdateHockeyMatchVenueCommand(matchId, request.Venue));
        return HandleResult(result, "Hockey match venue updated successfully", "Failed to update venue");
    }

    /// <summary>
    /// Updates scheduled start time.
    /// </summary>
    [HttpPut("{matchId:guid}/schedule")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> UpdateScheduledStart(
        Guid matchId,
        [FromBody] UpdateHockeyMatchScheduledStartRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new UpdateHockeyMatchScheduledStartCommand(matchId, request.ScheduledStartTime));
        return HandleResult(result, "Hockey match schedule updated successfully", "Failed to update schedule");
    }

    /// <summary>
    /// Corrects goals for one match side.
    /// </summary>
    [HttpPut("{matchId:guid}/team-goals")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> SetTeamGoals(
        Guid matchId,
        [FromBody] SetHockeyMatchTeamGoalsRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new SetHockeyMatchTeamGoalsCommand(matchId, request.TeamSlot, request.Goals));
        return HandleResult(result, "Hockey match team goals updated successfully", "Failed to update team goals");
    }

    /// <summary>
    /// Assigns an official to the match.
    /// </summary>
    [HttpPost("{matchId:guid}/officials")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> AddOfficial(
        Guid matchId,
        [FromBody] AddHockeyMatchOfficialRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new AddHockeyMatchOfficialCommand(
            matchId,
            request.OfficialId,
            request.Role,
            request.IsMainOfficial));
        return HandleResult(result, "Official added to hockey match successfully", "Failed to add official");
    }

    /// <summary>
    /// Creates a period score row.
    /// </summary>
    [HttpPost("{matchId:guid}/period-scores")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> AddPeriodScore(
        Guid matchId,
        [FromBody] AddHockeyPeriodScoreRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new AddHockeyPeriodScoreCommand(
            matchId,
            request.PeriodNumber,
            request.PeriodType));
        return HandleResult(result, "Period score added successfully", "Failed to add period score");
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

    /// <summary>
    /// Records a period start/end event.
    /// </summary>
    [HttpPost("{matchId:guid}/events/periods")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordPeriodEvent(
        Guid matchId,
        [FromBody] RecordHockeyPeriodEventRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new RecordHockeyPeriodEventCommand(
            matchId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.Action,
            request.Description));
        return HandleResult(result, "Period event recorded successfully", "Failed to record period event");
    }

    /// <summary>
    /// Records a faceoff.
    /// </summary>
    [HttpPost("{matchId:guid}/events/faceoffs")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordFaceoff(
        Guid matchId,
        [FromBody] RecordHockeyFaceoffRequest request)
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
            request.Description));
        return HandleResult(result, "Faceoff recorded successfully", "Failed to record faceoff");
    }

    /// <summary>
    /// Records a stoppage.
    /// </summary>
    [HttpPost("{matchId:guid}/events/stoppages")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordStoppage(
        Guid matchId,
        [FromBody] RecordHockeyStoppageRequest request)
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
            request.Description));
        return HandleResult(result, "Stoppage recorded successfully", "Failed to record stoppage");
    }

    /// <summary>
    /// Records a timeout.
    /// </summary>
    [HttpPost("{matchId:guid}/events/timeouts")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordTimeout(
        Guid matchId,
        [FromBody] RecordHockeyTimeoutRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new RecordHockeyTimeoutCommand(
            matchId,
            request.MatchTeamId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.Description));
        return HandleResult(result, "Timeout recorded successfully", "Failed to record timeout");
    }

    /// <summary>
    /// Records a goalie change.
    /// </summary>
    [HttpPost("{matchId:guid}/events/goalie-changes")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordGoalieChange(
        Guid matchId,
        [FromBody] RecordHockeyGoalieChangeRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new RecordHockeyGoalieChangeCommand(
            matchId,
            request.MatchTeamId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.OutgoingGoalieActivePlayerId,
            request.IncomingGoalieActivePlayerId,
            request.Reason,
            request.Description));
        return HandleResult(result, "Goalie change recorded successfully", "Failed to record goalie change");
    }

    /// <summary>
    /// Records a shootout attempt.
    /// </summary>
    [HttpPost("{matchId:guid}/events/shootout-attempts")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordShootoutAttempt(
        Guid matchId,
        [FromBody] RecordHockeyShootoutAttemptRequest request)
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
            request.Description));
        return HandleResult(result, "Shootout attempt recorded successfully", "Failed to record shootout attempt");
    }

    /// <summary>
    /// Records a failed coach-challenge penalty linked to a video review.
    /// </summary>
    [HttpPost("{matchId:guid}/events/failed-coach-challenge-penalties")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RecordFailedCoachChallengePenalty(
        Guid matchId,
        [FromBody] RecordHockeyFailedCoachChallengePenaltyRequest request)
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
            request.AllowChallengeInShootout));
        return HandleResult(
            result,
            "Failed coach-challenge penalty recorded successfully",
            "Failed to record failed coach-challenge penalty");
    }
}
