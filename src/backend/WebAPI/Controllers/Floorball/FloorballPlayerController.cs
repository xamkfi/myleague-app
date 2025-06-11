using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Application.Commands.Floorball.Player;
using Application.Common;
using Application.DTOs.Floorball;
using Application.Queries.Floorball.Player;
using Domain.Enums.Floorball;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;
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
        /// Gets all floorball players
        /// </summary>
        /// <returns>List of all floorball players</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballPlayerDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballPlayerDto>>>> GetAllPlayers()
        {
            _logger.LogInformation("Getting all floorball players");

            GetAllFloorballPlayersQuery query = new GetAllFloorballPlayersQuery();
            Result<IEnumerable<FloorballPlayerDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<List<FloorballPlayerDto>>.SuccessResponse(result.Data.ToList(), "Floorball players retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve floorball players";
            return BadRequest(ApiResponse<List<FloorballPlayerDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Gets all active floorball players
        /// </summary>
        /// <returns>List of active floorball players</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballPlayerDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballPlayerDto>>>> GetActivePlayers()
        {
            _logger.LogInformation("Getting active floorball players");

            GetActiveFloorballPlayersQuery query = new GetActiveFloorballPlayersQuery();
            Result<IEnumerable<FloorballPlayerDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<List<FloorballPlayerDto>>.SuccessResponse(result.Data.ToList(), "Active floorball players retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve active floorball players";
            return BadRequest(ApiResponse<List<FloorballPlayerDto>>.ErrorResponse(errorMessage));
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
        [ProducesResponseType(typeof(ApiResponse<FloorballPlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballPlayerDto>>> CreatePlayer([FromBody] CreateFloorballPlayerRequest request)
        {
            _logger.LogInformation("Creating floorball player for person ID: {personId}", request.PersonId);

            CreateFloorballPlayerCommand command = new CreateFloorballPlayerCommand(
                request.PersonId,
                (FloorballPosition)Enum.Parse(typeof(FloorballPosition), request.Position));

            Result<FloorballPlayerDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballPlayerDto>.SuccessResponse(result.Data, "Floorball player created successfully"));
            }

            string errorMessage = result.Error ?? "Failed to create floorball player";
            return BadRequest(ApiResponse<FloorballPlayerDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Updates an existing floorball player
        /// </summary>
        /// <param name="id">Player ID</param>
        /// <param name="request">Update player request</param>
        /// <returns>Updated player details</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballPlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballPlayerDto>>> UpdatePlayer(Guid id, [FromBody] UpdateFloorballPlayerRequest request)
        {
            _logger.LogInformation("Updating floorball player with ID: {id}", id);

            UpdateFloorballPlayerCommand command = new UpdateFloorballPlayerCommand(
                id,
                (FloorballPosition)Enum.Parse(typeof(FloorballPosition), request.Position),
                request.IsActive);

            Result<FloorballPlayerDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballPlayerDto>.SuccessResponse(result.Data, "Floorball player updated successfully"));
            }

            string errorMessage = result.Error ?? "Failed to update floorball player";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballPlayerDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballPlayerDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Deletes a floorball player
        /// </summary>
        /// <param name="id">Player ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id:guid}")]
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
