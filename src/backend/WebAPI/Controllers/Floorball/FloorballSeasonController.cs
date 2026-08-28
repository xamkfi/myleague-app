using Domain.Constants;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Application.Common;
using Application.Features.Floorball.Seasons.Commands;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Seasons.Queries;
using Domain.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using WebAPI.Models.Floorball;

namespace WebAPI.Controllers.Floorball
{
    /// <summary>
    /// Controller for managing floorball seasons
    /// </summary>
    [Route("api/[controller]")]
    public class FloorballSeasonController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FloorballSeasonController> _logger;

        /// <summary>
        /// Initializes new instance of FloorballSeasonController class
        /// </summary>
        /// <param name="mediator">Mediator instance for handling commands and queries</param>
        /// <param name="logger">Logger instance for logging</param>
        public FloorballSeasonController(IMediator mediator, ILogger<FloorballSeasonController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Gets all floorball seasons
        /// </summary>
        /// <returns>List of all floorball seasons</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballSeasonDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballSeasonDto>>>> GetAllSeasons()
        {
            _logger.LogInformation("Getting all floorball seasons");

            GetAllFloorballSeasonsQuery query = new GetAllFloorballSeasonsQuery();
            Result<IEnumerable<FloorballSeasonDto>> result = await _mediator.Send(query);

            return HandleListResult(result, "Floorball seasons retrieved successfully", "Failed to retrieve floorball seasons");
        }

        /// <summary>
        /// Gets distinct season years for public year navigation.
        /// </summary>
        [HttpGet("years")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballSeasonYearDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballSeasonYearDto>>>> GetSeasonYears()
        {
            _logger.LogInformation("Getting floorball season years");

            Result<IEnumerable<FloorballSeasonYearDto>> result =
                await _mediator.Send(new GetFloorballSeasonYearsQuery());

            return HandleListResult(result, "Floorball season years retrieved successfully", "Failed to retrieve floorball season years");
        }

        /// <summary>
        /// Gets a paginated slim list of floorball seasons (optional season-year filter).
        /// </summary>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballSeasonSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballSeasonSummaryDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballSeasonSummaryDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FloorballSeasonSummaryDto>>> GetSeasonsPaged(
            [FromQuery] GetFloorballSeasonsPagedRequest request)
        {
            _logger.LogInformation(
                "Getting paged floorball seasons - Page: {Page}, PageSize: {PageSize}, SeasonYear: {SeasonYear}",
                request.Page,
                request.PageSize,
                FormatSeasonYearForLog(request.SeasonYear));

            GetFloorballSeasonsPagedQuery query = new GetFloorballSeasonsPagedQuery(
                request.Page,
                request.PageSize,
                request.SeasonYear,
                request.TeamCategory);

            Result<PagedResult<FloorballSeasonSummaryDto>> result = await _mediator.Send(query);

            return HandlePaginatedResult(result, "Floorball seasons retrieved successfully", "Failed to retrieve floorball seasons");
        }

        /// <summary>
        /// Gets all active floorball seasons
        /// </summary>
        /// <returns>List of active floorball seasons</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballSeasonDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballSeasonDto>>>> GetActiveSeasons()
        {
            _logger.LogInformation("Getting active floorball seasons");

            GetActiveFloorballSeasonsQuery query = new GetActiveFloorballSeasonsQuery();
            Result<IEnumerable<FloorballSeasonDto>> result = await _mediator.Send(query);

            return HandleListResult(result, "Active floorball seasons retrieved successfully", "Failed to retrieve active floorball seasons");
        }

        /// <summary>
        /// Gets a floorball season by ID
        /// </summary>
        /// <param name="id">Season ID</param>
        /// <returns>Season details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonDto>>> GetSeasonById(Guid id)
        {
            _logger.LogInformation("Getting floorball season with ID: {id}", id);

            GetFloorballSeasonByIdQuery query = new GetFloorballSeasonByIdQuery(id);
            Result<FloorballSeasonDto> result = await _mediator.Send(query);

            return HandleResult(result, "Floorball season retrieved successfully", "Failed to retrieve floorball season");
        }

        /// <summary>
        /// Gets intro blocks for the featured season of an optional year.
        /// </summary>
        [HttpGet("content-blocks")]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonContentBlocksDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonContentBlocksDto>>> GetFeaturedContentBlocks(
            [FromQuery] string? seasonYear)
        {
            _logger.LogInformation(
                "Getting featured floorball season content blocks - SeasonYear: {SeasonYear}",
                FormatSeasonYearForLog(seasonYear));

            Result<FloorballSeasonContentBlocksDto> result =
                await _mediator.Send(new GetFloorballSeasonContentBlocksByYearQuery(seasonYear));

            return HandleResult(result, "Season content blocks retrieved successfully", "Failed to retrieve season content blocks");
        }

        /// <summary>
        /// Gets intro blocks for a floorball season.
        /// </summary>
        [HttpGet("{id:guid}/content-blocks")]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonContentBlocksDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonContentBlocksDto>>> GetContentBlocks(Guid id)
        {
            _logger.LogInformation("Getting floorball season content blocks for {id}", id);

            Result<FloorballSeasonContentBlocksDto> result =
                await _mediator.Send(new GetFloorballSeasonContentBlocksQuery(id));

            return HandleResult(result, "Season content blocks retrieved successfully", "Season not found");
        }

        /// <summary>
        /// Replaces intro blocks for a floorball season. Array order is the display order.
        /// </summary>
        [HttpPut("{id:guid}/content-blocks")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonContentBlocksDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonContentBlocksDto>>> ReplaceContentBlocks(
            Guid id,
            [FromBody] ReplaceFloorballSeasonContentBlocksRequest request)
        {
            _logger.LogInformation("Replacing floorball season content blocks for {id}", id);

            ReplaceFloorballSeasonContentBlocksCommand command = new(
                id,
                request.Items
                    .Select(item => new ReplaceFloorballSeasonContentBlockItem(item.Id, item.Title, item.ContentHtml))
                    .ToList());

            Result<FloorballSeasonContentBlocksDto> result = await _mediator.Send(command);
            return HandleResult(result, "Season content blocks updated successfully", "Failed to update season content blocks");
        }

        /// <summary>
        /// Gets floorball seasons by division
        /// </summary>
        /// <param name="divisionId">Division</param>
        /// <returns>List of seasons for the division</returns>
        [HttpGet("by-division/{division}")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballSeasonDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballSeasonDto>>>> GetSeasonsByDivision(Guid divisionId)
        {
            _logger.LogInformation("Getting floorball seasons for division: {division}", divisionId);

            GetFloorballSeasonsByDivisionQuery query = new GetFloorballSeasonsByDivisionQuery(divisionId);
            Result<IEnumerable<FloorballSeasonDto>> result = await _mediator.Send(query);

            return HandleListResult(result, "Floorball seasons retrieved successfully", "Failed to retrieve floorball seasons");
        }

        /// <summary>
        /// Creates a new floorball season
        /// </summary>
        /// <param name="request">Create season request</param>
        /// <returns>Created season details</returns>
        [HttpPost]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonDto>>> CreateSeason([FromBody] CreateFloorballSeasonRequest request)
        {
            _logger.LogInformation("Creating floorball season: {name}", SanitizeForLog(request.Name));

            if (!DateTime.TryParse(request.StartDate, out DateTime startDate) || !DateTime.TryParse(request.EndDate, out DateTime endDate))
            {
                return BadRequest(ApiResponse<FloorballSeasonDto>.ErrorResponse("Invalid date format"));
            }

            CreateFloorballSeasonCommand command = new CreateFloorballSeasonCommand(
                request.Name,
                request.DivisionIds,
                startDate,
                endDate,
                request.NumberOfPeriods,
                request.PeriodDurationMinutes,
                request.AllowOvertime,
                request.OvertimeDurationMinutes,
                request.AllowShootout,
                request.TeamCategory
            );

            Result<FloorballSeasonDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetSeasonById),
                    new { id = result.Data.Id },
                    ApiResponse<FloorballSeasonDto>.SuccessResponse(result.Data, "Floorball season created successfully")
                );
            }

            return ToErrorResponse(result, "Failed to create floorball season");
        }

        /// <summary>
        /// Updates an existing floorball season
        /// </summary>
        /// <param name="id">Season ID</param>
        /// <param name="request">Update season request</param>
        /// <returns>Updated season details</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonDto>>> UpdateSeason(Guid id, [FromBody] UpdateFloorballSeasonRequest request)
        {
            _logger.LogInformation("Updating floorball season with ID: {id}", id);

            if (!DateTime.TryParse(request.StartDate, out DateTime startDate) || !DateTime.TryParse(request.EndDate, out DateTime endDate))
            {
                return BadRequest(ApiResponse<FloorballSeasonDto>.ErrorResponse("Invalid date format"));
            }

            UpdateFloorballSeasonCommand command = new UpdateFloorballSeasonCommand(
                id,
                request.Name,
                startDate,
                endDate,
                request.NumberOfPeriods,
                request.PeriodDurationMinutes,
                request.AllowOvertime,
                request.OvertimeDurationMinutes,
                request.AllowShootout,
                request.TeamCategory
            );

            Result<FloorballSeasonDto> result = await _mediator.Send(command);

            return HandleResult(result, "Floorball season updated successfully", "Failed to update floorball season");
        }

        /// <summary>
        /// Activates a floorball season
        /// </summary>
        /// <param name="id">Season ID</param>
        /// <returns>Activated season details</returns>
        [HttpPut("{id:guid}/activate")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonDto>>> ActivateSeason(Guid id)
        {
            _logger.LogInformation("Activating floorball season with ID: {id}", id);

            ActivateFloorballSeasonCommand command = new ActivateFloorballSeasonCommand(id);
            Result<FloorballSeasonDto> result = await _mediator.Send(command);

            return HandleResult(result, "Floorball season activated successfully", "Failed to activate floorball season");
        }

        /// <summary>
        /// Deactivates a floorball season
        /// </summary>
        /// <param name="id">Season ID</param>
        /// <returns>Deactivated season details</returns>
        [HttpPut("{id:guid}/deactivate")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonDto>>> DeactivateSeason(Guid id)
        {
            _logger.LogInformation("Deactivating floorball season with ID: {id}", id);

            DeactivateFloorballSeasonCommand command = new DeactivateFloorballSeasonCommand(id);
            Result<FloorballSeasonDto> result = await _mediator.Send(command);

            return HandleResult(result, "Floorball season deactivated successfully", "Failed to deactivate floorball season");
        }

        /// <summary>
        /// Completes a floorball season
        /// </summary>
        /// <param name="id">Season ID</param>
        /// <returns>Completed season details</returns>
        [HttpPut("{id:guid}/complete")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonDto>>> CompleteSeason(Guid id)
        {
            _logger.LogInformation("Completing floorball season with ID: {id}", id);

            CompleteFloorballSeasonCommand command = new CompleteFloorballSeasonCommand(id);
            Result<FloorballSeasonDto> result = await _mediator.Send(command);

            return HandleResult(result, "Floorball season completed successfully", "Failed to complete floorball season");
        }

        /// <summary>
        /// Adds a team to a floorball season without assigning it to a division.
        /// Use POST {competitionId}/divisions/{divisionId}/teams/{teamId} instead to also assign the team to a division.
        /// </summary>
        /// <param name="competitionId">Season ID</param>
        /// <param name="teamId">Team ID</param>
        /// <returns>Updated season details</returns>
        [Obsolete("Use AddTeamToSeasonDivision instead to assign teams to a specific division within the season.")]
        [HttpPost("{competitionId:guid}/teams/{teamId:guid}")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonDto>>> AddTeamToSeason(Guid competitionId, Guid teamId)
        {
            _logger.LogInformation("Adding team {teamId} to floorball season with ID: {id}", teamId, competitionId);

            AddTeamToSeasonCommand command = new AddTeamToSeasonCommand(competitionId, teamId);
            Result<FloorballSeasonDto> result = await _mediator.Send(command);

            return HandleResult(result, "Team added to floorball season successfully", "Failed to add team to floorball season");
        }

        /// <summary>
        /// Removes a team from a floorball season
        /// </summary>
        /// <param name="competitionId">Season ID</param>
        /// <param name="teamId">Team ID</param>
        /// <returns>Updated season details</returns>
        [HttpDelete("{competitionId:guid}/teams/{teamId:guid}")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonDto>>> RemoveTeamFromSeason(Guid competitionId, Guid teamId)
        {
            _logger.LogInformation("Removing team {teamId} from floorball season with ID: {id}", teamId, competitionId);

            RemoveTeamFromSeasonCommand command = new RemoveTeamFromSeasonCommand(competitionId, teamId);
            Result<FloorballSeasonDto> result = await _mediator.Send(command);

            return HandleResult(result, "Team removed from floorball season successfully", "Failed to remove team from floorball season");
        }

        /// <summary>
        /// Adds a division to a floorball season
        /// </summary>
        /// <param name="competitionId">Season ID</param>
        /// <param name="divisionId">Division ID</param>
        [HttpPost("{competitionId:guid}/divisions/{divisionId:guid}")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> AddDivisionToSeason(Guid competitionId, Guid divisionId)
        {
            _logger.LogInformation("Adding division {divisionId} to floorball season with ID: {id}", divisionId, competitionId);
            AddDivisionToSeasonCommand command = new AddDivisionToSeasonCommand(competitionId, divisionId);
            Result result = await _mediator.Send(command);

            return HandleVoidResult(result, "Division added to floorball season successfully", "Failed to add division to season");
        }

        /// <summary>
        /// Removes a division from a floorball season
        /// </summary>
        /// <param name="competitionId">Season ID</param>
        /// <param name="divisionId">Division ID</param>
        [HttpDelete("{competitionId:guid}/divisions/{divisionId:guid}")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> RemoveDivisionFromSeason(Guid competitionId, Guid divisionId)
        {
            _logger.LogInformation("Removing division {divisionId} from floorball season with ID: {id}", divisionId, competitionId);
            RemoveDivisionFromSeasonCommand command = new RemoveDivisionFromSeasonCommand(competitionId, divisionId);
            Result result = await _mediator.Send(command);

            return HandleVoidResult(result, "Division removed from floorball season successfully", "Failed to remove division from season");
        }

        /// <summary>
        /// Adds a team to a specific division of a floorball season
        /// </summary>
        /// <param name="competitionId">Season ID</param>
        /// <param name="divisionId">Division ID</param>
        /// <param name="teamId">Team ID</param>
        [HttpPost("{competitionId:guid}/divisions/{divisionId:guid}/teams/{teamId:guid}")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> AddTeamToSeasonDivision(Guid competitionId, Guid divisionId, Guid teamId)
        {
            _logger.LogInformation("Adding team {teamId} to season {id} division {divisionId}", teamId, competitionId, divisionId);
            AddTeamToSeasonDivisionCommand command = new AddTeamToSeasonDivisionCommand(competitionId, divisionId, teamId);
            Result result = await _mediator.Send(command);

            return HandleVoidResult(result, "Team added to season division successfully", "Failed to add team to season division");
        }

        /// <summary>
        /// Removes a team from a specific division of a floorball season
        /// </summary>
        /// <param name="competitionId">Season ID</param>
        /// <param name="divisionId">Division ID</param>
        /// <param name="teamId">Team ID</param>
        [HttpDelete("{competitionId:guid}/divisions/{divisionId:guid}/teams/{teamId:guid}")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> RemoveTeamFromSeasonDivision(Guid competitionId, Guid divisionId, Guid teamId)
        {
            _logger.LogInformation("Removing team {teamId} from season {id} division {divisionId}", teamId, competitionId, divisionId);
            RemoveTeamFromSeasonDivisionCommand command = new RemoveTeamFromSeasonDivisionCommand(competitionId, divisionId, teamId);
            Result result = await _mediator.Send(command);

            return HandleVoidResult(result, "Team removed from season division successfully", "Failed to remove team from season division");
        }

        /// <summary>
        /// Deletes a floorball season
        /// </summary>
        /// <param name="id">Season ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteSeason(Guid id)
        {
            _logger.LogInformation("Deleting floorball season with ID: {id}", id);

            DeleteFloorballSeasonCommand command = new DeleteFloorballSeasonCommand(id);
            Result result = await _mediator.Send(command);

            return HandleVoidResult(result, "Floorball season deleted successfully", "Failed to delete floorball season");
        }

        /// <summary>
        /// Rebuilds a season-year label from parsed integers so user-controlled query text is never written to logs.
        /// </summary>
        private static string FormatSeasonYearForLog(string? seasonYear)
        {
            if (string.IsNullOrWhiteSpace(seasonYear))
            {
                return "all";
            }

            if (!FloorballSeasonYear.TryParse(seasonYear, out int startYear, out int endYear))
            {
                return "invalid";
            }

            return startYear == endYear
                ? startYear.ToString(CultureInfo.InvariantCulture)
                : string.Create(CultureInfo.InvariantCulture, $"{startYear}-{endYear}");
        }
    }
}
