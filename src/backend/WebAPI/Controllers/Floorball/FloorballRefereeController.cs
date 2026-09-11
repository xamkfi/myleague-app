using Domain.Constants;
using Application.Common;
using Application.Features.Floorball.Referees.Commands;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.Referees.Queries;
using Domain.Common;
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
    /// Controller for managing floorball referees
    /// </summary>
    [Route("api/[controller]")]
    public class FloorballRefereeController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FloorballRefereeController> _logger;

        /// <summary>
        /// Initializes new instance of FloorballRefereeController class
        /// </summary>
        /// <param name="mediator">Mediator instance for handling commands and queries</param>
        /// <param name="logger">Logger instance for logging</param>
        public FloorballRefereeController(IMediator mediator, ILogger<FloorballRefereeController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new floorball referee
        /// </summary>
        /// <param name="request">Create referee request</param>
        /// <returns>Created referee details</returns>
        [HttpPost]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<FloorballRefereeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballRefereeDto>>> CreateReferee([FromBody] CreateFloorballRefereeRequest request)
        {
            _logger.LogInformation("Creating floorball referee for person ID: {personId}", request.PersonId);

            if (!DateTime.TryParse(request.LicenseIssueDate, out DateTime licenseIssueDateUtc))
                return BadRequest(ApiResponse<FloorballRefereeDto>.ErrorResponse("License issue date must be a valid date (e.g., 2020-05-05, 10-10-1990, 2020.10.25)"));

            if (!DateTime.TryParse(request.LicenseExpiryDate, out DateTime licenseExpiryDateUtc))
                return BadRequest(ApiResponse<FloorballRefereeDto>.ErrorResponse("License expiry date must be a valid date (e.g., 2030-05-05, 10-10-2030, 2030.10.25)"));

            if (licenseExpiryDateUtc <= licenseIssueDateUtc)
                return BadRequest(ApiResponse<FloorballRefereeDto>.ErrorResponse("License expiry date must be after the issue date"));

            CreateFloorballRefereeCommand command = new CreateFloorballRefereeCommand(
                request.PersonId,
                licenseIssueDateUtc,
                licenseExpiryDateUtc);

            Result<FloorballRefereeDto> result = await _mediator.Send(command);

            return HandleResult(result, "Floorball referee created successfully", "Failed to create floorball referee");
        }

        /// <summary>
        /// Updates an existing floorball referee
        /// </summary>
        /// <param name="id">Referee ID</param>
        /// <param name="request">Update referee request</param>
        /// <returns>Updated referee details</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<FloorballRefereeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballRefereeDto>>> UpdateReferee(Guid id, [FromBody] UpdateFloorballRefereeRequest request)
        {
            _logger.LogInformation("Updating floorball referee with ID: {id}", id);

            DateTime? licenseIssueDateUtc = null;
            DateTime? licenseExpiryDateUtc = null;

            if (!string.IsNullOrEmpty(request.LicenseIssueDate))
            {
                if (!DateTime.TryParse(request.LicenseIssueDate, out DateTime parsedIssueDate))
                    return BadRequest(ApiResponse<FloorballRefereeDto>.ErrorResponse("License issue date must be a valid date (e.g., 2020-05-05, 10-10-1990, 2020.10.25)"));
                licenseIssueDateUtc = DateTime.SpecifyKind(parsedIssueDate, DateTimeKind.Utc);
            }

            if (!string.IsNullOrEmpty(request.LicenseExpiryDate))
            {
                if (!DateTime.TryParse(request.LicenseExpiryDate, out DateTime parsedExpiryDate))
                    return BadRequest(ApiResponse<FloorballRefereeDto>.ErrorResponse("License expiry date must be a valid date (e.g., 2030-05-05, 10-10-2030, 2030.10.25)"));
                licenseExpiryDateUtc = DateTime.SpecifyKind(parsedExpiryDate, DateTimeKind.Utc);
            }

            if (licenseIssueDateUtc.HasValue && licenseExpiryDateUtc.HasValue && licenseExpiryDateUtc <= licenseIssueDateUtc)
                return BadRequest(ApiResponse<FloorballRefereeDto>.ErrorResponse("License expiry date must be after the issue date"));

            UpdateFloorballRefereeCommand command = new UpdateFloorballRefereeCommand(
                id,
                licenseIssueDateUtc,
                licenseExpiryDateUtc,
                request.MatchesOfficiated,
                request.IsActive);

            Result<FloorballRefereeDto> result = await _mediator.Send(command);

            return HandleResult(result, "Floorball referee updated successfully", "Failed to update floorball referee");
        }

        /// <summary>
        /// Deletes a floorball referee
        /// </summary>
        /// <param name="id">Referee ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteReferee(Guid id)
        {
            _logger.LogInformation("Deleting floorball referee with ID: {id}", id);

            Result<FloorballRefereeDto> result = await _mediator.Send(new DeleteFloorballRefereeCommand(id));

            return HandleVoidResult(result, "Floorball referee deleted successfully", "Failed to delete floorball referee");
        }

        /// <summary>
        /// Gets all floorball referees with pagination and filtering support
        /// </summary>
        /// <param name="page">Page number (1-based, default: 1)</param>
        /// <param name="pageSize">Page size (0 = use default, default: 0)</param>
        /// <param name="isActive">Filter by active status (null = all, true = active only, false = inactive only)</param>
        /// <param name="searchTerm">Search term for referee names</param>
        /// <param name="licenseExpiringWithinDays">Filter for referees with license expiring within specified days</param>
        /// <returns>Paginated list of floorball referees</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballRefereeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballRefereeDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballRefereeDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FloorballRefereeDto>>> GetAllReferees(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 0,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? licenseExpiringWithinDays = null)
        {
            _logger.LogInformation(
                "Getting all floorball referees - Page: {Page}, PageSize: {PageSize}, IsActive: {IsActive}, SearchTerm: {SearchTerm}, LicenseExpiringWithinDays: {LicenseExpiringWithinDays}",
                page,
                pageSize,
                isActive,
                SanitizeForLog(searchTerm),
                licenseExpiringWithinDays);

            Result<PagedResult<FloorballRefereeDto>> result = await _mediator.Send(
                new GetAllFloorballRefereesQuery(page, pageSize, isActive, searchTerm, licenseExpiringWithinDays));

            return HandlePaginatedResult(result, "Floorball referees retrieved successfully", "Failed to retrieve floorball referees");
        }

        /// <summary>
        /// Gets a specific floorball referee by ID
        /// </summary>
        /// <param name="id">The referee ID</param>
        /// <returns>The floorball referee details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballRefereeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballRefereeDto>>> GetRefereeById(Guid id)
        {
            _logger.LogInformation("Getting floorball referee with ID: {RefereeId}", id);

            Result<FloorballRefereeDto> result = await _mediator.Send(new GetFloorballRefereeByIdQuery(id));

            return HandleResult(result, "Floorball referee retrieved successfully", "Failed to retrieve floorball referee");
        }
    }
}
