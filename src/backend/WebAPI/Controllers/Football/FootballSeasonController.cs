using Domain.Constants;
using Application.Common;
using Application.Features.Football.Seasons.Commands;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Seasons.Queries;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using WebAPI.Models.Football;

namespace WebAPI.Controllers.Football;

/// <summary>
/// Controller for managing football seasons
/// </summary>
[Route("api/[controller]")]
public class FootballSeasonController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<FootballSeasonController> _logger;

    /// <summary>
    /// Initializes a new instance of the FootballSeasonController class
    /// </summary>
    public FootballSeasonController(IMediator mediator, ILogger<FootballSeasonController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all seasons
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<FootballSeasonDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<FootballSeasonDto>>>> GetAllSeasons()
    {
        Result<IEnumerable<FootballSeasonDto>> result = await _mediator.Send(new GetAllFootballSeasonsQuery());
        return HandleListResult(result, "Football seasons retrieved successfully", "Failed to retrieve football seasons");
    }

    /// <summary>
    /// Get season years
    /// </summary>
    [HttpGet("years")]
    [ProducesResponseType(typeof(ApiResponse<List<FootballSeasonYearDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<FootballSeasonYearDto>>>> GetSeasonYears()
    {
        Result<IEnumerable<FootballSeasonYearDto>> result = await _mediator.Send(new GetFootballSeasonYearsQuery());
        return HandleListResult(result, "Football season years retrieved successfully", "Failed to retrieve football season years");
    }

    /// <summary>
    /// Get seasons paged
    /// </summary>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PaginatedApiResponse<FootballSeasonSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginatedApiResponse<FootballSeasonSummaryDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PaginatedApiResponse<FootballSeasonSummaryDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedApiResponse<FootballSeasonSummaryDto>>> GetSeasonsPaged(
        [FromQuery] GetFootballSeasonsPagedRequest request)
    {
        GetFootballSeasonsPagedQuery query = new(
            request.Page,
            request.PageSize,
            request.SeasonYear,
            request.TeamCategory);

        Result<PagedResult<FootballSeasonSummaryDto>> result = await _mediator.Send(query);
        return HandlePaginatedResult(result, "Football seasons retrieved successfully", "Failed to retrieve football seasons");
    }

    /// <summary>
    /// Get active seasons
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<List<FootballSeasonDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<FootballSeasonDto>>>> GetActiveSeasons()
    {
        Result<IEnumerable<FootballSeasonDto>> result = await _mediator.Send(new GetActiveFootballSeasonsQuery());
        return HandleListResult(result, "Active football seasons retrieved successfully", "Failed to retrieve active football seasons");
    }

    /// <summary>
    /// Get season by id
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FootballSeasonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballSeasonDto>>> GetSeasonById(Guid id)
    {
        Result<FootballSeasonDto> result = await _mediator.Send(new GetFootballSeasonByIdQuery(id));
        return HandleResult(result, "Football season retrieved successfully", "Failed to retrieve football season");
    }

    /// <summary>
    /// Get seasons by division
    /// </summary>
    [HttpGet("by-division/{division}")]
    [ProducesResponseType(typeof(ApiResponse<List<FootballSeasonDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<FootballSeasonDto>>>> GetSeasonsByDivision(Guid divisionId)
    {
        Result<IEnumerable<FootballSeasonDto>> result =
            await _mediator.Send(new GetFootballSeasonsByDivisionQuery(divisionId));
        return HandleListResult(result, "Football seasons retrieved successfully", "Failed to retrieve football seasons");
    }

    /// <summary>
    /// Create season
    /// </summary>
    [HttpPost]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballSeasonDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballSeasonDto>>> CreateSeason([FromBody] CreateFootballSeasonRequest request)
    {
        if (!DateTime.TryParse(request.StartDate, out DateTime startDate) || !DateTime.TryParse(request.EndDate, out DateTime endDate))
        {
            return BadRequest(ApiResponse<FootballSeasonDto>.ErrorResponse("Invalid date format"));
        }

        CreateFootballSeasonCommand command = new(
            request.Name,
            request.DivisionIds,
            startDate,
            endDate,
            request.NumberOfHalves,
            request.HalfDurationMinutes,
            request.PlayersOnField,
            request.RequireGoalkeeper,
            request.MaxSubstitutions,
            request.RequireOfficialsToStart,
            request.AllowExtraTime,
            request.ExtraTimeHalfCount,
            request.ExtraTimeHalfDurationMinutes,
            request.AllowPenaltyShootout,
            request.WinPoints,
            request.DrawPoints,
            request.LossPoints,
            request.TeamCategory);

        Result<FootballSeasonDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetSeasonById),
                new { id = result.Data.Id },
                ApiResponse<FootballSeasonDto>.SuccessResponse(result.Data, "Football season created successfully"));
        }

        return ToErrorResponse(result, "Failed to create football season");
    }

    /// <summary>
    /// Update season
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballSeasonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballSeasonDto>>> UpdateSeason(Guid id, [FromBody] UpdateFootballSeasonRequest request)
    {
        if (!DateTime.TryParse(request.StartDate, out DateTime startDate) || !DateTime.TryParse(request.EndDate, out DateTime endDate))
        {
            return BadRequest(ApiResponse<FootballSeasonDto>.ErrorResponse("Invalid date format"));
        }

        UpdateFootballSeasonCommand command = new(
            id,
            request.Name,
            startDate,
            endDate,
            request.NumberOfHalves,
            request.HalfDurationMinutes,
            request.PlayersOnField,
            request.RequireGoalkeeper,
            request.MaxSubstitutions,
            request.RequireOfficialsToStart,
            request.AllowExtraTime,
            request.ExtraTimeHalfCount,
            request.ExtraTimeHalfDurationMinutes,
            request.AllowPenaltyShootout,
            request.WinPoints,
            request.DrawPoints,
            request.LossPoints,
            request.TeamCategory);

        Result<FootballSeasonDto> result = await _mediator.Send(command);
        return HandleResult(result, "Football season updated successfully", "Failed to update football season");
    }

    /// <summary>
    /// Activate season
    /// </summary>
    [HttpPut("{id:guid}/activate")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballSeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballSeasonDto>>> ActivateSeason(Guid id)
    {
        Result<FootballSeasonDto> result = await _mediator.Send(new ActivateFootballSeasonCommand(id));
        return HandleResult(result, "Football season activated successfully", "Failed to activate football season");
    }

    /// <summary>
    /// Deactivate season
    /// </summary>
    [HttpPut("{id:guid}/deactivate")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballSeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballSeasonDto>>> DeactivateSeason(Guid id)
    {
        Result<FootballSeasonDto> result = await _mediator.Send(new DeactivateFootballSeasonCommand(id));
        return HandleResult(result, "Football season deactivated successfully", "Failed to deactivate football season");
    }

    /// <summary>
    /// Complete season
    /// </summary>
    [HttpPut("{id:guid}/complete")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballSeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballSeasonDto>>> CompleteSeason(Guid id)
    {
        Result<FootballSeasonDto> result = await _mediator.Send(new CompleteFootballSeasonCommand(id));
        return HandleResult(result, "Football season completed successfully", "Failed to complete football season");
    }

    /// <summary>
    /// Add team to season
    /// </summary>
    [Obsolete("Use AddTeamToSeasonDivision instead to assign teams to a specific division within the season.")]
    [HttpPost("{competitionId:guid}/teams/{teamId:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballSeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballSeasonDto>>> AddTeamToSeason(Guid competitionId, Guid teamId)
    {
        Result<FootballSeasonDto> result = await _mediator.Send(new AddTeamToSeasonCommand(competitionId, teamId));
        return HandleResult(result, "Team added to football season successfully", "Failed to add team to football season");
    }

    /// <summary>
    /// Remove team from season
    /// </summary>
    [HttpDelete("{competitionId:guid}/teams/{teamId:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballSeasonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballSeasonDto>>> RemoveTeamFromSeason(Guid competitionId, Guid teamId)
    {
        Result<FootballSeasonDto> result = await _mediator.Send(new RemoveTeamFromSeasonCommand(competitionId, teamId));
        return HandleResult(result, "Team removed from football season successfully", "Failed to remove team from football season");
    }

    /// <summary>
    /// Add division to season
    /// </summary>
    [HttpPost("{competitionId:guid}/divisions/{divisionId:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> AddDivisionToSeason(Guid competitionId, Guid divisionId)
    {
        Result result = await _mediator.Send(new AddDivisionToSeasonCommand(competitionId, divisionId));
        return HandleVoidResult(result, "Division added to football season successfully", "Failed to add division to season");
    }

    /// <summary>
    /// Remove division from season
    /// </summary>
    [HttpDelete("{competitionId:guid}/divisions/{divisionId:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> RemoveDivisionFromSeason(Guid competitionId, Guid divisionId)
    {
        Result result = await _mediator.Send(new RemoveDivisionFromSeasonCommand(competitionId, divisionId));
        return HandleVoidResult(result, "Division removed from football season successfully", "Failed to remove division from season");
    }

    /// <summary>
    /// Add team to season division
    /// </summary>
    [HttpPost("{competitionId:guid}/divisions/{divisionId:guid}/teams/{teamId:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> AddTeamToSeasonDivision(Guid competitionId, Guid divisionId, Guid teamId)
    {
        Result result = await _mediator.Send(new AddTeamToSeasonDivisionCommand(competitionId, divisionId, teamId));
        return HandleVoidResult(result, "Team added to season division successfully", "Failed to add team to season division");
    }

    /// <summary>
    /// Remove team from season division
    /// </summary>
    [HttpDelete("{competitionId:guid}/divisions/{divisionId:guid}/teams/{teamId:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> RemoveTeamFromSeasonDivision(Guid competitionId, Guid divisionId, Guid teamId)
    {
        Result result = await _mediator.Send(new RemoveTeamFromSeasonDivisionCommand(competitionId, divisionId, teamId));
        return HandleVoidResult(result, "Team removed from season division successfully", "Failed to remove team from season division");
    }

    /// <summary>
    /// Delete season
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> DeleteSeason(Guid id)
    {
        Result result = await _mediator.Send(new DeleteFootballSeasonCommand(id));
        return HandleVoidResult(result, "Football season deleted successfully", "Failed to delete football season");
    }
}
