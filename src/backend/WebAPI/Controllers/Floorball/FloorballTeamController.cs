using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Application.Commands.Floorball.Team;
using Application.Common;
using Domain.Common;
using Application.DTOs.Floorball;
using Application.Queries.Floorball.Team;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAPI.Models.Floorball;
using WebAPI.Models.Common;
using Domain.Enums.Floorball;

namespace WebAPI.Controllers.Floorball
{
    /// <summary>
    /// Controller for managing floorball teams
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FloorballTeamController : ControllerBase
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
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FloorballTeamDto>>> GetAllTeams([FromQuery] GetFloorballTeamsRequest request)
        {
            _logger.LogInformation("Getting all floorball teams with pagination - Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

            var query = new GetAllFloorballTeamsQuery(
                request.Page,
                request.PageSize,
                request.ClubId,
                request.Division
            );

            Result<PagedResult<FloorballTeamDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(PaginatedApiResponse<FloorballTeamDto>.SuccessResponse(result.Data, "Floorball teams retrieved successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, PaginatedApiResponse<FloorballTeamDto>.ErrorResponse(errorMessage));
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

            GetFloorballTeamByIdQuery query = new GetFloorballTeamByIdQuery(id);
            Result<FloorballTeamDto> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTeamDto>.SuccessResponse(result.Data, "Floorball team retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve floorball team";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballTeamDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballTeamDto>.ErrorResponse(errorMessage));
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

            GetFloorballTeamsByClubQuery query = new GetFloorballTeamsByClubQuery(clubId);
            Result<IEnumerable<FloorballTeamDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<List<FloorballTeamDto>>.SuccessResponse(result.Data.ToList(), "Floorball teams retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve floorball teams";
            return BadRequest(ApiResponse<List<FloorballTeamDto>>.ErrorResponse(errorMessage));
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

            GetFloorballTeamsByDivisionQuery query = new GetFloorballTeamsByDivisionQuery(divisionId);
            Result<IEnumerable<FloorballTeamDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<List<FloorballTeamDto>>.SuccessResponse(result.Data.ToList(), "Floorball teams retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve floorball teams";
            return BadRequest(ApiResponse<List<FloorballTeamDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Creates a new floorball team
        /// </summary>
        /// <param name="request">Create team request</param>
        /// <returns>Created team details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamDto>>> CreateTeam([FromBody] FloorballTeamRequest request)
        {
            _logger.LogInformation("Creating floorball team: {name}", request.Name);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for team creation: {errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ApiResponse<FloorballTeamDto>.ErrorResponse(ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()));
            }

            CreateFloorballTeamCommand command = new CreateFloorballTeamCommand(
                request.Name,
                request.DivisionId,
                request.ClubId,
                request.HomeArena,
                request.PrimaryJerseyColor,
                request.Category,
                request.SecondaryJerseyColor);

            Result<FloorballTeamDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetTeamById),
                    new { id = result.Data.Id },
                    ApiResponse<FloorballTeamDto>.SuccessResponse(result.Data, "Floorball team created successfully")
                );
            }

            string errorMessage = result.Error ?? "Failed to create floorball team";
            return BadRequest(ApiResponse<FloorballTeamDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Updates an existing floorball team
        /// </summary>
        /// <param name="id">Team ID</param>
        /// <param name="request">Update team request</param>
        /// <returns>Updated team details</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamDto>>> UpdateTeam(Guid id, [FromBody] FloorballTeamRequest request)
        {
            _logger.LogInformation("Updating floorball team with ID: {id}", id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for team update: {errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ApiResponse<FloorballTeamDto>.ErrorResponse(ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()));
            }

            UpdateFloorballTeamCommand command = new UpdateFloorballTeamCommand(
                id,
                request.Name,
                request.DivisionId,
                request.HomeArena,
                request.PrimaryJerseyColor,
                request.Category,
                request.SecondaryJerseyColor,
                request.LogoUrl);

            Result<FloorballTeamDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTeamDto>.SuccessResponse(result.Data, "Floorball team updated successfully"));
            }

            string errorMessage = result.Error ?? "Failed to update floorball team";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballTeamDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballTeamDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Deletes a floorball team
        /// </summary>
        /// <param name="id">Team ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteTeam(Guid id)
        {
            _logger.LogInformation("Deleting floorball team with ID: {id}", id);

            DeleteFloorballTeamCommand command = new DeleteFloorballTeamCommand(id);
            Result result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse.SuccessResponse("Floorball team deleted successfully"));
            }

            string errorMessage = result.Error ?? "Failed to delete floorball team";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Adds a player to a team
        /// </summary>
        /// <param name="teamId">Team ID</param>
        /// <param name="playerId">Player ID</param>
        /// <param name="position">Player position</param>
        /// <param name="jerseyNumber">Player jersey number (optional)</param>
        /// <returns>Updated team details</returns>
        [HttpPost("{teamId:guid}/players/{playerId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamDto>>> AddPlayerToTeam(
            Guid teamId, 
            Guid playerId,
            [FromQuery] FloorballPosition position,
            [FromQuery] int? jerseyNumber = null)
        {
            _logger.LogInformation("Adding player {playerId} to team {teamId} with position {position}", 
                playerId, teamId, position);

            AddPlayerToTeamCommand command = new AddPlayerToTeamCommand(
                teamId,
                playerId,
                position,
                jerseyNumber);

            Result<FloorballTeamDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTeamDto>.SuccessResponse(result.Data, "Player added to team successfully"));
            }

            string errorMessage = result.Error ?? "Failed to add player to team";
            return BadRequest(ApiResponse<FloorballTeamDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Removes a player from a team
        /// </summary>
        /// <param name="teamId">Team ID</param>
        /// <param name="playerId">Player ID</param>
        /// <returns>Updated team details</returns>
        [HttpDelete("{teamId:guid}/players/{playerId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamDto>>> RemovePlayerFromTeam(Guid teamId, Guid playerId)
        {
            _logger.LogInformation("Removing player {playerId} from team {teamId}", playerId, teamId);

            RemovePlayerFromTeamCommand command = new RemovePlayerFromTeamCommand(teamId, playerId);
            Result<FloorballTeamDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTeamDto>.SuccessResponse(result.Data, "Player removed from team successfully"));
            }

            string errorMessage = result.Error ?? "Failed to remove player from team";
            return BadRequest(ApiResponse<FloorballTeamDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Updates a player's information in a team roster
        /// </summary>
        /// <param name="teamId">Team ID</param>
        /// <param name="playerId">Player ID</param>
        /// <param name="request">Update team player request (position, jersey number, active status)</param>
        /// <returns>Updated team player details</returns>
        [HttpPut("{teamId:guid}/players/{playerId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamPlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamPlayerDto>>> UpdateTeamPlayer(
            Guid teamId, 
            Guid playerId,
            [FromBody] UpdateFloorballTeamPlayerRequest request)
        {
            _logger.LogInformation("Updating player {playerId} in team {teamId} with position {position}, jersey {jerseyNumber}, active {isActive}", 
                playerId, teamId, request.Position, request.JerseyNumber, request.IsActive);

            UpdateTeamPlayerCommand command = new UpdateTeamPlayerCommand(
                teamId,
                playerId,
                request.Position,
                request.JerseyNumber,
                request.IsActive);

            Result<FloorballTeamPlayerDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTeamPlayerDto>.SuccessResponse(result.Data, "Team player updated successfully"));
            }

            string errorMessage = result.Error ?? "Failed to update team player";
            return BadRequest(ApiResponse<FloorballTeamPlayerDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Updates the division of a floorball team
        /// </summary>
        /// <param name="teamId">The ID of the team to update</param>
        /// <param name="divisionId">The ID of the new division</param>
        /// <returns>Updated team details</returns>
        [HttpPatch("{teamId:guid}/division{divisionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamDto>>> UpdateTeamDivision(Guid teamId, Guid divisionId)
        {
            _logger.LogInformation("Updating teams {teamId} into division {divisionId}", teamId, divisionId);

            UpdateTeamDivisionCommand command = new UpdateTeamDivisionCommand(teamId, divisionId);
            Result<FloorballTeamDto> result = await _mediator.Send(command);

            if(result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTeamDto>.SuccessResponse(result.Data, "Team division updated succesfully"));
            }

            string errorMessage = result.Error ?? "Failed to update teams division";
            return BadRequest(ApiResponse<FloorballTeamDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Updates the logo of a floorball team
        /// </summary>
        /// <param name="id">The ID of the team to update</param>
        /// <param name="logoUrl">The new logo URL</param>
        /// <returns>Updated team details</returns>
        [HttpPatch("{id:guid}/logo")]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamDto>>> UpdateTeamLogo(Guid id, [FromBody] string? logoUrl)
        {
            _logger.LogInformation("Updating logo for team {teamId}", id);

            UpdateFloorballTeamLogoCommand command = new UpdateFloorballTeamLogoCommand(id, logoUrl);
            Result<FloorballTeamDto> result = await _mediator.Send(command);

            if(result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTeamDto>.SuccessResponse(result.Data, "Team logo updated successfully"));
            }

            string errorMessage = result.Error ?? "Failed to update team logo";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballTeamDto>.ErrorResponse(errorMessage));
            }
            return BadRequest(ApiResponse<FloorballTeamDto>.ErrorResponse(errorMessage));
        }
    }
} 
