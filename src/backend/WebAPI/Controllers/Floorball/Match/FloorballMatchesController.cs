using Application.Common;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Queries;
using Application.Features.Floorball.Teams.DTOs;
using Domain.Common;
using Domain.Enums.Floorball;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using WebAPI.Models.Floorball;

namespace WebAPI.Controllers.Floorball
{
    /// <summary>
    /// Read endpoints and CRUD lifecycle for floorball matches. Mutation endpoints that operate
    /// on a single match (start, complete, events, officials, roster) live in the dedicated
    /// sibling controllers under <c>api/floorball-matches/{matchId}</c>.
    /// </summary>
    [Route("api/floorball-matches")]
    public class FloorballMatchesController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FloorballMatchesController> _logger;

        /// <summary>
        /// Creates a new <see cref="FloorballMatchesController"/>.
        /// </summary>
        public FloorballMatchesController(
            IMediator mediator,
            ILogger<FloorballMatchesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all floorball matches with pagination and filtering
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballMatchDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballMatchDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FloorballMatchDto>>> GetAllMatches(
            [FromQuery] GetFloorballMatchesRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Getting all floorball matches with pagination - Page: {Page}, PageSize: {PageSize}, SortOrder: {SortOrder}",
                request.Page, request.PageSize, request.SortOrder);

            GetAllFloorballMatchesQuery query = new GetAllFloorballMatchesQuery(
                request.Page,
                request.PageSize,
                request.CompetitionId,
                request.TeamId,
                request.StartDate,
                request.EndDate,
                request.SortOrder,
                request.SearchQuery,
                request.Status,
                request.TournamentGroupId,
                request.CompetitionType
            );

            Result<PagedResult<FloorballMatchDto>> result = await _mediator.Send(query, cancellationToken);

            return HandlePaginatedResult(result, "Floorball matches retrieved successfully", "Failed to retrieve floorball matches");
        }

        /// <summary>
        /// Get a floorball match by ID
        /// </summary>
        [HttpGet("by-id/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> GetMatchById(
            Guid id,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting floorball match with ID of: {id}", id);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new GetFloorballMatchByIdQuery(id), cancellationToken);

            return HandleResult(result, "Floorball match retrieved successfully", "Floorball match not found");
        }

        /// <summary>
        /// Get floorball matches with season ID
        /// </summary>
        [HttpGet("by-competitionId/{competitionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballTeamDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballMatchDto>>>> GetMatchBySeason(
            Guid competitionId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting floorball matches with season ID of: {competitionId}", competitionId);

            Result<IEnumerable<FloorballMatchDto>> result = await _mediator.Send(
                new GetFloorballMatchesBySeasonQuery(competitionId), cancellationToken);

            return HandleListResult(result, "Retrieved floorball matches with season ID successfully", "Failed to retrieve floorball matches");
        }

        /// <summary>
        /// Get floorball matches with team ID
        /// </summary>
        [HttpGet("by-team/{teamId:guid}")]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballMatchDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballMatchDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FloorballMatchDto>>> GetMatchByTeam(
            Guid teamId,
            [FromQuery] GetTeamMatchesRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting floorball match with team ID of: {teamId}", teamId);

            GetFloorballMatchesByTeamQuery query = new GetFloorballMatchesByTeamQuery(
                Page: request.Page,
                PageSize: request.PageSize,
                TeamId: teamId,
                StartDate: request.StartDate,
                EndDate: request.EndDate
            );

            Result<PagedResult<FloorballMatchDto>> result = await _mediator.Send(query, cancellationToken);

            return HandlePaginatedResult(result, "Retrieved floorball matches with team ID successfully", "Failed to retrieve floorball matches with team ID");
        }

        /// <summary>
        /// Get today's floorball matches with team ID
        /// </summary>
        [HttpGet("by-team/{teamId:guid}/today")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballMatchDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballMatchDto>>>> GetTodaysMatchesByTeam(
            Guid teamId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting today's floorball match with team ID of: {teamId}", teamId);

            Result<IEnumerable<FloorballMatchDto>> result = await _mediator.Send(
                new GetTodaysMatchesByTeamQuery(teamId), cancellationToken);

            return HandleListResult(result, "Retrieved today's floorball matches with team ID successfully", "Failed to retrieve today's floorball matches with team ID");
        }

        /// <summary>
        /// Creates a new floorball match
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> CreateMatch(
            [FromBody] CreateFloorballMatchRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating floorball match {home} vs {away}", request.HomeTeamId, request.AwayTeamId);

            if (!DateTime.TryParse(request.ScheduledDateTime, out DateTime scheduledDateTime))
            {
                return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse("Invalid scheduled date and time format"));
            }

            FloorballTournamentStage? stage = null;
            if (!string.IsNullOrWhiteSpace(request.TournamentStage))
            {
                if (!Enum.TryParse(request.TournamentStage, true, out FloorballTournamentStage parsedStage))
                {
                    return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(
                        $"Invalid tournament stage '{request.TournamentStage}'. Valid values: {string.Join(", ", Enum.GetNames<FloorballTournamentStage>())}"));
                }
                stage = parsedStage;
            }

            CreateFloorballMatchCommand command = new CreateFloorballMatchCommand(
                request.CompetitionId,
                request.HomeTeamId,
                request.AwayTeamId,
                request.RefereeId,
                scheduledDateTime,
                request.Venue,
                request.TournamentGroupId,
                stage
            );

            Result<FloorballMatchDto> result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetMatchById),
                    new { id = result.Data.Id },
                    ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Floorball match created successfully")
                );
            }

            return ToErrorResponse(result, "Failed to create floorball match");
        }

        /// <summary>
        /// Updates an existing floorball match
        /// </summary>
        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> UpdateMatch(
            [FromBody] UpdateFloorballMatchRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating floorball match with ID: {id}", request.Id);

            if (!DateTimeOffset.TryParse(
                request.ScheduledDateTime,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out DateTimeOffset dto))
            {
                return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse("Invalid scheduled date and time format"));
            }

            DateTime scheduledUtc = dto.UtcDateTime;

            UpdateFloorballMatchCommand command = new UpdateFloorballMatchCommand(
                request.Id,
                scheduledUtc,
                request.Venue
            );

            Result<FloorballMatchDto> result = await _mediator.Send(command, cancellationToken);

            return HandleResult(result, "Floorball match updated successfully", "Failed to update floorball match");
        }

        /// <summary>
        /// Permanently deletes a floorball match. Only allowed for matches still in the
        /// <see cref="Domain.Enums.Floorball.FloorballMatchStatus.Scheduled"/> state — matches
        /// that have started, finished, or been cancelled cannot be deleted because they
        /// carry recorded events and statistics. Used by the tournament JSON import revert flow.
        /// </summary>
        [HttpDelete("{matchId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteMatch(
            Guid matchId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting match ID: {matchId}", matchId);

            Result result = await _mediator.Send(
                new DeleteFloorballMatchCommand(matchId), cancellationToken);

            return HandleVoidResult(result, "Match deleted successfully", "Failed to delete match");
        }
    }
}
