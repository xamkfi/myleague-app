using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Application.Commands.Floorball.Player;
using Application.Common;
using Domain.Common;
using Application.DTOs.Floorball;
using Application.Queries.Floorball.Player;
using Domain.Enums.Floorball;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using WebAPI.Models.Floorball;

namespace WebAPI.Controllers.Floorball
{
    /// <summary>
    /// Controller for managing floorball players
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FloorballPlayerController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FloorballPlayerController> _logger;

        /// <summary>
        /// Initializes new instance of FloorballPlayerController class
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public FloorballPlayerController(IMediator mediator, ILogger<FloorballPlayerController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Gets all floorball players with pagination and filtering
        /// </summary>
        /// <param name="request">Query parameters for pagination and filtering</param>
        /// <returns>Paginated list of floorball players</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballPlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballPlayerDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballPlayerDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FloorballPlayerDto>>> GetAllPlayers([FromQuery] GetFloorballPlayersRequest request)
        {
            _logger.LogInformation("Getting all floorball players with pagination - Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

            GetAllFloorballPlayersQuery query = new GetAllFloorballPlayersQuery(
                request.Page,
                request.PageSize,
                request.IsActive,
                request.Position,
                request.TeamId,
                request.SearchTerm
            );

            Result<PagedResult<FloorballPlayerDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(PaginatedApiResponse<FloorballPlayerDto>.SuccessResponse(result.Data, "Floorball players retrieved successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, PaginatedApiResponse<FloorballPlayerDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Gets all active floorball players with pagination and filtering
        /// </summary>
        /// <param name="request">Query parameters for pagination and filtering</param>
        /// <returns>Paginated list of active floorball players</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballPlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballPlayerDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballPlayerDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FloorballPlayerDto>>> GetActivePlayers([FromQuery] GetActiveFloorballPlayersRequest request)
        {
            _logger.LogInformation("Getting active floorball players with pagination - Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

            GetActiveFloorballPlayersQuery query = new GetActiveFloorballPlayersQuery(
                request.Page,
                request.PageSize,
                request.Position,
                request.TeamId
            );

            Result<PagedResult<FloorballPlayerDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(PaginatedApiResponse<FloorballPlayerDto>.SuccessResponse(result.Data, "Active floorball players retrieved successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, PaginatedApiResponse<FloorballPlayerDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Gets a floorball player by ID
        /// </summary>
        /// <param name="id">Player ID</param>
        /// <returns>Player details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballPlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballPlayerDto>>> GetPlayerById(Guid id)
        {
            _logger.LogInformation("Getting floorball player with ID: {id}", id);

            GetFloorballPlayerByIdQuery query = new GetFloorballPlayerByIdQuery(id);
            Result<FloorballPlayerDto> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballPlayerDto>.SuccessResponse(result.Data, "Floorball player retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve floorball player";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballPlayerDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballPlayerDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Creates a new floorball player
        /// </summary>
        /// <param name="request">Create player request</param>
        /// <returns>Created player details</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballPlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballPlayerDto>>> CreatePlayer([FromBody] CreateFloorballPlayerRequest request)
        {
            _logger.LogInformation("Creating floorball player for person ID: {personId}", request.PersonId);

            CreateFloorballPlayerCommand command = new CreateFloorballPlayerCommand(request.PersonId);

            Result<FloorballPlayerDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballPlayerDto>.SuccessResponse(result.Data, "Floorball player created successfully"));
            }

            string errorMessage = result.Error ?? "Failed to create floorball player";
            List<string> errorList = result.ValidationFailures.Select(x => x.ErrorMessage).ToList();
            return BadRequest(ApiResponse<FloorballPlayerDto>.ErrorResponse(errorMessage, errorList));
        }

        /// <summary>
        /// Updates an existing floorball player
        /// </summary>
        /// <param name="id">Player ID</param>
        /// <param name="request">Update player request</param>
        /// <returns>Updated player details</returns>
        [HttpPut("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballPlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballPlayerDto>>> UpdatePlayer(Guid id, [FromBody] UpdateFloorballPlayerRequest request)
        {
            _logger.LogInformation("Updating floorball player with ID: {id}", id);

            UpdateFloorballPlayerCommand command = new UpdateFloorballPlayerCommand(
                id,
                request.IsActive);

            Result<FloorballPlayerDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballPlayerDto>.SuccessResponse(result.Data, "Floorball player updated successfully"));
            }

            string errorMessage = result.Error ?? "Failed to update floorball player";
            List<string> errorList = result.ValidationFailures.Select(x => x.ErrorMessage).ToList();

            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballPlayerDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballPlayerDto>.ErrorResponse(errorMessage, errorList));
        }

        /// <summary>
        /// Gets a floorball player's match history with performance statistics
        /// </summary>
        /// <param name="id">Player ID</param>
        /// <param name="limit">Maximum number of recent matches to return (default: 10, max: 50)</param>
        /// <returns>Player information with match history and performance statistics</returns>
        [HttpGet("{id:guid}/matches")]
        [ProducesResponseType(typeof(ApiResponse<FloorballPlayerWithMatchesDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballPlayerWithMatchesDto>>> GetPlayerMatches(
            Guid id,
            [FromQuery] int limit = 10)
        {
            _logger.LogInformation("Getting match history for floorball player with ID: {id}, limit: {limit}", id, limit);

            GetFloorballPlayerMatchesQuery query = new GetFloorballPlayerMatchesQuery(id, limit);
            Result<FloorballPlayerWithMatchesDto> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballPlayerWithMatchesDto>.SuccessResponse(result.Data, "Player match history retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve player match history";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballPlayerWithMatchesDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballPlayerWithMatchesDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Deletes a floorball player
        /// </summary>
        /// <param name="id">Player ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeletePlayer(Guid id)
        {
            _logger.LogInformation("Deleting floorball player with ID: {id}", id);

            DeleteFloorballPlayerCommand command = new DeleteFloorballPlayerCommand(id);
            Result result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse.SuccessResponse("Floorball player deleted successfully"));
            }

            string errorMessage = result.Error ?? "Failed to delete floorball player";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse.ErrorResponse(errorMessage));
        }
    }
} 
