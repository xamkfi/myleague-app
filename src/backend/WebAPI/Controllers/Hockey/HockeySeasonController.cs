using Application.Common;
using Application.Features.Hockey.Competitions.Commands;
using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Seasons.Commands;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Seasons.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Hockey;

namespace WebAPI.Controllers.Hockey;

[Route("api/[controller]")]
public class HockeySeasonController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<HockeySeasonController> _logger;

    public HockeySeasonController(IMediator mediator, ILogger<HockeySeasonController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<HockeySeasonDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HockeySeasonDto>>>> GetAllSeasons()
    {
        Result<IEnumerable<HockeySeasonDto>> result = await _mediator.Send(new GetAllHockeySeasonsQuery());
        return HandleListResult(result, "Hockey seasons retrieved successfully", "Failed to retrieve hockey seasons");
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> GetSeasonById(Guid id)
    {
        Result<HockeySeasonDto> result = await _mediator.Send(new GetHockeySeasonByIdQuery(id));
        return HandleResult(result, "Hockey season retrieved successfully", "Hockey season not found");
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> CreateSeason([FromBody] CreateHockeySeasonRequest request)
    {
        CreateHockeySeasonCommand command = new(request.Name, request.StartDate, request.EndDate, request.SeasonCode);
        Result<HockeySeasonDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data is not null)
        {
            return CreatedAtAction(
                nameof(GetSeasonById),
                new { id = result.Data.Id },
                ApiResponse<HockeySeasonDto>.SuccessResponse(result.Data, "Hockey season created successfully"));
        }

        return HandleResult(result, "Hockey season created successfully", "Failed to create hockey season");
    }

    [HttpPost("{competitionId:guid}/teams")]
    [ProducesResponseType(typeof(ApiResponse<HockeyCompetitionTeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HockeyCompetitionTeamDto>>> AddTeam(
        Guid competitionId,
        [FromBody] AddTeamToHockeyCompetitionRequest request)
    {
        _logger.LogInformation("Adding team {TeamId} to hockey season {CompetitionId}", request.TeamId, competitionId);

        Result<HockeyCompetitionTeamDto> result = await _mediator.Send(
            new AddTeamToHockeyCompetitionCommand(competitionId, request.TeamId, request.Seed));

        return HandleResult(result, "Team added to hockey competition successfully", "Failed to add team to hockey competition");
    }
}
