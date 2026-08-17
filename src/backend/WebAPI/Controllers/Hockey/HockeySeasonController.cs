using Application.Common;
using Application.Features.Hockey.Competitions.Commands;
using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Seasons.Commands;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Seasons.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Hockey;

namespace WebAPI.Controllers.Hockey;

/// <summary>
/// API endpoints for hockey seasons.
/// </summary>
[Route("api/[controller]")]
public class HockeySeasonController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<HockeySeasonController> _logger;

    /// <summary>
    /// Creates a new <see cref="HockeySeasonController"/>.
    /// </summary>
    public HockeySeasonController(IMediator mediator, ILogger<HockeySeasonController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets all hockey seasons.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<HockeySeasonDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HockeySeasonDto>>>> GetAllSeasons(CancellationToken cancellationToken = default)
    {
        Result<IEnumerable<HockeySeasonDto>> result = await _mediator.Send(new GetAllHockeySeasonsQuery(), cancellationToken);
        return HandleListResult(result, "Hockey seasons retrieved successfully", "Failed to retrieve hockey seasons");
    }

    /// <summary>
    /// Gets active hockey seasons.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<List<HockeySeasonDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HockeySeasonDto>>>> GetActiveSeasons(CancellationToken cancellationToken = default)
    {
        Result<IEnumerable<HockeySeasonDto>> result = await _mediator.Send(new GetActiveHockeySeasonsQuery(), cancellationToken);
        return HandleListResult(result, "Active hockey seasons retrieved successfully", "Failed to retrieve active hockey seasons");
    }

    /// <summary>
    /// Gets a hockey season by id.
    /// </summary>
    /// <param name="id">Season id</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> GetSeasonById(Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<HockeySeasonDto> result = await _mediator.Send(new GetHockeySeasonByIdQuery(id), cancellationToken);
        return HandleResult(result, "Hockey season retrieved successfully", "Hockey season not found");
    }

    /// <summary>
    /// Creates a new hockey season.
    /// </summary>
    /// <param name="request">Season create payload</param>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> CreateSeason([FromBody] CreateHockeySeasonRequest request,
        CancellationToken cancellationToken = default)
    {
        CreateHockeySeasonCommand command = new(request.Name, request.StartDate, request.EndDate, request.SeasonCode);
        Result<HockeySeasonDto> result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess && result.Data is not null)
        {
            return CreatedAtAction(
                nameof(GetSeasonById),
                new { id = result.Data.Id },
                ApiResponse<HockeySeasonDto>.SuccessResponse(result.Data, "Hockey season created successfully"));
        }

        return HandleResult(result, "Hockey season created successfully", "Failed to create hockey season");
    }

    /// <summary>
    /// Updates hockey season details.
    /// </summary>
    [Authorize]
    [HttpPut("{seasonId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> UpdateSeason(
        Guid seasonId,
        [FromBody] UpdateHockeySeasonRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeySeasonDto> result = await _mediator.Send(new UpdateHockeySeasonCommand(
            seasonId,
            request.Name,
            request.StartDate,
            request.EndDate,
            request.SeasonCode), cancellationToken);

        return HandleResult(result, "Hockey season updated successfully", "Failed to update hockey season");
    }

    /// <summary>
    /// Publishes a hockey season.
    /// </summary>
    [Authorize]
    [HttpPost("{seasonId:guid}/publish")]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> Publish(Guid seasonId,
        CancellationToken cancellationToken = default) =>
        HandleResult(
            await _mediator.Send(new PublishHockeySeasonCommand(seasonId), cancellationToken),
            "Hockey season published successfully",
            "Failed to publish hockey season");

    /// <summary>
    /// Opens registration for a hockey season.
    /// </summary>
    [Authorize]
    [HttpPost("{seasonId:guid}/open-registration")]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> OpenRegistration(Guid seasonId,
        CancellationToken cancellationToken = default) =>
        HandleResult(
            await _mediator.Send(new OpenHockeySeasonRegistrationCommand(seasonId), cancellationToken),
            "Hockey season registration opened successfully",
            "Failed to open hockey season registration");

    /// <summary>
    /// Activates a hockey season.
    /// </summary>
    [Authorize]
    [HttpPost("{seasonId:guid}/activate")]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> Activate(Guid seasonId,
        CancellationToken cancellationToken = default) =>
        HandleResult(
            await _mediator.Send(new ActivateHockeySeasonCommand(seasonId), cancellationToken),
            "Hockey season activated successfully",
            "Failed to activate hockey season");

    /// <summary>
    /// Deactivates a hockey season.
    /// </summary>
    [Authorize]
    [HttpPost("{seasonId:guid}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> Deactivate(Guid seasonId,
        CancellationToken cancellationToken = default) =>
        HandleResult(
            await _mediator.Send(new DeactivateHockeySeasonCommand(seasonId), cancellationToken),
            "Hockey season deactivated successfully",
            "Failed to deactivate hockey season");

    /// <summary>
    /// Cancels a hockey season.
    /// </summary>
    [Authorize]
    [HttpPost("{seasonId:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> Cancel(Guid seasonId,
        CancellationToken cancellationToken = default) =>
        HandleResult(
            await _mediator.Send(new CancelHockeySeasonCommand(seasonId), cancellationToken),
            "Hockey season cancelled successfully",
            "Failed to cancel hockey season");

    /// <summary>
    /// Completes a hockey season.
    /// </summary>
    [Authorize]
    [HttpPost("{seasonId:guid}/complete")]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> Complete(Guid seasonId,
        CancellationToken cancellationToken = default) =>
        HandleResult(
            await _mediator.Send(new CompleteHockeySeasonCommand(seasonId), cancellationToken),
            "Hockey season completed successfully",
            "Failed to complete hockey season");

    /// <summary>
    /// Sets the season champion (season must already be completed).
    /// </summary>
    [Authorize]
    [HttpPost("{seasonId:guid}/champion")]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> SetChampion(
        Guid seasonId,
        [FromBody] SetHockeySeasonChampionRequest request,
        CancellationToken cancellationToken = default) =>
        HandleResult(
            await _mediator.Send(new SetHockeySeasonChampionCommand(seasonId, request.ChampionCompetitionTeamId), cancellationToken),
            "Hockey season champion set successfully",
            "Failed to set hockey season champion");

    /// <summary>
    /// Adds a hockey team to a season competition.
    /// </summary>
    /// <param name="competitionId">Season (competition) id</param>
    /// <param name="request">Team id and optional seed</param>
    [Authorize]
    [HttpPost("{competitionId:guid}/teams")]
    [ProducesResponseType(typeof(ApiResponse<HockeyCompetitionTeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HockeyCompetitionTeamDto>>> AddTeam(
        Guid competitionId,
        [FromBody] AddTeamToHockeyCompetitionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding team {TeamId} to hockey season {CompetitionId}", request.TeamId, competitionId);

        Result<HockeyCompetitionTeamDto> result = await _mediator.Send(
            new AddTeamToHockeyCompetitionCommand(competitionId, request.TeamId, request.Seed), cancellationToken);

        return HandleResult(result, "Team added to hockey competition successfully", "Failed to add team to hockey competition");
    }

    /// <summary>
    /// Removes a hockey team from a season competition.
    /// </summary>
    [Authorize]
    [HttpDelete("{seasonId:guid}/teams/{teamId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> RemoveTeam(Guid seasonId, Guid teamId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyCompetitionDto> commandResult = await _mediator.Send(
            new RemoveTeamFromHockeyCompetitionCommand(seasonId, teamId), cancellationToken);
        if (commandResult.IsFailure)
        {
            return HandleResult(
                Result<HockeySeasonDto>.Failure(commandResult.Error ?? "Failed to remove team", commandResult.Errors),
                "Team removed from hockey season successfully",
                "Failed to remove team from hockey season");
        }

        return HandleResult(
            await _mediator.Send(new GetHockeySeasonByIdQuery(seasonId), cancellationToken),
            "Team removed from hockey season successfully",
            "Failed to remove team from hockey season");
    }

    /// <summary>
    /// Adds a Common Division link to a hockey season.
    /// </summary>
    [Authorize]
    [HttpPost("{seasonId:guid}/divisions")]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> AddDivision(
        Guid seasonId,
        [FromBody] AddDivisionToHockeySeasonRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyCompetitionDto> commandResult = await _mediator.Send(new CreateHockeyCompetitionDivisionCommand(
            seasonId,
            request.DivisionId,
            request.Name,
            request.SortOrder), cancellationToken);
        if (commandResult.IsFailure)
        {
            return HandleResult(
                Result<HockeySeasonDto>.Failure(commandResult.Error ?? "Failed to add division", commandResult.Errors),
                "Division added to hockey season successfully",
                "Failed to add division to hockey season");
        }

        return HandleResult(
            await _mediator.Send(new GetHockeySeasonByIdQuery(seasonId), cancellationToken),
            "Division added to hockey season successfully",
            "Failed to add division to hockey season");
    }

    /// <summary>
    /// Soft-removes a competition division from a hockey season.
    /// </summary>
    [Authorize]
    [HttpDelete("{seasonId:guid}/divisions/{competitionDivisionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> RemoveDivision(
        Guid seasonId,
        Guid competitionDivisionId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyCompetitionDto> commandResult = await _mediator.Send(
            new RemoveHockeyCompetitionDivisionCommand(seasonId, competitionDivisionId), cancellationToken);
        if (commandResult.IsFailure)
        {
            return HandleResult(
                Result<HockeySeasonDto>.Failure(commandResult.Error ?? "Failed to remove division", commandResult.Errors),
                "Division removed from hockey season successfully",
                "Failed to remove division from hockey season");
        }

        return HandleResult(
            await _mediator.Send(new GetHockeySeasonByIdQuery(seasonId), cancellationToken),
            "Division removed from hockey season successfully",
            "Failed to remove division from hockey season");
    }

    /// <summary>
    /// Places a competition team into a season division.
    /// </summary>
    [Authorize]
    [HttpPost("{seasonId:guid}/divisions/{competitionDivisionId:guid}/teams")]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> AddTeamToDivision(
        Guid seasonId,
        Guid competitionDivisionId,
        [FromBody] AddTeamToHockeySeasonDivisionRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyCompetitionDto> commandResult = await _mediator.Send(new AddTeamToHockeyCompetitionDivisionCommand(
            seasonId,
            competitionDivisionId,
            request.CompetitionTeamId,
            request.Seed), cancellationToken);
        if (commandResult.IsFailure)
        {
            return HandleResult(
                Result<HockeySeasonDto>.Failure(commandResult.Error ?? "Failed to add team to division", commandResult.Errors),
                "Team added to hockey season division successfully",
                "Failed to add team to hockey season division");
        }

        return HandleResult(
            await _mediator.Send(new GetHockeySeasonByIdQuery(seasonId), cancellationToken),
            "Team added to hockey season division successfully",
            "Failed to add team to hockey season division");
    }

    /// <summary>
    /// Soft-removes a competition team from a season division.
    /// </summary>
    [Authorize]
    [HttpDelete("{seasonId:guid}/divisions/{competitionDivisionId:guid}/teams/{competitionTeamId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> RemoveTeamFromDivision(
        Guid seasonId,
        Guid competitionDivisionId,
        Guid competitionTeamId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyCompetitionDto> commandResult = await _mediator.Send(
            new RemoveTeamFromHockeyCompetitionDivisionCommand(
                seasonId,
                competitionDivisionId,
                competitionTeamId), cancellationToken);
        if (commandResult.IsFailure)
        {
            return HandleResult(
                Result<HockeySeasonDto>.Failure(commandResult.Error ?? "Failed to remove team from division", commandResult.Errors),
                "Team removed from hockey season division successfully",
                "Failed to remove team from hockey season division");
        }

        return HandleResult(
            await _mediator.Send(new GetHockeySeasonByIdQuery(seasonId), cancellationToken),
            "Team removed from hockey season division successfully",
            "Failed to remove team from hockey season division");
    }
    /// <summary>
    /// Creates a playoff series on a hockey season.
    /// </summary>
    [Authorize]
    [HttpPost("{seasonId:guid}/playoff-series")]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> CreatePlayoffSeries(
        Guid seasonId,
        [FromBody] CreateHockeyPlayoffSeriesRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeySeasonDto> result = await _mediator.Send(new CreateHockeySeasonPlayoffSeriesCommand(
            seasonId,
            request.Round,
            request.SeriesOrder,
            request.BestOf,
            request.HomeCompetitionTeamId,
            request.AwayCompetitionTeamId), cancellationToken);

        return HandleResult(result, "Playoff series created successfully", "Failed to create playoff series");
    }

    /// <summary>
    /// Assigns home/away teams to a season playoff series.
    /// </summary>
    [Authorize]
    [HttpPost("{seasonId:guid}/playoff-series/{seriesId:guid}/teams")]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> AssignPlayoffSeriesTeams(
        Guid seasonId,
        Guid seriesId,
        [FromBody] AssignHockeyPlayoffSeriesTeamsRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeySeasonDto> result = await _mediator.Send(new AssignHockeySeasonPlayoffSeriesTeamsCommand(
            seasonId,
            seriesId,
            request.HomeCompetitionTeamId,
            request.AwayCompetitionTeamId), cancellationToken);

        return HandleResult(result, "Playoff series teams assigned successfully", "Failed to assign playoff series teams");
    }

    /// <summary>
    /// Replaces the season playoff schedule.
    /// </summary>
    [Authorize]
    [HttpPut("{seasonId:guid}/playoff-schedule")]
    [ProducesResponseType(typeof(ApiResponse<HockeySeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeySeasonDto>>> SetPlayoffSchedule(
        Guid seasonId,
        [FromBody] SetHockeyTournamentPlayoffScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeySeasonDto> result = await _mediator.Send(
            new SetHockeySeasonPlayoffScheduleCommand(seasonId, request.Slots), cancellationToken);

        return HandleResult(result, "Playoff schedule set successfully", "Failed to set playoff schedule");
    }
}
