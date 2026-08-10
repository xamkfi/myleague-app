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
    /// Deletes a goal event (live-ops undo).
    /// </summary>
    [HttpDelete("{matchId:guid}/events/goals/{eventId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> DeleteGoal(Guid matchId, Guid eventId)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new DeleteHockeyGoalCommand(matchId, eventId));
        return HandleResult(result, "Goal deleted successfully", "Failed to delete goal");
    }

    /// <summary>
    /// Corrects a goal event (live-ops modify).
    /// </summary>
    [HttpPut("{matchId:guid}/events/goals/{eventId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> UpdateGoal(
        Guid matchId,
        Guid eventId,
        [FromBody] UpdateHockeyGoalRequest request)
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
            request.Description));

        return HandleResult(result, "Goal updated successfully", "Failed to update goal");
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
    /// Deletes a penalty event (live-ops undo).
    /// </summary>
    [HttpDelete("{matchId:guid}/events/penalties/{eventId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> DeletePenalty(Guid matchId, Guid eventId)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new DeleteHockeyPenaltyCommand(matchId, eventId));
        return HandleResult(result, "Penalty deleted successfully", "Failed to delete penalty");
    }

    /// <summary>
    /// Corrects a penalty event (live-ops modify).
    /// </summary>
    [HttpPut("{matchId:guid}/events/penalties/{eventId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> UpdatePenalty(
        Guid matchId,
        Guid eventId,
        [FromBody] UpdateHockeyPenaltyRequest request)
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
            request.Description));

        return HandleResult(result, "Penalty updated successfully", "Failed to update penalty");
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
    /// Deletes a shot event (live-ops undo).
    /// </summary>
    [HttpDelete("{matchId:guid}/events/shots/{eventId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> DeleteShot(Guid matchId, Guid eventId)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new DeleteHockeyShotCommand(matchId, eventId));
        return HandleResult(result, "Shot deleted successfully", "Failed to delete shot");
    }

    /// <summary>
    /// Corrects a shot event (live-ops modify).
    /// </summary>
    [HttpPut("{matchId:guid}/events/shots/{eventId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> UpdateShot(
        Guid matchId,
        Guid eventId,
        [FromBody] UpdateHockeyShotRequest request)
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
            request.Description));

        return HandleResult(result, "Shot updated successfully", "Failed to update shot");
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

    /// <summary>
    /// Adds a match line to one side.
    /// </summary>
    [HttpPost("{matchId:guid}/lines")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> AddLine(
        Guid matchId,
        [FromBody] AddHockeyMatchLineRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new AddHockeyMatchLineCommand(
            matchId,
            request.MatchTeamId,
            request.Name,
            request.LineType,
            request.LineNumber,
            request.Notes));
        return HandleResult(result, "Match line added successfully", "Failed to add match line");
    }

    /// <summary>
    /// Removes a match line.
    /// </summary>
    [HttpDelete("{matchId:guid}/lines/{matchLineId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RemoveLine(
        Guid matchId,
        Guid matchLineId,
        [FromQuery] Guid matchTeamId)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new RemoveHockeyMatchLineCommand(matchId, matchTeamId, matchLineId));
        return HandleResult(result, "Match line removed successfully", "Failed to remove match line");
    }

    /// <summary>
    /// Adds a player to a match line.
    /// </summary>
    [HttpPost("{matchId:guid}/lines/{matchLineId:guid}/players")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> AddLinePlayer(
        Guid matchId,
        Guid matchLineId,
        [FromBody] AddHockeyMatchLinePlayerRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new AddHockeyMatchLinePlayerCommand(
            matchId,
            request.MatchTeamId,
            matchLineId,
            request.MatchActivePlayerId,
            request.Slot,
            request.Order));
        return HandleResult(result, "Line player added successfully", "Failed to add line player");
    }

    /// <summary>
    /// Removes a player from a match line.
    /// </summary>
    [HttpDelete("{matchId:guid}/lines/{matchLineId:guid}/players/{matchActivePlayerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RemoveLinePlayer(
        Guid matchId,
        Guid matchLineId,
        Guid matchActivePlayerId,
        [FromQuery] Guid matchTeamId)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new RemoveHockeyMatchLinePlayerCommand(
            matchId,
            matchTeamId,
            matchLineId,
            matchActivePlayerId));
        return HandleResult(result, "Line player removed successfully", "Failed to remove line player");
    }

    /// <summary>
    /// Updates a match line name.
    /// </summary>
    [HttpPut("{matchId:guid}/lines/{matchLineId:guid}/name")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> UpdateLineName(
        Guid matchId,
        Guid matchLineId,
        [FromBody] UpdateHockeyMatchLineNameRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new UpdateHockeyMatchLineNameCommand(
            matchId,
            request.MatchTeamId,
            matchLineId,
            request.Name));
        return HandleResult(result, "Match line name updated successfully", "Failed to update line name");
    }

    /// <summary>
    /// Updates match line notes.
    /// </summary>
    [HttpPut("{matchId:guid}/lines/{matchLineId:guid}/notes")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> UpdateLineNotes(
        Guid matchId,
        Guid matchLineId,
        [FromBody] UpdateHockeyMatchLineNotesRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new UpdateHockeyMatchLineNotesCommand(
            matchId,
            request.MatchTeamId,
            matchLineId,
            request.Notes));
        return HandleResult(result, "Match line notes updated successfully", "Failed to update line notes");
    }

    /// <summary>
    /// Locks a match line.
    /// </summary>
    [HttpPost("{matchId:guid}/lines/{matchLineId:guid}/lock")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> LockLine(
        Guid matchId,
        Guid matchLineId,
        [FromQuery] Guid matchTeamId)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new LockHockeyMatchLineCommand(matchId, matchTeamId, matchLineId));
        return HandleResult(result, "Match line locked successfully", "Failed to lock match line");
    }

    /// <summary>
    /// Unlocks a match line.
    /// </summary>
    [HttpPost("{matchId:guid}/lines/{matchLineId:guid}/unlock")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> UnlockLine(
        Guid matchId,
        Guid matchLineId,
        [FromQuery] Guid matchTeamId)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new UnlockHockeyMatchLineCommand(matchId, matchTeamId, matchLineId));
        return HandleResult(result, "Match line unlocked successfully", "Failed to unlock match line");
    }

    /// <summary>
    /// Deactivates a match line.
    /// </summary>
    [HttpPost("{matchId:guid}/lines/{matchLineId:guid}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> DeactivateLine(
        Guid matchId,
        Guid matchLineId,
        [FromQuery] Guid matchTeamId)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new DeactivateHockeyMatchLineCommand(matchId, matchTeamId, matchLineId));
        return HandleResult(result, "Match line deactivated successfully", "Failed to deactivate match line");
    }

    /// <summary>
    /// Enables on-ice tracking for a match side.
    /// </summary>
    [HttpPost("{matchId:guid}/on-ice/enable")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> EnableOnIce(
        Guid matchId,
        [FromBody] HockeyMatchTeamIdRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new EnableHockeyMatchOnIceTrackingCommand(matchId, request.MatchTeamId, request.UserId));
        return HandleResult(result, "On-ice tracking enabled successfully", "Failed to enable on-ice tracking");
    }

    /// <summary>
    /// Disables on-ice tracking for a match side.
    /// </summary>
    [HttpPost("{matchId:guid}/on-ice/disable")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> DisableOnIce(
        Guid matchId,
        [FromBody] HockeyMatchTeamIdRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new DisableHockeyMatchOnIceTrackingCommand(matchId, request.MatchTeamId, request.UserId));
        return HandleResult(result, "On-ice tracking disabled successfully", "Failed to disable on-ice tracking");
    }

    /// <summary>
    /// Puts a player on the ice.
    /// </summary>
    [HttpPost("{matchId:guid}/on-ice/players")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> AddPlayerToIce(
        Guid matchId,
        [FromBody] AddHockeyMatchPlayerToIceRequest request)
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
            request.UserId));
        return HandleResult(result, "Player added to ice successfully", "Failed to add player to ice");
    }

    /// <summary>
    /// Removes a player from the ice.
    /// </summary>
    [HttpDelete("{matchId:guid}/on-ice/players/{matchActivePlayerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RemovePlayerFromIce(
        Guid matchId,
        Guid matchActivePlayerId,
        [FromQuery] Guid matchTeamId,
        [FromQuery] int? periodNumber = null,
        [FromQuery] int? timeInSeconds = null,
        [FromQuery] Guid? userId = null)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new RemoveHockeyMatchPlayerFromIceCommand(
            matchId,
            matchTeamId,
            matchActivePlayerId,
            periodNumber,
            timeInSeconds,
            userId));
        return HandleResult(result, "Player removed from ice successfully", "Failed to remove player from ice");
    }

    /// <summary>
    /// Clears all players from the ice.
    /// </summary>
    [HttpPost("{matchId:guid}/on-ice/clear")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> ClearIce(
        Guid matchId,
        [FromBody] HockeyMatchIceActionRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new ClearHockeyMatchIceCommand(
            matchId,
            request.MatchTeamId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.UserId));
        return HandleResult(result, "Ice cleared successfully", "Failed to clear ice");
    }

    /// <summary>
    /// Applies a match line onto the ice.
    /// </summary>
    [HttpPost("{matchId:guid}/on-ice/apply-line/{matchLineId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> ApplyLineToIce(
        Guid matchId,
        Guid matchLineId,
        [FromBody] HockeyMatchIceActionRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new ApplyHockeyMatchLineToIceCommand(
            matchId,
            request.MatchTeamId,
            matchLineId,
            request.PeriodNumber,
            request.TimeInSeconds,
            request.UserId));
        return HandleResult(result, "Line applied to ice successfully", "Failed to apply line to ice");
    }

    /// <summary>
    /// Sets the active goalie for a match side.
    /// </summary>
    [HttpPut("{matchId:guid}/active-goalie")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> SetActiveGoalie(
        Guid matchId,
        [FromBody] HockeyMatchTeamPlayerRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new SetHockeyMatchActiveGoalieCommand(
            matchId,
            request.MatchTeamId,
            request.MatchActivePlayerId));
        return HandleResult(result, "Active goalie set successfully", "Failed to set active goalie");
    }

    /// <summary>
    /// Clears the active goalie for a match side.
    /// </summary>
    [HttpDelete("{matchId:guid}/active-goalie")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> ClearActiveGoalie(
        Guid matchId,
        [FromQuery] Guid matchTeamId)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new ClearHockeyMatchActiveGoalieCommand(matchId, matchTeamId));
        return HandleResult(result, "Active goalie cleared successfully", "Failed to clear active goalie");
    }

    /// <summary>
    /// Deactivates a dressed roster player.
    /// </summary>
    [HttpPost("{matchId:guid}/roster/deactivate-player")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> DeactivateRosterPlayer(
        Guid matchId,
        [FromBody] HockeyMatchTeamPlayerRequest request)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new DeactivateHockeyMatchRosterPlayerCommand(
            matchId,
            request.MatchTeamId,
            request.MatchActivePlayerId));
        return HandleResult(result, "Roster player deactivated successfully", "Failed to deactivate roster player");
    }
}

