using Application.Common;
using Application.Features.Hockey.Players.Commands;
using Application.Features.Hockey.Players.DTOs;
using Application.Features.Hockey.Players.Queries;
using Domain.Common;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using WebAPI.Models.Hockey;

namespace WebAPI.Controllers.Hockey;

/// <summary>
/// API endpoints for hockey players.
/// </summary>
[Route("api/[controller]")]
public class HockeyPlayerController : BaseApiController
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Creates a new <see cref="HockeyPlayerController"/>.
    /// </summary>
    public HockeyPlayerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets paginated hockey players.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PaginatedApiResponse<HockeyPlayerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginatedApiResponse<HockeyPlayerDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedApiResponse<HockeyPlayerDto>>> GetPagedPlayers(
        [FromQuery] GetPagedHockeyPlayersRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<PagedResult<HockeyPlayerDto>> result = await _mediator.Send(
            new GetPagedHockeyPlayersQuery(
                request.Page,
                request.PageSize,
                request.SearchTerm,
                request.IsActive,
                request.Position,
                request.ClubId,
                request.TeamId,
                request.TeamCategory),
            cancellationToken);
        return HandlePaginatedResult(result, "Hockey players retrieved successfully", "Failed to retrieve hockey players");
    }

    /// <summary>
    /// Gets a hockey player by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyPlayerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyPlayerDto>>> GetPlayerById(Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyPlayerDto> result = await _mediator.Send(new GetHockeyPlayerByIdQuery(id), cancellationToken);
        return HandleResult(result, "Hockey player retrieved successfully", "Hockey player not found");
    }

    /// <summary>
    /// Creates a new hockey player profile.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HockeyPlayerDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HockeyPlayerDto>>> CreatePlayer([FromBody] CreateHockeyPlayerRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyPlayerDto> result = await _mediator.Send(new CreateHockeyPlayerCommand(
            request.PersonId,
            request.PrimaryPosition,
            request.Shoots,
            request.Catches,
            request.LicenseNumber), cancellationToken);

        if (result.IsSuccess && result.Data is not null)
        {
            return CreatedAtAction(
                nameof(GetPlayerById),
                new { id = result.Data.Id },
                ApiResponse<HockeyPlayerDto>.SuccessResponse(result.Data, "Hockey player created successfully"));
        }

        return HandleResult(result, "Hockey player created successfully", "Failed to create hockey player");
    }

    /// <summary>
    /// Deletes a hockey player.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeletePlayer(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        Result result = await _mediator.Send(new DeleteHockeyPlayerCommand(id), cancellationToken);
        return HandleVoidResult(result, "Hockey player deleted successfully", "Failed to delete hockey player");
    }
}
