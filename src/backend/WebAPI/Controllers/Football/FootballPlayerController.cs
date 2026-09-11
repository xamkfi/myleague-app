using Domain.Constants;
using Application.Common;
using Domain.Common;
using Application.Features.Football.Players.Commands;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Players.Queries;
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
    /// Controller for managing football players
    /// </summary>
    [Route("api/[controller]")]
    public class FootballPlayerController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FootballPlayerController> _logger;

        /// <summary>
        /// Initializes new instance of FootballPlayerController class
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public FootballPlayerController(IMediator mediator, ILogger<FootballPlayerController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Gets all football players with pagination and filtering
        /// </summary>
        /// <param name="request">Query parameters for pagination and filtering</param>
        /// <returns>Paginated list of football players</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedApiResponse<FootballPlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FootballPlayerDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FootballPlayerDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FootballPlayerDto>>> GetAllPlayers([FromQuery] GetFootballPlayersRequest request)
        {
            _logger.LogInformation("Getting all football players with pagination - Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

            Result<PagedResult<FootballPlayerDto>> result = await _mediator.Send(new GetAllFootballPlayersQuery(
                request.Page,
                request.PageSize,
                request.IsActive,
                request.Position,
                request.TeamId,
                request.SearchTerm));

            return HandlePaginatedResult(result, "Football players retrieved successfully", "Failed to retrieve football players");
        }

        /// <summary>
        /// Gets all active football players with pagination and filtering
        /// </summary>
        /// <param name="request">Query parameters for pagination and filtering</param>
        /// <returns>Paginated list of active football players</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(PaginatedApiResponse<FootballPlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FootballPlayerDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FootballPlayerDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FootballPlayerDto>>> GetActivePlayers([FromQuery] GetActiveFootballPlayersRequest request)
        {
            _logger.LogInformation("Getting active football players with pagination - Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

            Result<PagedResult<FootballPlayerDto>> result = await _mediator.Send(new GetActiveFootballPlayersQuery(
                request.Page,
                request.PageSize,
                request.Position,
                request.TeamId));

            return HandlePaginatedResult(result, "Active football players retrieved successfully", "Failed to retrieve active football players");
        }

        /// <summary>
        /// Gets a football player by ID
        /// </summary>
        /// <param name="id">Player ID</param>
        /// <returns>Player details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FootballPlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FootballPlayerDto>>> GetPlayerById(Guid id)
        {
            _logger.LogInformation("Getting football player with ID: {id}", id);

            Result<FootballPlayerDto> result = await _mediator.Send(new GetFootballPlayerByIdQuery(id));

            return HandleResult(result, "Football player retrieved successfully", "Failed to retrieve football player");
        }

        /// <summary>
        /// Creates a new football player
        /// </summary>
        /// <param name="request">Create player request</param>
        /// <returns>Created player details</returns>
        [HttpPost]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<FootballPlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FootballPlayerDto>>> CreatePlayer([FromBody] CreateFootballPlayerRequest request)
        {
            _logger.LogInformation("Creating football player for person ID: {personId}", request.PersonId);

            Result<FootballPlayerDto> result = await _mediator.Send(new CreateFootballPlayerCommand(request.PersonId));

            return HandleResult(result, "Football player created successfully", "Failed to create football player");
        }

        /// <summary>
        /// Updates an existing football player
        /// </summary>
        /// <param name="id">Player ID</param>
        /// <param name="request">Update player request</param>
        /// <returns>Updated player details</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<FootballPlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FootballPlayerDto>>> UpdatePlayer(Guid id, [FromBody] UpdateFootballPlayerRequest request)
        {
            _logger.LogInformation("Updating football player with ID: {id}", id);

            Result<FootballPlayerDto> result = await _mediator.Send(new UpdateFootballPlayerCommand(id, request.IsActive));

            return HandleResult(result, "Football player updated successfully", "Failed to update football player");
        }

        /// <summary>
        /// Gets a football player's match history with performance statistics
        /// </summary>
        /// <param name="id">Player ID</param>
        /// <param name="limit">Maximum number of recent matches to return (default: 10, max: 50)</param>
        /// <returns>Player information with match history and performance statistics</returns>
        [HttpGet("{id:guid}/matches")]
        [ProducesResponseType(typeof(ApiResponse<FootballPlayerWithMatchesDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FootballPlayerWithMatchesDto>>> GetPlayerMatches(
            Guid id,
            [FromQuery] int limit = 10)
        {
            _logger.LogInformation("Getting match history for football player with ID: {id}, limit: {limit}", id, limit);

            Result<FootballPlayerWithMatchesDto> result = await _mediator.Send(new GetFootballPlayerMatchesQuery(id, limit));

            return HandleResult(result, "Player match history retrieved successfully", "Failed to retrieve player match history");
        }

        /// <summary>
        /// Deletes a football player
        /// </summary>
        /// <param name="id">Player ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeletePlayer(Guid id)
        {
            _logger.LogInformation("Deleting football player with ID: {id}", id);

            Result result = await _mediator.Send(new DeleteFootballPlayerCommand(id));

            return HandleVoidResult(result, "Football player deleted successfully", "Failed to delete football player");
        }
    }
}
