using Application.Common;
using Application.Features.Hockey.Competitions.Commands;
using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Tournaments.Commands;
using Application.Features.Hockey.Tournaments.DTOs;
using Application.Features.Hockey.Tournaments.Queries;
using Domain.Constants;
using Domain.Enums.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Hockey;

namespace WebAPI.Controllers.Hockey;

/// <summary>
/// API endpoints for hockey tournaments.
/// </summary>
[Route("api/[controller]")]
public class HockeyTournamentController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<HockeyTournamentController> _logger;

    /// <summary>
    /// Creates a new <see cref="HockeyTournamentController"/>.
    /// </summary>
    public HockeyTournamentController(IMediator mediator, ILogger<HockeyTournamentController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets all hockey tournaments.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<HockeyTournamentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HockeyTournamentDto>>>> GetAllTournaments(
        [FromQuery] TeamCategory? teamCategory = null,
        CancellationToken cancellationToken = default)
    {
        Result<IEnumerable<HockeyTournamentDto>> result = await _mediator.Send(new GetAllHockeyTournamentsQuery(teamCategory), cancellationToken);
        return HandleListResult(result, "Hockey tournaments retrieved successfully", "Failed to retrieve hockey tournaments");
    }

    /// <summary>
    /// Gets active hockey tournaments.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<List<HockeyTournamentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HockeyTournamentDto>>>> GetActiveTournaments(
        [FromQuery] TeamCategory? teamCategory = null,
        CancellationToken cancellationToken = default)
    {
        Result<IEnumerable<HockeyTournamentDto>> result = await _mediator.Send(new GetActiveHockeyTournamentsQuery(teamCategory), cancellationToken);
        return HandleListResult(result, "Active hockey tournaments retrieved successfully", "Failed to retrieve active hockey tournaments");
    }

    /// <summary>
    /// Gets a hockey tournament by id.
    /// </summary>
    /// <param name="id">Tournament id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> GetTournamentById(Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyTournamentDto> result = await _mediator.Send(new GetHockeyTournamentByIdQuery(id), cancellationToken);
        return HandleResult(result, "Hockey tournament retrieved successfully", "Hockey tournament not found");
    }

    /// <summary>
    /// Creates a new hockey tournament.
    /// </summary>
    /// <param name="request">Tournament create payload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> CreateTournament(
        [FromBody] CreateHockeyTournamentRequest request,
        CancellationToken cancellationToken = default)
    {
        CreateHockeyTournamentCommand command = new(
            request.Name,
            request.StartDate,
            request.EndDate,
            request.Venue,
            request.ContentHtml,
            request.TeamCategory);

        Result<HockeyTournamentDto> result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess && result.Data is not null)
        {
            return CreatedAtAction(
                nameof(GetTournamentById),
                new { id = result.Data.Id },
                ApiResponse<HockeyTournamentDto>.SuccessResponse(result.Data, "Hockey tournament created successfully"));
        }

        return HandleResult(result, "Hockey tournament created successfully", "Failed to create hockey tournament");
    }

    /// <summary>
    /// Updates hockey tournament details.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPut("{tournamentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> UpdateTournament(
        Guid tournamentId,
        [FromBody] UpdateHockeyTournamentRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyTournamentDto> result = await _mediator.Send(new UpdateHockeyTournamentCommand(
            tournamentId,
            request.Name,
            request.StartDate,
            request.EndDate,
            request.Venue,
            request.ContentHtml,
            request.TeamCategory), cancellationToken);

        return HandleResult(result, "Hockey tournament updated successfully", "Failed to update hockey tournament");
    }

    /// <summary>
    /// Updates hockey tournament rules.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPut("{tournamentId:guid}/rules")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> UpdateRules(
        Guid tournamentId,
        [FromBody] UpdateHockeyTournamentRulesRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyTournamentDto> result = await _mediator.Send(new UpdateHockeyTournamentRulesCommand(
            tournamentId,
            request.Format,
            request.HasGroupStage,
            request.HasPlayoffs,
            request.HasBronzeGame,
            request.HasPlacementGames,
            request.TeamsAdvancingPerGroup), cancellationToken);

        return HandleResult(result, "Hockey tournament rules updated successfully", "Failed to update hockey tournament rules");
    }

    /// <summary>
    /// Publishes a hockey tournament.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{tournamentId:guid}/publish")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> Publish(Guid tournamentId,
        CancellationToken cancellationToken = default) =>
        HandleResult(
            await _mediator.Send(new PublishHockeyTournamentCommand(tournamentId), cancellationToken),
            "Hockey tournament published successfully",
            "Failed to publish hockey tournament");

    /// <summary>
    /// Opens registration for a hockey tournament.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{tournamentId:guid}/open-registration")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> OpenRegistration(Guid tournamentId,
        CancellationToken cancellationToken = default) =>
        HandleResult(
            await _mediator.Send(new OpenHockeyTournamentRegistrationCommand(tournamentId), cancellationToken),
            "Hockey tournament registration opened successfully",
            "Failed to open hockey tournament registration");

    /// <summary>
    /// Activates a hockey tournament.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{tournamentId:guid}/activate")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> Activate(Guid tournamentId,
        CancellationToken cancellationToken = default) =>
        HandleResult(
            await _mediator.Send(new ActivateHockeyTournamentCommand(tournamentId), cancellationToken),
            "Hockey tournament activated successfully",
            "Failed to activate hockey tournament");

    /// <summary>
    /// Deactivates a hockey tournament.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{tournamentId:guid}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> Deactivate(Guid tournamentId,
        CancellationToken cancellationToken = default) =>
        HandleResult(
            await _mediator.Send(new DeactivateHockeyTournamentCommand(tournamentId), cancellationToken),
            "Hockey tournament deactivated successfully",
            "Failed to deactivate hockey tournament");

    /// <summary>
    /// Cancels a hockey tournament.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{tournamentId:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> Cancel(Guid tournamentId,
        CancellationToken cancellationToken = default) =>
        HandleResult(
            await _mediator.Send(new CancelHockeyTournamentCommand(tournamentId), cancellationToken),
            "Hockey tournament cancelled successfully",
            "Failed to cancel hockey tournament");

    /// <summary>
    /// Completes a hockey tournament.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{tournamentId:guid}/complete")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> Complete(Guid tournamentId,
        CancellationToken cancellationToken = default) =>
        HandleResult(
            await _mediator.Send(new CompleteHockeyTournamentCommand(tournamentId), cancellationToken),
            "Hockey tournament completed successfully",
            "Failed to complete hockey tournament");

    /// <summary>
    /// Starts the tournament group stage.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{tournamentId:guid}/start-group-stage")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> StartGroupStage(Guid tournamentId,
        CancellationToken cancellationToken = default) =>
        HandleResult(
            await _mediator.Send(new StartHockeyTournamentGroupStageCommand(tournamentId), cancellationToken),
            "Hockey tournament group stage started successfully",
            "Failed to start hockey tournament group stage");

    /// <summary>
    /// Starts the tournament playoff stage.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{tournamentId:guid}/start-playoff-stage")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> StartPlayoffStage(Guid tournamentId,
        CancellationToken cancellationToken = default) =>
        HandleResult(
            await _mediator.Send(new StartHockeyTournamentPlayoffStageCommand(tournamentId), cancellationToken),
            "Hockey tournament playoff stage started successfully",
            "Failed to start hockey tournament playoff stage");

    /// <summary>
    /// Advances the tournament to finals.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{tournamentId:guid}/advance-to-finals")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> AdvanceToFinals(Guid tournamentId,
        CancellationToken cancellationToken = default) =>
        HandleResult(
            await _mediator.Send(new AdvanceHockeyTournamentToFinalsCommand(tournamentId), cancellationToken),
            "Hockey tournament advanced to finals successfully",
            "Failed to advance hockey tournament to finals");

    /// <summary>
    /// Sets the tournament champion.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{tournamentId:guid}/champion")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> SetChampion(
        Guid tournamentId,
        [FromBody] SetHockeyTournamentChampionRequest request,
        CancellationToken cancellationToken = default) =>
        HandleResult(
            await _mediator.Send(new SetHockeyTournamentChampionCommand(tournamentId, request.ChampionCompetitionTeamId), cancellationToken),
            "Hockey tournament champion set successfully",
            "Failed to set hockey tournament champion");

    /// <summary>
    /// Adds a hockey team to a tournament competition.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{competitionId:guid}/teams")]
    [ProducesResponseType(typeof(ApiResponse<HockeyCompetitionTeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyCompetitionTeamDto>>> AddTeam(
        Guid competitionId,
        [FromBody] AddTeamToHockeyCompetitionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding team {TeamId} to hockey tournament {CompetitionId}", request.TeamId, competitionId);

        Result<HockeyCompetitionTeamDto> result = await _mediator.Send(
            new AddTeamToHockeyCompetitionCommand(competitionId, request.TeamId, request.Seed), cancellationToken);

        return HandleResult(result, "Team added to hockey competition successfully", "Failed to add team to hockey competition");
    }

    /// <summary>
    /// Removes a hockey team from a tournament competition.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpDelete("{tournamentId:guid}/teams/{teamId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> RemoveTeam(Guid tournamentId, Guid teamId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyCompetitionDto> commandResult = await _mediator.Send(
            new RemoveTeamFromHockeyCompetitionCommand(tournamentId, teamId), cancellationToken);
        if (commandResult.IsFailure)
        {
            return HandleResult(
                Result<HockeyTournamentDto>.Failure(
                    commandResult.Error ?? "Failed to remove team",
                    commandResult.Errors),
                "Team removed from hockey tournament successfully",
                "Failed to remove team from hockey tournament");
        }

        return HandleResult(
            await _mediator.Send(new GetHockeyTournamentByIdQuery(tournamentId), cancellationToken),
            "Team removed from hockey tournament successfully",
            "Failed to remove team from hockey tournament");
    }
    /// <summary>
    /// Creates a group (lohko) within a hockey tournament.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{tournamentId:guid}/groups")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> CreateGroup(
        Guid tournamentId,
        [FromBody] CreateHockeyTournamentGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyTournamentDto> result = await _mediator.Send(
            new CreateHockeyTournamentGroupCommand(tournamentId, request.Name), cancellationToken);

        return HandleResult(result, "Hockey tournament group created successfully", "Failed to create hockey tournament group");
    }

    /// <summary>
    /// Removes a group from a hockey tournament.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpDelete("{tournamentId:guid}/groups/{groupId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> RemoveGroup(Guid tournamentId, Guid groupId,
        CancellationToken cancellationToken = default) =>
        HandleResult(
            await _mediator.Send(new RemoveHockeyTournamentGroupCommand(tournamentId, groupId), cancellationToken),
            "Hockey tournament group removed successfully",
            "Failed to remove hockey tournament group");

    /// <summary>
    /// Adds a competition team to a hockey tournament group.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{tournamentId:guid}/groups/{groupId:guid}/teams")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> AddTeamToGroup(
        Guid tournamentId,
        Guid groupId,
        [FromBody] AddTeamToHockeyTournamentGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyTournamentDto> result = await _mediator.Send(
            new AddTeamToHockeyTournamentGroupCommand(
                tournamentId,
                groupId,
                request.CompetitionTeamId,
                request.Seed), cancellationToken);

        return HandleResult(result, "Team added to hockey tournament group successfully", "Failed to add team to hockey tournament group");
    }

    /// <summary>
    /// Removes a competition team from a hockey tournament group.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpDelete("{tournamentId:guid}/groups/{groupId:guid}/teams/{competitionTeamId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> RemoveTeamFromGroup(
        Guid tournamentId,
        Guid groupId,
        Guid competitionTeamId,
        CancellationToken cancellationToken = default) =>
        HandleResult(
            await _mediator.Send(new RemoveTeamFromHockeyTournamentGroupCommand(tournamentId, groupId, competitionTeamId), cancellationToken),
            "Team removed from hockey tournament group successfully",
            "Failed to remove team from hockey tournament group");

    /// <summary>
    /// Creates a playoff series on a hockey tournament.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{tournamentId:guid}/playoff-series")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> CreatePlayoffSeries(
        Guid tournamentId,
        [FromBody] CreateHockeyPlayoffSeriesRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyTournamentDto> result = await _mediator.Send(new CreateHockeyPlayoffSeriesCommand(
            tournamentId,
            request.Round,
            request.SeriesOrder,
            request.BestOf,
            request.HomeCompetitionTeamId,
            request.AwayCompetitionTeamId), cancellationToken);

        return HandleResult(result, "Playoff series created successfully", "Failed to create playoff series");
    }

    /// <summary>
    /// Assigns home/away teams to a playoff series.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{tournamentId:guid}/playoff-series/{seriesId:guid}/teams")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> AssignPlayoffSeriesTeams(
        Guid tournamentId,
        Guid seriesId,
        [FromBody] AssignHockeyPlayoffSeriesTeamsRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyTournamentDto> result = await _mediator.Send(new AssignHockeyPlayoffSeriesTeamsCommand(
            tournamentId,
            seriesId,
            request.HomeCompetitionTeamId,
            request.AwayCompetitionTeamId), cancellationToken);

        return HandleResult(result, "Playoff series teams assigned successfully", "Failed to assign playoff series teams");
    }

    /// <summary>
    /// Replaces the tournament playoff schedule.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPut("{tournamentId:guid}/playoff-schedule")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTournamentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyTournamentDto>>> SetPlayoffSchedule(
        Guid tournamentId,
        [FromBody] SetHockeyTournamentPlayoffScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyTournamentDto> result = await _mediator.Send(
            new SetHockeyTournamentPlayoffScheduleCommand(tournamentId, request.Slots), cancellationToken);

        return HandleResult(result, "Playoff schedule set successfully", "Failed to set playoff schedule");
    }
}
