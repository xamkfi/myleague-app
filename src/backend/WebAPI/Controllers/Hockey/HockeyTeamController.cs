using Application.Common;
using Application.Features.Hockey.Teams.Commands;
using Application.Features.Hockey.Teams.DTOs;
using Application.Features.Hockey.Teams.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Hockey;

namespace WebAPI.Controllers.Hockey;

[Route("api/[controller]")]
public class HockeyTeamController : BaseApiController
{
    private readonly IMediator _mediator;

    public HockeyTeamController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyTeamDto>>> GetTeamById(Guid id)
    {
        Result<HockeyTeamDto> result = await _mediator.Send(new GetHockeyTeamByIdQuery(id));
        return HandleResult(result, "Hockey team retrieved successfully", "Hockey team not found");
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HockeyTeamDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HockeyTeamDto>>> CreateTeam([FromBody] CreateHockeyTeamRequest request)
    {
        CreateHockeyTeamCommand command = new(
            request.Name,
            request.ClubId,
            request.TeamCategory,
            request.DivisionId,
            request.HomeArena,
            request.PrimaryJerseyColor,
            request.SecondaryJerseyColor,
            request.ShortName);

        Result<HockeyTeamDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data is not null)
        {
            return CreatedAtAction(
                nameof(GetTeamById),
                new { id = result.Data.Id },
                ApiResponse<HockeyTeamDto>.SuccessResponse(result.Data, "Hockey team created successfully"));
        }

        return HandleResult(result, "Hockey team created successfully", "Failed to create hockey team");
    }
}
