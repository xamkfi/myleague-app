using Domain.Constants;
using Application.Common;
using Application.Features.Football.Referees.Commands;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.Referees.Queries;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using WebAPI.Models.Football;

namespace WebAPI.Controllers.Football
{
    /// <summary>
    /// Controller for managing football referees
    /// </summary>
    [Route("api/[controller]")]
    public class FootballRefereeController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FootballRefereeController> _logger;

        /// <summary>
        /// Initializes new instance of FootballRefereeController class
        /// </summary>
        /// <param name="mediator">Mediator instance for handling commands and queries</param>
        /// <param name="logger">Logger instance for logging</param>
        public FootballRefereeController(IMediator mediator, ILogger<FootballRefereeController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new football referee
        /// </summary>
        /// <param name="request">Create referee request</param>
        /// <returns>Created referee details</returns>
        [HttpPost]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<FootballRefereeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FootballRefereeDto>>> CreateReferee([FromBody] CreateFootballRefereeRequest request)
        {
            _logger.LogInformation("Creating football referee for person ID: {personId}", request.PersonId);

            if (!DateTime.TryParse(request.LicenseIssueDate, out DateTime licenseIssueDateUtc))
                return BadRequest(ApiResponse<FootballRefereeDto>.ErrorResponse("License issue date must be a valid date (e.g., 2020-05-05, 10-10-1990, 2020.10.25)"));

            if (!DateTime.TryParse(request.LicenseExpiryDate, out DateTime licenseExpiryDateUtc))
                return BadRequest(ApiResponse<FootballRefereeDto>.ErrorResponse("License expiry date must be a valid date (e.g., 2030-05-05, 10-10-2030, 2030.10.25)"));

            if (licenseExpiryDateUtc <= licenseIssueDateUtc)
                return BadRequest(ApiResponse<FootballRefereeDto>.ErrorResponse("License expiry date must be after the issue date"));

            CreateFootballRefereeCommand command = new CreateFootballRefereeCommand(
                request.PersonId,
                licenseIssueDateUtc,
                licenseExpiryDateUtc);

            Result<FootballRefereeDto> result = await _mediator.Send(command);

            return HandleResult(result, "Football referee created successfully", "Failed to create football referee");
        }

        /// <summary>
        /// Updates an existing football referee
        /// </summary>
        /// <param name="id">Referee ID</param>
        /// <param name="request">Update referee request</param>
        /// <returns>Updated referee details</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<FootballRefereeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FootballRefereeDto>>> UpdateReferee(Guid id, [FromBody] UpdateFootballRefereeRequest request)
        {
            _logger.LogInformation("Updating football referee with ID: {id}", id);

            DateTime? licenseIssueDateUtc = null;
            DateTime? licenseExpiryDateUtc = null;

            if (!string.IsNullOrEmpty(request.LicenseIssueDate))
            {
                if (!DateTime.TryParse(request.LicenseIssueDate, out DateTime parsedIssueDate))
                    return BadRequest(ApiResponse<FootballRefereeDto>.ErrorResponse("License issue date must be a valid date (e.g., 2020-05-05, 10-10-1990, 2020.10.25)"));
                licenseIssueDateUtc = DateTime.SpecifyKind(parsedIssueDate, DateTimeKind.Utc);
            }

            if (!string.IsNullOrEmpty(request.LicenseExpiryDate))
            {
                if (!DateTime.TryParse(request.LicenseExpiryDate, out DateTime parsedExpiryDate))
                    return BadRequest(ApiResponse<FootballRefereeDto>.ErrorResponse("License expiry date must be a valid date (e.g., 2030-05-05, 10-10-2030, 2030.10.25)"));
                licenseExpiryDateUtc = DateTime.SpecifyKind(parsedExpiryDate, DateTimeKind.Utc);
            }

            if (licenseIssueDateUtc.HasValue && licenseExpiryDateUtc.HasValue && licenseExpiryDateUtc <= licenseIssueDateUtc)
                return BadRequest(ApiResponse<FootballRefereeDto>.ErrorResponse("License expiry date must be after the issue date"));

            UpdateFootballRefereeCommand command = new UpdateFootballRefereeCommand(
                id,
                licenseIssueDateUtc,
                licenseExpiryDateUtc,
                request.MatchesOfficiated,
                request.IsActive);

            Result<FootballRefereeDto> result = await _mediator.Send(command);

            return HandleResult(result, "Football referee updated successfully", "Failed to update football referee");
        }

        /// <summary>
        /// Deletes a football referee
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
            _logger.LogInformation("Deleting football referee with ID: {id}", id);

            Result<FootballRefereeDto> result = await _mediator.Send(new DeleteFootballRefereeCommand(id));

            return HandleVoidResult(result, "Football referee deleted successfully", "Failed to delete football referee");
        }

        /// <summary>
        /// Gets all football referees with pagination and filtering support
        /// </summary>
        /// <param name="page">Page number (1-based, default: 1)</param>
        /// <param name="pageSize">Page size (0 = use default, default: 0)</param>
        /// <param name="isActive">Filter by active status (null = all, true = active only, false = inactive only)</param>
        /// <param name="searchTerm">Search term for referee names</param>
        /// <param name="licenseExpiringWithinDays">Filter for referees with license expiring within specified days</param>
        /// <returns>Paginated list of football referees</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedApiResponse<FootballRefereeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FootballRefereeDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FootballRefereeDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FootballRefereeDto>>> GetAllReferees(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 0,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? licenseExpiringWithinDays = null)
        {
            _logger.LogInformation(
                "Getting all football referees - Page: {Page}, PageSize: {PageSize}, IsActive: {IsActive}, SearchTerm: {SearchTerm}, LicenseExpiringWithinDays: {LicenseExpiringWithinDays}",
                page,
                pageSize,
                isActive,
                SanitizeForLog(searchTerm),
                licenseExpiringWithinDays);

            Result<PagedResult<FootballRefereeDto>> result = await _mediator.Send(
                new GetAllFootballRefereesQuery(page, pageSize, isActive, searchTerm, licenseExpiringWithinDays));

            return HandlePaginatedResult(result, "Football referees retrieved successfully", "Failed to retrieve football referees");
        }

        /// <summary>
        /// Gets a specific football referee by ID
        /// </summary>
        /// <param name="id">The referee ID</param>
        /// <returns>The football referee details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FootballRefereeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FootballRefereeDto>>> GetRefereeById(Guid id)
        {
            _logger.LogInformation("Getting football referee with ID: {RefereeId}", id);

            Result<FootballRefereeDto> result = await _mediator.Send(new GetFootballRefereeByIdQuery(id));

            return HandleResult(result, "Football referee retrieved successfully", "Failed to retrieve football referee");
        }
    }
}
