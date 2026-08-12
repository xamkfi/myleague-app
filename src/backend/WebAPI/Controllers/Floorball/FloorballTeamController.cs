using Application.Common;
using Domain.Common;
using Application.Features.Floorball.Teams.Commands;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Teams.Queries;
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
    /// Controller for managing floorball teams
    /// </summary>
    [Route("api/[controller]")]
    public class FloorballTeamController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FloorballTeamController> _logger;

        /// <summary>
        /// Initializes new instance of FloorballTeamController class
        /// </summary>
        /// <param name="mediator">Mediator instance for handling commands and queries</param>
        /// <param name="logger">Logger instance for logging</param>
        public FloorballTeamController(IMediator mediator, ILogger<FloorballTeamController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Gets all floorball teams with pagination and filtering
        /// </summary>
        /// <param name="request">Query parameters for pagination and filtering</param>
        /// <returns>Paginated list of floorball teams</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballTeamDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballTeamDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FloorballTeamDto>>> GetAllTeams([FromQuery] GetFloorballTeamsRequest request)
        {
            _logger.LogInformation("Getting all floorball teams with pagination - Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

            Result<PagedResult<FloorballTeamDto>> result = await _mediator.Send(new GetAllFloorballTeamsQuery(
                request.Page,
                request.PageSize,
                request.ClubId,
                request.Division,
                request.TeamCategory));

            return HandlePaginatedResult(result, "Floorball teams retrieved successfully", "Failed to retrieve floorball teams");
        }

        /// <summary>
        /// Gets a floorball team by ID
        /// </summary>
        /// <param name="id">Team ID</param>
        /// <returns>Team details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamDto>>> GetTeamById(Guid id)
        {
            _logger.LogInformation("Getting floorball team with ID: {id}", id);

            Result<FloorballTeamDto> result = await _mediator.Send(new GetFloorballTeamByIdQuery(id));

            return HandleResult(result, "Floorball team retrieved successfully", "Failed to retrieve floorball team");
        }

        /// <summary>
        /// Gets all floorball teams for a specific club
        /// </summary>
        /// <param name="clubId">Club ID</param>
        /// <returns>List of teams belonging to the club</returns>
        [HttpGet("club/{clubId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballTeamDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballTeamDto>>>> GetTeamsByClub(Guid clubId)
        {
            _logger.LogInformation("Getting floorball teams for club with ID: {clubId}", clubId);

            Result<IEnumerable<FloorballTeamDto>> result = await _mediator.Send(new GetFloorballTeamsByClubQuery(clubId));

            return HandleListResult(result, "Floorball teams retrieved successfully", "Failed to retrieve floorball teams");
        }

        /// <summary>
        /// Gets all floorball teams in a specific division
        /// </summary>
        /// <param name="divisionId">Division</param>
        /// <returns>List of teams in the division</returns>
        [HttpGet("division/{divisionId}")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballTeamDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballTeamDto>>>> GetTeamsByDivision(Guid divisionId)
        {
            _logger.LogInformation("Getting floorball teams for division: {division}", divisionId);

            Result<IEnumerable<FloorballTeamDto>> result = await _mediator.Send(new GetFloorballTeamsByDivisionQuery(divisionId));

            return HandleListResult(result, "Floorball teams retrieved successfully", "Failed to retrieve floorball teams");
        }

        /// <summary>
        /// Gets floorball team names filtered by name
        /// </summary>
        /// <param name="nameFilter">Optional filter string to search team names</param>
        /// <returns>List of matching team names</returns>
        [HttpGet("names")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballTeamNameDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballTeamNameDto>>>> GetTeamNames([FromQuery] string? nameFilter)
        {
            _logger.LogInformation("Getting floorball team names filtered by: {nameFilter}", SanitizeForLog(nameFilter));

            Result<List<FloorballTeamNameDto>> result = await _mediator.Send(new GetTeamNamesQuery(nameFilter));

            return HandleResult(result, "Filtered team names retrieved successfully", "Failed to retrieve team names");
        }

        /// <summary>
        /// Gets all floorball teams without roster with pagination, search, and filtering
        /// </summary>
        /// <param name="request">Query parameters for pagination and filtering</param>
        /// <returns>Paginated list of floorball teams without roster</returns>
        [HttpGet("without-roster")]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballTeamSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballTeamSummaryDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballTeamSummaryDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FloorballTeamSummaryDto>>> GetAllTeamsWithoutRoster([FromQuery] GetAllTeamsWithoutRosterRequest request)
        {
            _logger.LogInformation(
                "Getting all floorball teams without roster - Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}, TeamCategory: {TeamCategory}",
                request.Page,
                request.PageSize,
                SanitizeForLog(request.SearchTerm),
                SanitizeForLog(request.TeamCategory));

            Result<PagedResult<FloorballTeamSummaryDto>> result = await _mediator.Send(new GetAllTeamsWithoutRosterQuery(
                request.Page,
                request.PageSize,
                request.SearchTerm,
                request.TeamCategory));

            return HandlePaginatedResult(result, "Floorball teams without roster retrieved successfully", "Failed to retrieve floorball teams without roster");
        }

        /// <summary>
        /// Creates a new floorball team
        /// </summary>
        /// <param name="request">Create team request</param>
        /// <returns>Created team details</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamDto>>> CreateTeam([FromBody] FloorballTeamRequest request)
        {
            _logger.LogInformation("Creating floorball team: {name}", SanitizeForLog(request.Name));

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Invalid model state for team creation: {errors}",
                    SanitizeForLog(string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
                return BadRequest(ApiResponse<FloorballTeamDto>.ErrorResponse(ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()));
            }

            Result<FloorballTeamDto> result = await _mediator.Send(new CreateFloorballTeamCommand(
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
                    ApiResponse<FloorballTeamDto>.SuccessResponse(result.Data, "Floorball team created successfully"));
            }

            return ToErrorResponse(result, "Failed to create floorball team");
        }

        /// <summary>
        /// Updates an existing floorball team
        /// </summary>
        /// <param name="id">Team ID</param>
        /// <param name="request">Update team request</param>
        /// <returns>Updated team details</returns>
        [HttpPut("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamDto>>> UpdateTeam(Guid id, [FromBody] FloorballTeamRequest request)
        {
            _logger.LogInformation("Updating floorball team with ID: {id}", id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Invalid model state for team update: {errors}",
                    SanitizeForLog(string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
                return BadRequest(ApiResponse<FloorballTeamDto>.ErrorResponse(ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()));
            }

            Result<FloorballTeamDto> result = await _mediator.Send(new UpdateFloorballTeamCommand(
                id,
                request.Name,
                request.DivisionId,
                request.HomeArena,
                request.PrimaryJerseyColor,
                request.Category,
                request.SecondaryJerseyColor,
                request.LogoUrl,
                request.ShortName));

            return HandleResult(result, "Floorball team updated successfully", "Failed to update floorball team");
        }

        /// <summary>
        /// Deletes a floorball team
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
            _logger.LogInformation("Deleting floorball team with ID: {id}", id);

            Result result = await _mediator.Send(new DeleteFloorballTeamCommand(id));

            return HandleVoidResult(result, "Floorball team deleted successfully", "Failed to delete floorball team");
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
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamDto>>> AddPlayerToTeam(
            Guid teamId,
            Guid playerId,
            [FromQuery] FloorballPosition position,
            [FromQuery] int? jerseyNumber = null,
            [FromQuery] int? requestedJerseyNumber = null)
        {
            _logger.LogInformation(
                "Adding player {playerId} to team {teamId} with position {position}",
                playerId,
                teamId,
                SanitizeForLog(position));

            Result<FloorballTeamDto> result = await _mediator.Send(new AddPlayerToTeamCommand(
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
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamDto>>> RemovePlayerFromTeam(Guid teamId, Guid playerId)
        {
            _logger.LogInformation("Removing player {playerId} from team {teamId}", playerId, teamId);

            Result<FloorballTeamDto> result = await _mediator.Send(new RemovePlayerFromTeamCommand(teamId, playerId));

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
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamPlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamPlayerDto>>> UpdateTeamPlayer(
            Guid teamId,
            Guid playerId,
            [FromBody] UpdateFloorballTeamPlayerRequest request)
        {
            _logger.LogInformation(
                "Updating player {playerId} in team {teamId} with position {position}, jersey {jerseyNumber}, active {isActive}",
                playerId,
                teamId,
                SanitizeForLog(request.Position),
                request.JerseyNumber,
                request.IsActive);

            Result<FloorballTeamPlayerDto> result = await _mediator.Send(new UpdateTeamPlayerCommand(
                teamId,
                playerId,
                request.Position,
                request.JerseyNumber,
                request.IsActive));

            return HandleResult(result, "Team player updated successfully", "Failed to update team player");
        }

        /// <summary>
        /// Updates the division of a floorball team
        /// </summary>
        /// <param name="teamId">The ID of the team to update</param>
        /// <param name="divisionId">The ID of the new division</param>
        /// <returns>Updated team details</returns>
        [HttpPatch("{teamId:guid}/division{divisionId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamDto>>> UpdateTeamDivision(Guid teamId, Guid divisionId)
        {
            _logger.LogInformation("Updating teams {teamId} into division {divisionId}", teamId, divisionId);

            Result<FloorballTeamDto> result = await _mediator.Send(new UpdateTeamDivisionCommand(teamId, divisionId));

            return HandleResult(result, "Team division updated succesfully", "Failed to update teams division");
        }

        /// <summary>
        /// Updates the logo of a floorball team
        /// </summary>
        /// <param name="id">The ID of the team to update</param>
        /// <param name="logoUrl">The new logo URL</param>
        /// <returns>Updated team details</returns>
        [HttpPatch("{id:guid}/logo")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamDto>>> UpdateTeamLogo(Guid id, [FromBody] string? logoUrl)
        {
            _logger.LogInformation("Updating logo for team {teamId}", id);

            Result<FloorballTeamDto> result = await _mediator.Send(new UpdateFloorballTeamLogoCommand(id, logoUrl));

            return HandleResult(result, "Team logo updated successfully", "Failed to update team logo");
        }
    }
}
