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
/// Hockey match queries, creation, deletion, team assignment, venue and schedule.
/// Every hockey match controller shares the <c>api/HockeyMatch</c> route prefix.
/// </summary>
[Route("api/HockeyMatch")]
public class HockeyMatchesController : BaseApiController
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Creates a new <see cref="HockeyMatchesController"/>.
    /// </summary>
    public HockeyMatchesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets paginated hockey matches without event, line, or on-ice graphs.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PaginatedApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginatedApiResponse<HockeyMatchDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedApiResponse<HockeyMatchDto>>> GetPaged(
        [FromQuery] GetPagedHockeyMatchesRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<PagedResult<HockeyMatchDto>> result = await _mediator.Send(
            new GetPagedHockeyMatchesQuery(
                request.Page,
                request.PageSize,
                request.CompetitionId,
                request.TeamId,
                request.StartDate,
                request.EndDate,
                request.Status,
                request.SortOrder,
                request.SearchQuery,
                IncludeDrafts(request.IncludeDrafts)),
            cancellationToken);
        return HandlePaginatedResult(result, "Hockey matches retrieved successfully", "Failed to retrieve hockey matches");
    }

    /// <summary>
    /// Gets paginated hockey matches for public calendar and schedule views.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedApiResponse<HockeyMatchListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginatedApiResponse<HockeyMatchListDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedApiResponse<HockeyMatchListDto>>> GetList(
        [FromQuery] GetHockeyMatchesRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<PagedResult<HockeyMatchListDto>> result = await _mediator.Send(
            new GetHockeyMatchesQuery(
                request.Page,
                request.PageSize,
                request.StartDate,
                request.EndDate,
                request.TeamCategory,
                request.SortOrder,
                IncludeDrafts(request.IncludeDrafts),
                request.Statuses,
                request.CompetitionId,
                request.ActiveSeasonsOnly),
            cancellationToken);
        return HandlePaginatedResult(result, "Hockey matches retrieved successfully", "Failed to retrieve hockey matches");
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
        [FromQuery] bool includeDrafts = false,
        CancellationToken cancellationToken = default)
    {
        Result<IEnumerable<HockeyMatchDto>> result = await _mediator.Send(
            new GetHockeyMatchesByCompetitionQuery(competitionId, IncludeDrafts(includeDrafts)),
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
    /// Gets live, about-to-start, and just-finished matches with score and status only.
    /// Meant for short-interval polling; optionally scoped to one competition.
    /// </summary>
    [HttpGet("live")]
    [ProducesResponseType(typeof(ApiResponse<List<HockeyLiveMatchDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HockeyLiveMatchDto>>>> GetLive(
        [FromQuery] Guid? competitionId,
        [FromQuery] bool includeDrafts = false,
        CancellationToken cancellationToken = default)
    {
        Result<IEnumerable<HockeyLiveMatchDto>> result = await _mediator.Send(
            new GetHockeyLiveMatchesQuery(competitionId, IncludeDrafts(includeDrafts)),
            cancellationToken);
        return HandleListResult(result, "Live hockey matches retrieved successfully", "Failed to retrieve live hockey matches");
    }

    /// <summary>
    /// Gets the latest matches a career player was dressed for, newest first.
    /// Includes player selections but not events, lines, or on-ice state.
    /// </summary>
    /// <param name="playerId">Career player id.</param>
    /// <param name="limit">Maximum number of matches (1-200, default 50).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("player/{playerId:guid}/recent")]
    [ProducesResponseType(typeof(ApiResponse<List<HockeyMatchDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<HockeyMatchDto>>>> GetRecentByPlayer(
        Guid playerId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        Result<IEnumerable<HockeyMatchDto>> result = await _mediator.Send(
            new GetHockeyPlayerRecentMatchesQuery(playerId, limit),
            cancellationToken);
        return HandleListResult(result, "Hockey matches retrieved successfully", "Failed to retrieve hockey matches");
    }

    /// <summary>
    /// Creates a hockey match.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
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
            request.Venue,
            request.PlayoffRound,
            request.PlayoffMatchOrder,
            request.NextMatchId,
            request.NextMatchSlot), cancellationToken);

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
    [Authorize(Roles = AuthRoles.AdminOnly)]
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
    /// Updates match venue.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
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
    [Authorize(Roles = AuthRoles.AdminOnly)]
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
    /// Deletes a match that is still scheduled. Started and finished matches are left in place.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpDelete("{matchId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteMatch(Guid matchId, CancellationToken cancellationToken)
    {
        Result result = await _mediator.Send(new DeleteHockeyMatchCommand(matchId), cancellationToken);
        return HandleVoidResult(result, "Match deleted successfully", "Failed to delete match");
    }
}
