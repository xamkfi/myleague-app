using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Application.Commands.Floorball.Season;
using Application.Common;
using Application.DTOs.Floorball;
using Application.Queries.Floorball.Season;
using Domain.Enums.Floorball;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;
using WebAPI.Models.Floorball;

namespace WebAPI.Controllers.Floorball
{
    /// <summary>
    /// Controller for managing floorball seasons
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FloorballSeasonController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FloorballSeasonController> _logger;

        /// <summary>
        /// Initializes new instance of FloorballSeasonController class
        /// </summary>
        /// <param name="mediator">Mediator instance for handling commands and queries</param>
        /// <param name="logger">Logger instance for logging</param>
        public FloorballSeasonController(IMediator mediator, ILogger<FloorballSeasonController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Gets all floorball seasons
        /// </summary>
        /// <returns>List of all floorball seasons</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballSeasonDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballSeasonDto>>>> GetAllSeasons()
        {
            _logger.LogInformation("Getting all floorball seasons");

            GetAllFloorballSeasonsQuery query = new GetAllFloorballSeasonsQuery();
            Result<IEnumerable<FloorballSeasonDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<List<FloorballSeasonDto>>.SuccessResponse(result.Data.ToList(), "Floorball seasons retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve floorball seasons";
            return BadRequest(ApiResponse<List<FloorballSeasonDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Gets all active floorball seasons
        /// </summary>
        /// <returns>List of active floorball seasons</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballSeasonDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballSeasonDto>>>> GetActiveSeasons()
        {
            _logger.LogInformation("Getting active floorball seasons");

            GetActiveFloorballSeasonsQuery query = new GetActiveFloorballSeasonsQuery();
            Result<IEnumerable<FloorballSeasonDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<List<FloorballSeasonDto>>.SuccessResponse(result.Data.ToList(), "Active floorball seasons retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve active floorball seasons";
            return BadRequest(ApiResponse<List<FloorballSeasonDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Gets a floorball season by ID
        /// </summary>
        /// <param name="id">Season ID</param>
        /// <returns>Season details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonDto>>> GetSeasonById(Guid id)
        {
            _logger.LogInformation("Getting floorball season with ID: {id}", id);

            GetFloorballSeasonByIdQuery query = new GetFloorballSeasonByIdQuery(id);
            Result<FloorballSeasonDto> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballSeasonDto>.SuccessResponse(result.Data, "Floorball season retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve floorball season";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballSeasonDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballSeasonDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Gets floorball seasons by division
        /// </summary>
        /// <param name="divisionId">Division</param>
        /// <returns>List of seasons for the division</returns>
        [HttpGet("by-division/{division}")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballSeasonDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballSeasonDto>>>> GetSeasonsByDivision(Guid divisionId)
        {
            _logger.LogInformation("Getting floorball seasons for division: {division}", divisionId);

            GetFloorballSeasonsByDivisionQuery query = new GetFloorballSeasonsByDivisionQuery(divisionId);
            Result<IEnumerable<FloorballSeasonDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<List<FloorballSeasonDto>>.SuccessResponse(result.Data.ToList(), "Floorball seasons retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve floorball seasons";
            return BadRequest(ApiResponse<List<FloorballSeasonDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Creates a new floorball season
        /// </summary>
        /// <param name="request">Create season request</param>
        /// <returns>Created season details</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonDto>>> CreateSeason([FromBody] CreateFloorballSeasonRequest request)
        {
            _logger.LogInformation("Creating floorball season: {name}", request.Name);

            if (!DateTime.TryParse(request.StartDate, out DateTime startDate) || !DateTime.TryParse(request.EndDate, out DateTime endDate))
            {
                return BadRequest(ApiResponse<FloorballSeasonDto>.ErrorResponse("Invalid date format"));
            }

            CreateFloorballSeasonCommand command = new CreateFloorballSeasonCommand(
                request.Name,
                request.DivisionId,
                startDate,
                endDate
            );

            Result<FloorballSeasonDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetSeasonById),
                    new { id = result.Data.Id },
                    ApiResponse<FloorballSeasonDto>.SuccessResponse(result.Data, "Floorball season created successfully")
                );
            }

            string errorMessage = result.Error ?? "Failed to create floorball season";
            return BadRequest(ApiResponse<FloorballSeasonDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Updates an existing floorball season
        /// </summary>
        /// <param name="id">Season ID</param>
        /// <param name="request">Update season request</param>
        /// <returns>Updated season details</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonDto>>> UpdateSeason(Guid id, [FromBody] UpdateFloorballSeasonRequest request)
        {
            _logger.LogInformation("Updating floorball season with ID: {id}", id);

            if (!DateTime.TryParse(request.StartDate, out DateTime startDate) || !DateTime.TryParse(request.EndDate, out DateTime endDate))
            {
                return BadRequest(ApiResponse<FloorballSeasonDto>.ErrorResponse("Invalid date format"));
            }

            UpdateFloorballSeasonCommand command = new UpdateFloorballSeasonCommand(
                id,
                request.Name,
                startDate,
                endDate
            );

            Result<FloorballSeasonDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballSeasonDto>.SuccessResponse(result.Data, "Floorball season updated successfully"));
            }

            string errorMessage = result.Error ?? "Failed to update floorball season";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballSeasonDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballSeasonDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Activates a floorball season
        /// </summary>
        /// <param name="id">Season ID</param>
        /// <returns>Activated season details</returns>
        [HttpPut("{id:guid}/activate")]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonDto>>> ActivateSeason(Guid id)
        {
            _logger.LogInformation("Activating floorball season with ID: {id}", id);

            ActivateFloorballSeasonCommand command = new ActivateFloorballSeasonCommand(id);
            Result<FloorballSeasonDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballSeasonDto>.SuccessResponse(result.Data, "Floorball season activated successfully"));
            }

            string errorMessage = result.Error ?? "Failed to activate floorball season";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballSeasonDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballSeasonDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Deactivates a floorball season
        /// </summary>
        /// <param name="id">Season ID</param>
        /// <returns>Deactivated season details</returns>
        [HttpPut("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonDto>>> DeactivateSeason(Guid id)
        {
            _logger.LogInformation("Deactivating floorball season with ID: {id}", id);

            DeactivateFloorballSeasonCommand command = new DeactivateFloorballSeasonCommand(id);
            Result<FloorballSeasonDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballSeasonDto>.SuccessResponse(result.Data, "Floorball season deactivated successfully"));
            }

            string errorMessage = result.Error ?? "Failed to deactivate floorball season";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballSeasonDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballSeasonDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Completes a floorball season
        /// </summary>
        /// <param name="id">Season ID</param>
        /// <returns>Completed season details</returns>
        [HttpPut("{id:guid}/complete")]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonDto>>> CompleteSeason(Guid id)
        {
            _logger.LogInformation("Completing floorball season with ID: {id}", id);

            CompleteFloorballSeasonCommand command = new CompleteFloorballSeasonCommand(id);
            Result<FloorballSeasonDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballSeasonDto>.SuccessResponse(result.Data, "Floorball season completed successfully"));
            }

            string errorMessage = result.Error ?? "Failed to complete floorball season";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballSeasonDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballSeasonDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Adds a team to a floorball season
        /// </summary>
        /// <param name="seasonId">Season ID</param>
        /// <param name="teamId">Team ID</param>
        /// <returns>Updated season details</returns>
        [HttpPost("{seasonId:guid}/teams/{teamId:guid}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonDto>>> AddTeamToSeason(Guid seasonId, Guid teamId)
        {
            _logger.LogInformation("Adding team {teamId} to floorball season with ID: {id}", teamId, seasonId);

            AddTeamToSeasonCommand command = new AddTeamToSeasonCommand(seasonId, teamId);
            Result<FloorballSeasonDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballSeasonDto>.SuccessResponse(result.Data, "Team added to floorball season successfully"));
            }

            string errorMessage = result.Error ?? "Failed to add team to floorball season";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballSeasonDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballSeasonDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Removes a team from a floorball season
        /// </summary>
        /// <param name="seasonId">Season ID</param>
        /// <param name="teamId">Team ID</param>
        /// <returns>Updated season details</returns>
        [HttpDelete("{seasonId:guid}/teams/{teamId:guid}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonDto>>> RemoveTeamFromSeason(Guid seasonId, Guid teamId)
        {
            _logger.LogInformation("Removing team {teamId} from floorball season with ID: {id}", teamId, seasonId);

            RemoveTeamFromSeasonCommand command = new RemoveTeamFromSeasonCommand(seasonId, teamId);
            Result<FloorballSeasonDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballSeasonDto>.SuccessResponse(result.Data, "Team removed from floorball season successfully"));
            }

            string errorMessage = result.Error ?? "Failed to remove team from floorball season";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballSeasonDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballSeasonDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Deletes a floorball season
        /// </summary>
        /// <param name="id">Season ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteSeason(Guid id)
        {
            _logger.LogInformation("Deleting floorball season with ID: {id}", id);

            DeleteFloorballSeasonCommand command = new DeleteFloorballSeasonCommand(id);
            Result result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse.SuccessResponse("Floorball season deleted successfully"));
            }

            string errorMessage = result.Error ?? "Failed to delete floorball season";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse.ErrorResponse(errorMessage));
        }
    }
} 
