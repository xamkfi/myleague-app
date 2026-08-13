using Application.Common;
using Domain.Common;
using Application.Features.Football.Teams.Commands;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Teams.Queries;
using Domain.Enums.Football;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using WebAPI.Models.Football;

namespace WebAPI.Controllers.Football
{
    /// <summary>
    /// Controller for managing football teams
    /// </summary>
    [Route("api/[controller]")]
    public class FootballTeamController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FootballTeamController> _logger;

        /// <summary>
        /// Initializes new instance of FootballTeamController class
        /// </summary>
        /// <param name="mediator">Mediator instance for handling commands and queries</param>
        /// <param name="logger">Logger instance for logging</param>
        public FootballTeamController(IMediator mediator, ILogger<FootballTeamController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Gets all football teams with pagination and filtering
        /// </summary>
        /// <param name="request">Query parameters for pagination and filtering</param>
        /// <returns>Paginated list of football teams</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedApiResponse<FootballTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FootballTeamDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FootballTeamDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FootballTeamDto>>> GetAllTeams([FromQuery] GetFootballTeamsRequest request)
        {
            _logger.LogInformation("Getting all football teams with pagination - Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

            Result<PagedResult<FootballTeamDto>> result = await _mediator.Send(new GetAllFootballTeamsQuery(
                request.Page,
                request.PageSize,
                request.ClubId,
                request.Division,
                request.TeamCategories));

            return HandlePaginatedResult(result, "Football teams retrieved successfully", "Failed to retrieve football teams");
        }

        /// <summary>
        /// Gets a football team by ID
        /// </summary>
        /// <param name="id">Team ID</param>
        /// <returns>Team details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FootballTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FootballTeamDto>>> GetTeamById(Guid id)
        {
            _logger.LogInformation("Getting football team with ID: {id}", id);

            Result<FootballTeamDto> result = await _mediator.Send(new GetFootballTeamByIdQuery(id));

            return HandleResult(result, "Football team retrieved successfully", "Failed to retrieve football team");
        }

        /// <summary>
        /// Gets all football teams for a specific club
        /// </summary>
        /// <param name="clubId">Club ID</param>
        /// <returns>List of teams belonging to the club</returns>
        [HttpGet("club/{clubId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<FootballTeamDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FootballTeamDto>>>> GetTeamsByClub(Guid clubId)
        {
            _logger.LogInformation("Getting football teams for club with ID: {clubId}", clubId);

            Result<IEnumerable<FootballTeamDto>> result = await _mediator.Send(new GetFootballTeamsByClubQuery(clubId));

            return HandleListResult(result, "Football teams retrieved successfully", "Failed to retrieve football teams");
        }

        /// <summary>
        /// Gets all football teams in a specific division
        /// </summary>
        /// <param name="divisionId">Division</param>
        /// <returns>List of teams in the division</returns>
        [HttpGet("division/{divisionId}")]
        [ProducesResponseType(typeof(ApiResponse<List<FootballTeamDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FootballTeamDto>>>> GetTeamsByDivision(Guid divisionId)
        {
            _logger.LogInformation("Getting football teams for division: {division}", divisionId);

            Result<IEnumerable<FootballTeamDto>> result = await _mediator.Send(new GetFootballTeamsByDivisionQuery(divisionId));

            return HandleListResult(result, "Football teams retrieved successfully", "Failed to retrieve football teams");
        }

        /// <summary>
        /// Gets football team names filtered by name
        /// </summary>
        /// <param name="nameFilter">Optional filter string to search team names</param>
        /// <returns>List of matching team names</returns>
        [HttpGet("names")]
        [ProducesResponseType(typeof(ApiResponse<List<FootballTeamNameDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FootballTeamNameDto>>>> GetTeamNames([FromQuery] string? nameFilter)
        {
            _logger.LogInformation("Getting football team names filtered by: {nameFilter}", SanitizeForLog(nameFilter));

            Result<List<FootballTeamNameDto>> result = await _mediator.Send(new GetTeamNamesQuery(nameFilter));

            return HandleResult(result, "Filtered team names retrieved successfully", "Failed to retrieve team names");
        }

        /// <summary>
        /// Gets all football teams without roster with pagination, search, and filtering
        /// </summary>
        /// <param name="request">Query parameters for pagination and filtering</param>
        /// <returns>Paginated list of football teams without roster</returns>
        [HttpGet("without-roster")]
        [ProducesResponseType(typeof(PaginatedApiResponse<FootballTeamSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FootballTeamSummaryDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FootballTeamSummaryDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FootballTeamSummaryDto>>> GetAllTeamsWithoutRoster([FromQuery] GetAllTeamsWithoutRosterRequest request)
        {
            _logger.LogInformation(
                "Getting all football teams without roster - Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}, TeamCategory: {TeamCategory}",
                request.Page,
                request.PageSize,
                SanitizeForLog(request.SearchTerm),
                SanitizeForLog(request.TeamCategory));

            Result<PagedResult<FootballTeamSummaryDto>> result = await _mediator.Send(new GetAllTeamsWithoutRosterQuery(
                request.Page,
                request.PageSize,
                request.SearchTerm,
                request.TeamCategory));

            return HandlePaginatedResult(result, "Football teams without roster retrieved successfully", "Failed to retrieve football teams without roster");
        }

        /// <summary>
        /// Creates a new football team
        /// </summary>
        /// <param name="request">Create team request</param>
        /// <returns>Created team details</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FootballTeamDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FootballTeamDto>>> CreateTeam([FromBody] FootballTeamRequest request)
        {
            _logger.LogInformation("Creating football team: {name}", SanitizeForLog(request.Name));

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Invalid model state for team creation: {errors}",
                    SanitizeForLog(string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
                return BadRequest(ApiResponse<FootballTeamDto>.ErrorResponse(ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()));
            }

            Result<FootballTeamDto> result = await _mediator.Send(new CreateFootballTeamCommand(
                request.Name,
                request.DivisionId,
                request.ClubId,
                request.HomeArena,
                request.PrimaryJerseyColor,
                request.Category,
                request.SecondaryJerseyColor,
                request.ShortName));

            if (result.IsSuccess && result.Data is not null)
            {
                return CreatedAtAction(
                    nameof(GetTeamById),
                    new { id = result.Data.Id },
                    ApiResponse<FootballTeamDto>.SuccessResponse(result.Data, "Football team created successfully"));
            }

            return ToErrorResponse(result, "Failed to create football team");
        }

        /// <summary>
        /// Updates an existing football team
        /// </summary>
        /// <param name="id">Team ID</param>
        /// <param name="request">Update team request</param>
        /// <returns>Updated team details</returns>
        [HttpPut("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FootballTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FootballTeamDto>>> UpdateTeam(Guid id, [FromBody] FootballTeamRequest request)
        {
            _logger.LogInformation("Updating football team with ID: {id}", id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Invalid model state for team update: {errors}",
                    SanitizeForLog(string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
                return BadRequest(ApiResponse<FootballTeamDto>.ErrorResponse(ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()));
            }

            Result<FootballTeamDto> result = await _mediator.Send(new UpdateFootballTeamCommand(
                id,
                request.Name,
                request.DivisionId,
                request.HomeArena,
                request.PrimaryJerseyColor,
                request.Category,
                request.SecondaryJerseyColor,
                request.LogoUrl,
                request.ShortName));

            return HandleResult(result, "Football team updated successfully", "Failed to update football team");
        }

        /// <summary>
        /// Deletes a football team
        /// </summary>
        /// <param name="id">Team ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteTeam(Guid id)
        {
            _logger.LogInformation("Deleting football team with ID: {id}", id);

            Result result = await _mediator.Send(new DeleteFootballTeamCommand(id));

            return HandleVoidResult(result, "Football team deleted successfully", "Failed to delete football team");
        }

        /// <summary>
        /// Adds a player to a team
        /// </summary>
        /// <param name="teamId">Team ID</param>
        /// <param name="playerId">Player ID</param>
        /// <param name="position">Player position</param>
        /// <param name="jerseyNumber">Player jersey number (optional)</param>
        /// <param name="requestedJerseyNumber">
        /// The jersey number originally requested by the caller. When it differs from
        /// <paramref name="jerseyNumber"/>, the roster entry is flagged so the admin UI can
        /// highlight the row for review. Pass <c>null</c> when there was no substitution.
        /// </param>
        /// <returns>Updated team details</returns>
        [HttpPost("{teamId:guid}/players/{playerId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FootballTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FootballTeamDto>>> AddPlayerToTeam(
            Guid teamId,
            Guid playerId,
            [FromQuery] FootballPosition position,
            [FromQuery] int? jerseyNumber = null,
            [FromQuery] int? requestedJerseyNumber = null)
        {
            _logger.LogInformation(
                "Adding player {playerId} to team {teamId} with position {position}",
                playerId,
                teamId,
                SanitizeForLog(position));

            Result<FootballTeamDto> result = await _mediator.Send(new AddPlayerToTeamCommand(
                teamId,
                playerId,
                position,
                jerseyNumber,
                requestedJerseyNumber));

            return HandleResult(result, "Player added to team successfully", "Failed to add player to team");
        }

        /// <summary>
        /// Removes a player from a team
        /// </summary>
        /// <param name="teamId">Team ID</param>
        /// <param name="playerId">Player ID</param>
        /// <returns>Updated team details</returns>
        [HttpDelete("{teamId:guid}/players/{playerId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FootballTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FootballTeamDto>>> RemovePlayerFromTeam(Guid teamId, Guid playerId)
        {
            _logger.LogInformation("Removing player {playerId} from team {teamId}", playerId, teamId);

            Result<FootballTeamDto> result = await _mediator.Send(new RemovePlayerFromTeamCommand(teamId, playerId));

            return HandleResult(result, "Player removed from team successfully", "Failed to remove player from team");
        }

        /// <summary>
        /// Updates a player's information in a team roster
        /// </summary>
        /// <param name="teamId">Team ID</param>
        /// <param name="playerId">Player ID</param>
        /// <param name="request">Update team player request (position, jersey number, active status)</param>
        /// <returns>Updated team player details</returns>
        [HttpPut("{teamId:guid}/players/{playerId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FootballTeamPlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FootballTeamPlayerDto>>> UpdateTeamPlayer(
            Guid teamId,
            Guid playerId,
            [FromBody] UpdateFootballTeamPlayerRequest request)
        {
            _logger.LogInformation(
                "Updating player {playerId} in team {teamId} with position {position}, jersey {jerseyNumber}, active {isActive}",
                playerId,
                teamId,
                SanitizeForLog(request.Position),
                request.JerseyNumber,
                request.IsActive);

            Result<FootballTeamPlayerDto> result = await _mediator.Send(new UpdateTeamPlayerCommand(
                teamId,
                playerId,
                request.Position,
                request.JerseyNumber,
                request.IsActive));

            return HandleResult(result, "Team player updated successfully", "Failed to update team player");
        }

        /// <summary>
        /// Updates the division of a football team
        /// </summary>
        /// <param name="teamId">The ID of the team to update</param>
        /// <param name="divisionId">The ID of the new division</param>
        /// <returns>Updated team details</returns>
        [HttpPatch("{teamId:guid}/division{divisionId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FootballTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FootballTeamDto>>> UpdateTeamDivision(Guid teamId, Guid divisionId)
        {
            _logger.LogInformation("Updating teams {teamId} into division {divisionId}", teamId, divisionId);

            Result<FootballTeamDto> result = await _mediator.Send(new UpdateTeamDivisionCommand(teamId, divisionId));

            return HandleResult(result, "Team division updated succesfully", "Failed to update teams division");
        }

        /// <summary>
        /// Updates the logo of a football team
        /// </summary>
        /// <param name="id">The ID of the team to update</param>
        /// <param name="logoUrl">The new logo URL</param>
        /// <returns>Updated team details</returns>
        [HttpPatch("{id:guid}/logo")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FootballTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FootballTeamDto>>> UpdateTeamLogo(Guid id, [FromBody] string? logoUrl)
        {
            _logger.LogInformation("Updating logo for team {teamId}", id);

            Result<FootballTeamDto> result = await _mediator.Send(new UpdateFootballTeamLogoCommand(id, logoUrl));

            return HandleResult(result, "Team logo updated successfully", "Failed to update team logo");
        }
    }
}
