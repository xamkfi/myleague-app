using Application.Common;
using Domain.Common;
using Application.Features.Floorball.Players.Commands;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Players.Queries;
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
    /// Controller for managing floorball players
    /// </summary>
    [Route("api/[controller]")]
    public class FloorballPlayerController : BaseApiController
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

            Result<PagedResult<FloorballPlayerDto>> result = await _mediator.Send(new GetAllFloorballPlayersQuery(
                request.Page,
                request.PageSize,
                request.IsActive,
                request.Position,
                request.TeamId,
                request.SearchTerm));

            return HandlePaginatedResult(result, "Floorball players retrieved successfully", "Failed to retrieve floorball players");
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

            Result<PagedResult<FloorballPlayerDto>> result = await _mediator.Send(new GetActiveFloorballPlayersQuery(
                request.Page,
                request.PageSize,
                request.Position,
                request.TeamId));

            return HandlePaginatedResult(result, "Active floorball players retrieved successfully", "Failed to retrieve active floorball players");
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

            Result<FloorballPlayerDto> result = await _mediator.Send(new GetFloorballPlayerByIdQuery(id));

            return HandleResult(result, "Floorball player retrieved successfully", "Failed to retrieve floorball player");
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

            Result<FloorballPlayerDto> result = await _mediator.Send(new CreateFloorballPlayerCommand(request.PersonId));

            return HandleResult(result, "Floorball player created successfully", "Failed to create floorball player");
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

            Result<FloorballPlayerDto> result = await _mediator.Send(new UpdateFloorballPlayerCommand(id, request.IsActive));

            return HandleResult(result, "Floorball player updated successfully", "Failed to update floorball player");
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

            Result<FloorballPlayerWithMatchesDto> result = await _mediator.Send(new GetFloorballPlayerMatchesQuery(id, limit));

            return HandleResult(result, "Player match history retrieved successfully", "Failed to retrieve player match history");
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

            Result result = await _mediator.Send(new DeleteFloorballPlayerCommand(id));

            return HandleVoidResult(result, "Floorball player deleted successfully", "Failed to delete floorball player");
        }
    }
}
