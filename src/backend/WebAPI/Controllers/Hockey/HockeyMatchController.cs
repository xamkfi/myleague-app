using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> GetById(
        Guid matchId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new GetHockeyMatchByIdQuery(matchId),
            cancellationToken);
        return HandleResult(result, "Hockey match retrieved successfully", "Hockey match not found");
    }

    /// <summary>
    /// Gets hockey matches for a competition (season or tournament).
    /// </summary>
    [HttpGet("competition/{competitionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<HockeyMatchDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HockeyMatchDto>>>> GetByCompetition(
        Guid competitionId,
        CancellationToken cancellationToken = default)
    {
        Result<IEnumerable<HockeyMatchDto>> result = await _mediator.Send(
            new GetHockeyMatchesByCompetitionQuery(competitionId),
            cancellationToken);
        return HandleListResult(result, "Hockey matches retrieved successfully", "Failed to retrieve hockey matches");
    }

    /// <summary>
    /// Gets hockey matches involving a career team (home or away).
    /// </summary>
    [HttpGet("team/{teamId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<HockeyMatchDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HockeyMatchDto>>>> GetByTeam(
        Guid teamId,
        CancellationToken cancellationToken = default)
    {
        Result<IEnumerable<HockeyMatchDto>> result = await _mediator.Send(
            new GetHockeyMatchesByTeamQuery(teamId),
            cancellationToken);
        return HandleListResult(result, "Hockey matches retrieved successfully", "Failed to retrieve hockey matches");
    }

    /// <summary>
    /// Creates a hockey match.
    /// </summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> Create([FromBody] CreateHockeyMatchRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new CreateHockeyMatchCommand(
            request.ScheduledStartTime,
            request.MatchType,
            request.CompetitionId,
            request.CompetitionDivisionId,
            request.TournamentGroupId,
            request.PlayoffSeriesId,
            request.Venue), cancellationToken);

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
    [Authorize]
    [HttpPut("{matchId:guid}/teams")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> AddHomeAwayTeams(
        Guid matchId,
        [FromBody] AddHomeAwayTeamsToHockeyMatchRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new AddHomeAwayTeamsToHockeyMatchCommand(
            matchId,
            request.HomeTeamId,
            request.AwayTeamId), cancellationToken);

        return HandleResult(result, "Teams assigned to hockey match successfully", "Failed to assign teams");
    }

    /// <summary>
    /// Sets and confirms the roster for one match side.
    /// </summary>
    [Authorize]
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
    /// Marks a hockey match as started.
    /// </summary>
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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
    /// Updates match venue.
    /// </summary>
    [Authorize]
    [HttpPut("{matchId:guid}/venue")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> UpdateVenue(
        Guid matchId,
        [FromBody] UpdateHockeyMatchVenueRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new UpdateHockeyMatchVenueCommand(matchId, request.Venue), cancellationToken);
        return HandleResult(result, "Hockey match venue updated successfully", "Failed to update venue");
    }

    /// <summary>
    /// Updates scheduled start time.
    /// </summary>
    [Authorize]
    [HttpPut("{matchId:guid}/schedule")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> UpdateScheduledStart(
        Guid matchId,
        [FromBody] UpdateHockeyMatchScheduledStartRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new UpdateHockeyMatchScheduledStartCommand(matchId, request.ScheduledStartTime), cancellationToken);
        return HandleResult(result, "Hockey match schedule updated successfully", "Failed to update schedule");
    }

    /// <summary>
    /// Corrects goals for one match side.
    /// </summary>
    [Authorize]
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
    /// Assigns an official to the match.
    /// </summary>
    [Authorize]
    [HttpPost("{matchId:guid}/officials")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> AddOfficial(
        Guid matchId,
        [FromBody] AddHockeyMatchOfficialRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new AddHockeyMatchOfficialCommand(
            matchId,
            request.OfficialId,
            request.Role,
            request.IsMainOfficial), cancellationToken);
        return HandleResult(result, "Official added to hockey match successfully", "Failed to add official");
    }

    /// <summary>
    /// Creates a period score row.
    /// </summary>
    [Authorize]
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

    /// <summary>
    /// Records a goal.
    /// </summary>
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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
    /// Deletes a penalty event (live-ops undo).
    /// </summary>
    [Authorize]
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
    [Authorize]
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
    /// Records a shot.
    /// </summary>
    [Authorize]
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
            request.Description), cancellationToken);

        return HandleResult(result, "Shot recorded successfully", "Failed to record shot");
    }

    /// <summary>
    /// Deletes a shot event (live-ops undo).
    /// </summary>
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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

    /// <summary>
    /// Adds a match line to one side.
    /// </summary>
    [Authorize]
    [HttpPost("{matchId:guid}/lines")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> AddLine(
        Guid matchId,
        [FromBody] AddHockeyMatchLineRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new AddHockeyMatchLineCommand(
            matchId,
            request.MatchTeamId,
            request.Name,
            request.LineType,
            request.LineNumber,
            request.Notes), cancellationToken);
        return HandleResult(result, "Match line added successfully", "Failed to add match line");
    }

    /// <summary>
    /// Removes a match line.
    /// </summary>
    [Authorize]
    [HttpDelete("{matchId:guid}/lines/{matchLineId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RemoveLine(
        Guid matchId,
        Guid matchLineId,
        [FromQuery] Guid matchTeamId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new RemoveHockeyMatchLineCommand(matchId, matchTeamId, matchLineId), cancellationToken);
        return HandleResult(result, "Match line removed successfully", "Failed to remove match line");
    }

    /// <summary>
    /// Adds a player to a match line.
    /// </summary>
    [Authorize]
    [HttpPost("{matchId:guid}/lines/{matchLineId:guid}/players")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> AddLinePlayer(
        Guid matchId,
        Guid matchLineId,
        [FromBody] AddHockeyMatchLinePlayerRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new AddHockeyMatchLinePlayerCommand(
            matchId,
            request.MatchTeamId,
            matchLineId,
            request.MatchActivePlayerId,
            request.Slot,
            request.Order), cancellationToken);
        return HandleResult(result, "Line player added successfully", "Failed to add line player");
    }

    /// <summary>
    /// Removes a player from a match line.
    /// </summary>
    [Authorize]
    [HttpDelete("{matchId:guid}/lines/{matchLineId:guid}/players/{matchActivePlayerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RemoveLinePlayer(
        Guid matchId,
        Guid matchLineId,
        Guid matchActivePlayerId,
        [FromQuery] Guid matchTeamId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new RemoveHockeyMatchLinePlayerCommand(
            matchId,
            matchTeamId,
            matchLineId,
            matchActivePlayerId), cancellationToken);
        return HandleResult(result, "Line player removed successfully", "Failed to remove line player");
    }

    /// <summary>
    /// Updates a match line name.
    /// </summary>
    [Authorize]
    [HttpPut("{matchId:guid}/lines/{matchLineId:guid}/name")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> UpdateLineName(
        Guid matchId,
        Guid matchLineId,
        [FromBody] UpdateHockeyMatchLineNameRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new UpdateHockeyMatchLineNameCommand(
            matchId,
            request.MatchTeamId,
            matchLineId,
            request.Name), cancellationToken);
        return HandleResult(result, "Match line name updated successfully", "Failed to update line name");
    }

    /// <summary>
    /// Updates match line notes.
    /// </summary>
    [Authorize]
    [HttpPut("{matchId:guid}/lines/{matchLineId:guid}/notes")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> UpdateLineNotes(
        Guid matchId,
        Guid matchLineId,
        [FromBody] UpdateHockeyMatchLineNotesRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new UpdateHockeyMatchLineNotesCommand(
            matchId,
            request.MatchTeamId,
            matchLineId,
            request.Notes), cancellationToken);
        return HandleResult(result, "Match line notes updated successfully", "Failed to update line notes");
    }

    /// <summary>
    /// Locks a match line.
    /// </summary>
    [Authorize]
    [HttpPost("{matchId:guid}/lines/{matchLineId:guid}/lock")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> LockLine(
        Guid matchId,
        Guid matchLineId,
        [FromQuery] Guid matchTeamId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new LockHockeyMatchLineCommand(matchId, matchTeamId, matchLineId), cancellationToken);
        return HandleResult(result, "Match line locked successfully", "Failed to lock match line");
    }

    /// <summary>
    /// Unlocks a match line.
    /// </summary>
    [Authorize]
    [HttpPost("{matchId:guid}/lines/{matchLineId:guid}/unlock")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> UnlockLine(
        Guid matchId,
        Guid matchLineId,
        [FromQuery] Guid matchTeamId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new UnlockHockeyMatchLineCommand(matchId, matchTeamId, matchLineId), cancellationToken);
        return HandleResult(result, "Match line unlocked successfully", "Failed to unlock match line");
    }

    /// <summary>
    /// Deactivates a match line.
    /// </summary>
    [Authorize]
    [HttpPost("{matchId:guid}/lines/{matchLineId:guid}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> DeactivateLine(
        Guid matchId,
        Guid matchLineId,
        [FromQuery] Guid matchTeamId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new DeactivateHockeyMatchLineCommand(matchId, matchTeamId, matchLineId), cancellationToken);
        return HandleResult(result, "Match line deactivated successfully", "Failed to deactivate match line");
    }

    /// <summary>
    /// Enables on-ice tracking for a match side.
    /// </summary>
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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

    /// <summary>
    /// Sets the active goalie for a match side.
    /// </summary>
    [Authorize]
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
    [Authorize]
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

    /// <summary>
    /// Deactivates a dressed roster player.
    /// </summary>
    [Authorize]
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
}

