using Application.Common;
using Application.Features.Hockey.Competitions.Commands;
using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Tournaments.Commands;
using Application.Features.Hockey.Tournaments.DTOs;
using Application.Features.Hockey.Tournaments.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Hockey;

namespace WebAPI.Controllers.Hockey;

[Route("api/[controller]")]
public class HockeyTournamentController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<HockeyTournamentController> _logger;

    public HockeyTournamentController(IMediator mediator, ILogger<HockeyTournamentController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<HockeyTournamentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HockeyTournamentDto>>>> GetAllTournaments()
    {
        Result<IEnumerable<HockeyTournamentDto>> result = await _mediator.Send(new GetAllHockeyTournamentsQuery());
        return HandleListResult(result, "Hockey tournaments retrieved successfully", "Failed to retrieve hockey tournaments");
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> GetTournamentById(Guid id)
    {
        Result<HockeyTournamentDto> result = await _mediator.Send(new GetHockeyTournamentByIdQuery(id));
        return HandleResult(result, "Hockey tournament retrieved successfully", "Hockey tournament not found");
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> CreateTournament(
        [FromBody] CreateHockeyTournamentRequest request)
    {
        CreateHockeyTournamentCommand command = new(
            request.Name,
            request.StartDate,
            request.EndDate,
            request.Venue,
            request.ContentHtml);

        Result<HockeyTournamentDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data is not null)
        {
            return CreatedAtAction(
                nameof(GetTournamentById),
                new { id = result.Data.Id },
                ApiResponse<HockeyTournamentDto>.SuccessResponse(result.Data, "Hockey tournament created successfully"));
        }

        return HandleResult(result, "Hockey tournament created successfully", "Failed to create hockey tournament");
    }

    [HttpPost("{competitionId:guid}/teams")]
    [ProducesResponseType(typeof(ApiResponse<HockeyCompetitionTeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HockeyCompetitionTeamDto>>> AddTeam(
        Guid competitionId,
        [FromBody] AddTeamToHockeyCompetitionRequest request)
    {
        _logger.LogInformation("Adding team {TeamId} to hockey tournament {CompetitionId}", request.TeamId, competitionId);

        Result<HockeyCompetitionTeamDto> result = await _mediator.Send(
            new AddTeamToHockeyCompetitionCommand(competitionId, request.TeamId, request.Seed));

        return HandleResult(result, "Team added to hockey competition successfully", "Failed to add team to hockey competition");
    }
}
