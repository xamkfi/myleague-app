using Application.Common;
using Application.Features.Hockey.Officials.Commands;
using Application.Features.Hockey.Officials.DTOs;
using Application.Features.Hockey.Officials.Queries;
using Domain.Common;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using WebAPI.Models.Hockey;

namespace WebAPI.Controllers.Hockey;

/// <summary>
/// API endpoints for hockey official profiles.
/// </summary>
[Route("api/[controller]")]
public class HockeyOfficialController : BaseApiController
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Creates a new <see cref="HockeyOfficialController"/>.
    /// </summary>
    public HockeyOfficialController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lists hockey officials.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<HockeyOfficialDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<HockeyOfficialDto>>>> GetOfficials(
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        Result<IReadOnlyList<HockeyOfficialDto>> result =
            await _mediator.Send(new GetHockeyOfficialsQuery(isActive), cancellationToken);
        return HandleResult(result, "Hockey officials retrieved successfully", "Failed to retrieve hockey officials");
    }

    /// <summary>
    /// Gets paginated hockey officials.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PaginatedApiResponse<HockeyOfficialDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginatedApiResponse<HockeyOfficialDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedApiResponse<HockeyOfficialDto>>> GetPagedOfficials(
        [FromQuery] GetPagedHockeyOfficialsRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<PagedResult<HockeyOfficialDto>> result = await _mediator.Send(
            new GetPagedHockeyOfficialsQuery(
                request.Page,
                request.PageSize,
                request.IsActive,
                request.SearchTerm,
                request.LicenseExpiringWithinDays),
            cancellationToken);
        return HandlePaginatedResult(result, "Hockey officials retrieved successfully", "Failed to retrieve hockey officials");
    }

    /// <summary>
    /// Gets a hockey official by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyOfficialDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyOfficialDto>>> GetOfficialById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyOfficialDto> result =
            await _mediator.Send(new GetHockeyOfficialByIdQuery(id), cancellationToken);
        return HandleResult(result, "Hockey official retrieved successfully", "Hockey official not found");
    }

    /// <summary>
    /// Creates a new hockey official profile.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HockeyOfficialDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HockeyOfficialDto>>> CreateOfficial(
        [FromBody] CreateHockeyOfficialRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyOfficialDto> result = await _mediator.Send(new CreateHockeyOfficialCommand(
            request.PersonId,
            request.OfficialRole,
            request.OfficialNumber,
            request.LicenseIssueDate,
            request.LicenseExpiryDate), cancellationToken);

        if (result.IsSuccess && result.Data is not null)
        {
            return CreatedAtAction(
                nameof(GetOfficialById),
                new { id = result.Data.Id },
                ApiResponse<HockeyOfficialDto>.SuccessResponse(result.Data, "Hockey official created successfully"));
        }

        return HandleResult(result, "Hockey official created successfully", "Failed to create hockey official");
    }

    /// <summary>
    /// Updates an existing hockey official profile.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyOfficialDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyOfficialDto>>> UpdateOfficial(
        Guid id,
        [FromBody] UpdateHockeyOfficialRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyOfficialDto> result = await _mediator.Send(new UpdateHockeyOfficialCommand(
            id,
            request.OfficialRole,
            request.OfficialNumber,
            request.LicenseIssueDate,
            request.LicenseExpiryDate,
            request.IsActive), cancellationToken);

        return HandleResult(result, "Hockey official updated successfully", "Failed to update hockey official");
    }
}
