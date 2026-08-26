using Domain.Constants;
using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.FooterContacts.Commands;
using Application.Features.Common.FooterContacts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common;

/// <summary>
/// Controller for public footer contact entries.
/// </summary>
[Route("api/[controller]")]
public class FooterContactController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<FooterContactController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FooterContactController"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR mediator.</param>
    /// <param name="logger">The logger.</param>
    public FooterContactController(
        IMediator mediator,
        ILogger<FooterContactController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all footer contacts, ordered for display.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FooterContactDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FooterContactDto>>>> GetAll()
    {
        _logger.LogInformation("Getting footer contacts");
        Result<IReadOnlyList<FooterContactDto>> result = await _mediator.Send(new GetAllFooterContactsQuery());
        return HandleResult(result, "Footer contacts retrieved successfully", "Failed to retrieve footer contacts");
    }

    /// <summary>
    /// Get a footer contact by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<FooterContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FooterContactDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FooterContactDto>>> GetById(Guid id)
    {
        _logger.LogInformation("Getting footer contact {ContactId}", id);
        Result<FooterContactDto> result = await _mediator.Send(new GetFooterContactByIdQuery(id));
        return HandleResult(result, "Footer contact retrieved successfully", "Footer contact not found");
    }

    /// <summary>
    /// Create a footer contact.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FooterContactDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<FooterContactDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<FooterContactDto>>> Create(
        [FromBody] CreateFooterContactRequest request)
    {
        _logger.LogInformation("Creating footer contact {Title}", SanitizeForLog(request.Title));

        CreateFooterContactCommand command = new(
            request.Title,
            request.Details,
            request.Email,
            request.Phone,
            request.Url,
            request.SortOrder,
            User?.Identity?.Name);

        Result<FooterContactDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data.Id },
                ApiResponse<FooterContactDto>.SuccessResponse(result.Data, "Footer contact created successfully"));
        }

        return ToErrorResponse(result, "Failed to create footer contact");
    }

    /// <summary>
    /// Update a footer contact.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FooterContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FooterContactDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<FooterContactDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<FooterContactDto>>> Update(
        Guid id,
        [FromBody] UpdateFooterContactRequest request)
    {
        _logger.LogInformation("Updating footer contact {ContactId}", id);

        UpdateFooterContactCommand command = new(
            id,
            request.Title,
            request.Details,
            request.Email,
            request.Phone,
            request.Url,
            request.SortOrder,
            User?.Identity?.Name);

        Result<FooterContactDto> result = await _mediator.Send(command);
        return HandleResult(result, "Footer contact updated successfully", "Failed to update footer contact");
    }

    /// <summary>
    /// Delete a footer contact.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        _logger.LogInformation("Deleting footer contact {ContactId}", id);
        Result<bool> result = await _mediator.Send(new DeleteFooterContactCommand(id));
        return HandleResult(result, "Footer contact deleted successfully", "Failed to delete footer contact");
    }
}
