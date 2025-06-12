using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Application.Commands.Floorball.Team;
using Application.Common;
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
        /// Gets all floorball teams
        /// </summary>
        /// <returns>List of all floorball teams</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballTeamDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<List<FloorballTeamDto>>>> GetAllTeams()
        {
            _logger.LogInformation("Getting all floorball teams");

            GetAllFloorballTeamsQuery query = new GetAllFloorballTeamsQuery();
            Result<IEnumerable<FloorballTeamDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<List<FloorballTeamDto>>.SuccessResponse(result.Data.ToList(), "Floorball teams retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve floorball teams";
            return BadRequest(ApiResponse<List<FloorballTeamDto>>.ErrorResponse(errorMessage));
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
        /// <param name="division">Division</param>
        /// <returns>List of teams in the division</returns>
        [HttpGet("division/{division}")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballTeamDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballTeamDto>>>> GetTeamsByDivision(string division)
        {
            _logger.LogInformation("Getting floorball teams for division: {division}", division);

            if (!Enum.TryParse<FloorballDivision>(division, true, out FloorballDivision divisionEnum))
            {
                _logger.LogWarning("Invalid division specified: {division}", division);
                return BadRequest(ApiResponse<List<FloorballTeamDto>>.ErrorResponse("Invalid division specified"));
            }

            GetFloorballTeamsByDivisionQuery query = new GetFloorballTeamsByDivisionQuery(divisionEnum);
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
                request.Division,
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
                request.Division,
                request.HomeArena,
                request.PrimaryJerseyColor,
                request.Category,
                request.SecondaryJerseyColor);

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
    }
} 
