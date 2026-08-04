using Application.Common;
using Application.Features.Hockey.Competitions.Commands;
using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Competitions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Hockey;

namespace WebAPI.Controllers.Hockey;

/// <summary>
/// Shared API endpoints for hockey competitions (season or tournament).
/// </summary>
[Route("api/[controller]")]
public class HockeyCompetitionController : BaseApiController
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Creates a new <see cref="HockeyCompetitionController"/>.
    /// </summary>
    public HockeyCompetitionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets a hockey competition by id.
    /// </summary>
    [HttpGet("{competitionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyCompetitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyCompetitionDto>>> GetById(Guid competitionId)
    {
        Result<HockeyCompetitionDto> result = await _mediator.Send(new GetHockeyCompetitionByIdQuery(competitionId));
        return HandleResult(result, "Hockey competition retrieved successfully", "Hockey competition not found");
    }

    /// <summary>
    /// Adds a hockey team to a competition.
    /// </summary>
    [HttpPost("{competitionId:guid}/teams")]
    [ProducesResponseType(typeof(ApiResponse<HockeyCompetitionTeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyCompetitionTeamDto>>> AddTeam(
        Guid competitionId,
        [FromBody] AddTeamToHockeyCompetitionRequest request)
    {
        Result<HockeyCompetitionTeamDto> result = await _mediator.Send(
            new AddTeamToHockeyCompetitionCommand(competitionId, request.TeamId, request.Seed));

        return HandleResult(result, "Team added to hockey competition successfully", "Failed to add team to hockey competition");
    }

    /// <summary>
    /// Removes a hockey team from a competition.
    /// </summary>
    [HttpDelete("{competitionId:guid}/teams/{teamId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyCompetitionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyCompetitionDto>>> RemoveTeam(Guid competitionId, Guid teamId) =>
        HandleResult(
            await _mediator.Send(new RemoveTeamFromHockeyCompetitionCommand(competitionId, teamId)),
            "Team removed from hockey competition successfully",
            "Failed to remove team from hockey competition");

    /// <summary>
    /// Adds a Common Division link to a competition.
    /// </summary>
    [HttpPost("{competitionId:guid}/divisions")]
    [ProducesResponseType(typeof(ApiResponse<HockeyCompetitionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyCompetitionDto>>> CreateDivision(
        Guid competitionId,
        [FromBody] AddDivisionToHockeySeasonRequest request)
    {
        Result<HockeyCompetitionDto> result = await _mediator.Send(new CreateHockeyCompetitionDivisionCommand(
            competitionId,
            request.DivisionId,
            request.Name,
            request.SortOrder));

        return HandleResult(result, "Division added to hockey competition successfully", "Failed to add division to hockey competition");
    }

    /// <summary>
    /// Soft-removes a competition division.
    /// </summary>
    [HttpDelete("{competitionId:guid}/divisions/{competitionDivisionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyCompetitionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyCompetitionDto>>> RemoveDivision(
        Guid competitionId,
        Guid competitionDivisionId) =>
        HandleResult(
            await _mediator.Send(new RemoveHockeyCompetitionDivisionCommand(competitionId, competitionDivisionId)),
            "Division removed from hockey competition successfully",
            "Failed to remove division from hockey competition");

    /// <summary>
    /// Places a competition team into a division.
    /// </summary>
    [HttpPost("{competitionId:guid}/divisions/{competitionDivisionId:guid}/teams")]
    [ProducesResponseType(typeof(ApiResponse<HockeyCompetitionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyCompetitionDto>>> AddTeamToDivision(
        Guid competitionId,
        Guid competitionDivisionId,
        [FromBody] AddTeamToHockeySeasonDivisionRequest request)
    {
        Result<HockeyCompetitionDto> result = await _mediator.Send(new AddTeamToHockeyCompetitionDivisionCommand(
            competitionId,
            competitionDivisionId,
            request.CompetitionTeamId,
            request.Seed));

        return HandleResult(result, "Team added to hockey competition division successfully", "Failed to add team to division");
    }

    /// <summary>
    /// Soft-removes a competition team from a division.
    /// </summary>
    [HttpDelete("{competitionId:guid}/divisions/{competitionDivisionId:guid}/teams/{competitionTeamId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyCompetitionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyCompetitionDto>>> RemoveTeamFromDivision(
        Guid competitionId,
        Guid competitionDivisionId,
        Guid competitionTeamId) =>
        HandleResult(
            await _mediator.Send(new RemoveTeamFromHockeyCompetitionDivisionCommand(
                competitionId,
                competitionDivisionId,
                competitionTeamId)),
            "Team removed from hockey competition division successfully",
            "Failed to remove team from division");

    /// <summary>
    /// Updates competition rules metadata (nested match/standing/roster use Domain defaults).
    /// </summary>
    [HttpPut("{competitionId:guid}/rules")]
    [ProducesResponseType(typeof(ApiResponse<HockeyCompetitionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyCompetitionDto>>> UpdateRules(
        Guid competitionId,
        [FromBody] UpdateHockeyCompetitionRulesRequest request)
    {
        Result<HockeyCompetitionDto> result = await _mediator.Send(new UpdateHockeyCompetitionRulesCommand(
            competitionId,
            request.Name,
            request.RuleBookVersion,
            request.RuleBookSource));

        return HandleResult(result, "Hockey competition rules updated successfully", "Failed to update hockey competition rules");
    }
}
